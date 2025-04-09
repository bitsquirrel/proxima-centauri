using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;

namespace Functions_for_Dynamics_Operations
{
    public class LabelCRUD
    {
        /// <summary>
        /// Create new labels in cache from the row just created by the user
        /// </summary>
        /// <param name="fileCollection">collection of label files in current model</param>
        /// <param name="label">current label file selected</param>
        /// <param name="id">label id currently being used</param>
        /// <param name="text">text of the label</param>
        /// <param name="description">description of the label</param>
        /// <param name="translate">indicates if translation must be done</param>
        /// <param name="noTransLang">no translation for the language specified</param>
        public static void CreateNewLabelsFromRowInCache(DLabelFileCollection fileCollection, DLabelFile label, string id, string text, string description, LangTranslate langTranslate, string noTransLang, string descriptionExisted, bool translate)
        {
            if (!fileCollection.Labels.ContainsKey(id))
            {
                fileCollection.Labels.Add(id, new Dictionary<string, Label>() { { label.Language, new Label(text, description, descriptionExisted) } });

                if (fileCollection.Labels.TryGetValue(id, out Dictionary<string, Label> labelValues))
                {
                    foreach (DLabelFile secondaryLabel in fileCollection.Files)
                    {
                        if (secondaryLabel.Language != label.Language)
                        {   // Translate to the other languages, bypassing the just created one
                            string translatedValue = text;
                            // Only translate if the setting is active
                            if (translate)
                            {
                                // Do not translate to the language being bypassed
                                if (secondaryLabel.Language != noTransLang)
                                    translatedValue = langTranslate.TranslateAzure(text, label.Language, secondaryLabel.Language);
                            }

                            labelValues.Add(secondaryLabel.Language, new Label(translatedValue, description, descriptionExisted));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create new secondary labels and populate the cache with these values
        /// </summary>
        /// <param name="fileCollection">collection of label files in current model</param>
        /// <param name="dlabel">current label file selected</param>
        /// <param name="translate">indicates if translation must be done</param>
        /// <param name="id">label id currently being used</param>
        /// <param name="noTranslateLanguage">no translation for the language specified</param>
        public static void CreateNewLabelsSecondaryInCache(DLabelFileCollection fileCollection, DLabelFile dlabel, LangTranslate translate, string id, string noTranslateLanguage)
        {
            KeyValuePair<string, Label> currentLanguageLabel = fileCollection.Labels[id].First(e => e.Key == dlabel.Language);
            Label label = currentLanguageLabel.Value;

            if (fileCollection.Labels.TryGetValue(id, out Dictionary<string, Label> labelValues))
            {   // contains the label id already 
                foreach (DLabelFile file in fileCollection.Files)
                {
                    if (file.Language != dlabel.Language)
                    {
                        if (!labelValues.ContainsKey(file.Language))
                        {   // Add the labels to the cache
                            string translatedValue = "";

                            if (file.Language != noTranslateLanguage)
                                translatedValue = translate.TranslateAzure(label.Text, currentLanguageLabel.Key, file.Language);

                            labelValues.Add(file.Language, new Label(translatedValue, label.Description, label.DescriptionExisted));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update on the primary label must therefore upudate the other labels
        /// </summary>
        /// <param name="fileCollection">Collection of label files found</param>
        /// <param name="dlabel">Current label file with specific language</param>
        /// <param name="translate">Translation engine</param>
        /// <param name="id">Id of the new label</param>
        public static void UpdateLabelSecondaryInCache(DLabelFileCollection fileCollection, DLabelFile dlabel, LangTranslate translate, string id)
        {
            if (fileCollection.Labels[id].TryGetValue(dlabel.Language, out Label label))
            {
                if (fileCollection.Labels.TryGetValue(id, out Dictionary<string, Label> labelValues))
                {   // contains the label id already 
                    foreach (DLabelFile file in fileCollection.Files)
                    {
                        if (file.Language != dlabel.Language)
                        {
                            Label labelToUpdate = labelValues.First(e => e.Key == file.Language).Value;

                            labelToUpdate.Text = translate.TranslateAzure(label.Text, dlabel.Language, file.Language);
                            labelToUpdate.Description = label.Description;
                        }
                    }
                }
            }
        }

        internal class Threader
        {
            public DLabelFile DLabelFile;

            public void DoWork()
            {
                DumpLabelFileAndCompile(DLabelFile);
            }
        }

        /// <summary>
        /// Build the string used to replace the current text in the physical label file
        /// </summary>
        /// <param name="fileCollection">Collection of label files found</param>
        public static void DumpLabelsToFilesToBuffMulti(DLabelFileCollection fileCollection, bool noDefaultDescription)
        {
            // Loop through the label ids to generate the buff for files
            GenerateBuff(fileCollection);

            List<Threader> threads = new List<Threader>();
            // Loop through the files to replace the content
            foreach (DLabelFile dLabelFile in fileCollection.Files)
            {
                threads.Add(new Threader() { DLabelFile = dLabelFile });
            }

            Parallel.ForEach(threads, new ParallelOptions() { MaxDegreeOfParallelism = 16 }, worker => worker.DoWork());
        }

        /// <summary>
        /// Build the string used to replace the current text in the physical label file
        /// </summary>
        /// <param name="fileCollection">Collection of label files found</param>
        public static void DumpLabelsToFilesToBuff(DLabelFileCollection fileCollection)
        {
            // Loop through the label ids to generate the buff for files
            GenerateBuff(fileCollection);

            // Loop through the files to replace the content
            foreach (DLabelFile labelFile in fileCollection.Files)
            {
                // Reset the content to what we have
                DumpLabelFile(labelFile);
            }
            // Compile the labels after dumping the file
            DynaxUtils.CompileLabels(fileCollection);
        }

        /// <summary>
        /// Dump a single file 
        /// </summary>
        /// <param name="fileCollection">Collection of label files found</param>
        /// <param name="label">Current label file with specific language</param>
        public static void DumpLabelFileSingleToBuff(DLabelFileCollection fileCollection, DLabelFile label)
        {
            // Loop through the label ids to generate the buff for files
            GenerateBuffSingle(fileCollection, label);

            // Reset the content to what we have
            DumpLabelFile(label);

            // Compile the labels after dumping the file 
            DynaxUtils.CompileLabels(fileCollection);
        }

        public static void GenerateBuff(DLabelFileCollection fileCollection)
        {
            // Loop through the label ids to generate the buff for files
            foreach (KeyValuePair<string, Dictionary<string, Label>> languageLabels in fileCollection.Labels)
            {
                foreach (KeyValuePair<string, Label> langLabel in languageLabels.Value)
                {
                    Label label = langLabel.Value;

                    DLabelFile labelFile = fileCollection.Files.First(e => e.Language == langLabel.Key);
                    // Build the string to save to file
                    if (labelFile != null)
                    {
                        // If the label file had the empty description, then do not remove it - this causes problems at check in if another developer where they had the value set
                        if (label.Description == "" || label.DescriptionExisted == " ;")
                            labelFile.Buff += $"{languageLabels.Key}={label.Text}{Environment.NewLine}";
                        else
                            labelFile.Buff += $"{languageLabels.Key}={label.Text}{Environment.NewLine} ;{label.Description}{Environment.NewLine}";
                    }
                }
            }
        }

        public static void GenerateBuffSingle(DLabelFileCollection fileCollection, DLabelFile dlabel)
        {
            // Loop through the label ids to generate the buff for files
            foreach (KeyValuePair<string, Dictionary<string, Label>> languageLabels in fileCollection.Labels)
            {
                // For now only buff if the language exists
                if (languageLabels.Value.ContainsKey(dlabel.Language))
                {
                    Label label = languageLabels.Value.First(e => e.Key == dlabel.Language).Value;

                    // If the label file had the empty description, then do not remove it - this causes problems at check in if another developer where they had the value set
                    if (label.Description == "" || label.DescriptionExisted == "")
                        dlabel.Buff += $"{languageLabels.Key}={label.Text}{Environment.NewLine}";
                    else
                        dlabel.Buff += $"{languageLabels.Key}={label.Text}{Environment.NewLine} ;{label.Description}{Environment.NewLine}";
                }
            }
        }

        /// <summary>
        /// Dump the DLabelFile to the text file on the physical drive
        /// </summary>
        /// <param name="dLabel">Label file to overwrite</param>
        private static void DumpLabelFileAndCompile(DLabelFile dLabel)
        {
            // Reset the content to what we have
            DumpLabelFile(dLabel);

            // Compile after saving
            DynaxUtils.CompileLabel(dLabel);
        }

        /// <summary>
        /// Dump the DLabelFile to the text file on the physical drive
        /// </summary>
        /// <param name="dlabel">Label file to overwrite</param>
        private static void DumpLabelFile(DLabelFile dLabel)
        {
            // Grab a backup of the file
            string fileBackup = File.ReadAllText(dLabel.FilePath, Encoding.UTF8);

            try
            {
                // Dump the final NewLine from the text
                if (dLabel.Buff != "")
                {
                    if (dLabel.Buff.Length >= 2)
                        dLabel.Buff = dLabel.Buff.Remove(dLabel.Buff.Length - 2);
                }
                
                // Rebuild from the grid
                using (StreamWriter file = new StreamWriter(dLabel.FilePath, false, Encoding.UTF8))
                {
                    file.WriteLine(dLabel.Buff);
                }
            }
            catch (Exception)
            {   // dump the original back into the file if any failures
                File.WriteAllText(dLabel.FilePath, fileBackup, Encoding.UTF8);
            }
            // Scrub the buff for rebuild later 
            dLabel.Buff = "";
        }

        /// <summary>
        /// Dump the new content to the label file
        /// </summary>
        /// <param name="label">current label file being processed</param>
        /// THE FILE IS NOT SAVED AS UTF8, THIS IS CAUSING ISSUES ON COMPARE
        protected static void Del_DumpLabelContent(DLabelFile label)
        {
            VSProjectNode vSProjectNode = VStudioUtils.GetSelectedProjectOrFirstActiveProject();
            IDesignMetaModelService metaModelService = vSProjectNode.DesignMetaModelService;
            AxLabelFile axLabelFile = metaModelService.GetLabelFile(label.Name);
            ModelInfo modelInfo = vSProjectNode.GetProjectsModelInfo(); 
            
            // Get the current content
            string currentContent = "", newContent = label.Buff.Remove(label.Buff.Length - 2);

            try
            {
                // # 1 - Secure the current content
                using (Stream content = metaModelService.CurrentMetadataProvider.LabelFiles.GetContent(axLabelFile, modelInfo))
                {
                    using (StreamReader reader = new StreamReader(content))
                    {
                        currentContent = reader.ReadToEnd();
                    }
                }

                try
                {
                    // # 2 - Scrub the file of the current content
                    metaModelService.CurrentMetadataProvider.LabelFiles.DeleteContent(axLabelFile, modelInfo);

                    try
                    {
                        // # 3 - Set the new content
                        byte[] byteArray = Encoding.UTF8.GetBytes(newContent);

                        using (MemoryStream stream = new MemoryStream(byteArray))
                        {
                            metaModelService.CurrentMetadataProvider.LabelFiles.PutContent(axLabelFile, modelInfo, stream);
                        }
                    }
                    catch (Exception exWrite)
                    {
                        VStudioUtils.LogToOutput($"Error saving label file : {label.Name} - {exWrite}");
                        // # 4 - Try to put the old content back
                        byte[] byteArray = Encoding.UTF8.GetBytes(currentContent);

                        using (MemoryStream stream = new MemoryStream(byteArray))
                        {
                            metaModelService.CurrentMetadataProvider.LabelFiles.PutContent(axLabelFile, modelInfo, stream);
                        }
                    }
                }
                catch (Exception exDelete)
                {
                    VStudioUtils.LogToOutput($"Error saving/deleting label file : {label.Name} - {exDelete}");
                }
            }
            catch (Exception exMain)
            {
                VStudioUtils.LogToOutput($"Error saving and or deleting content label file : {label.Name} - {exMain}");
            }
            // Scrub the buffer to prevent duplication on the next udpate/delete/create
            label.Buff = "";
        }
    }
}
