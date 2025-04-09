using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Functions_for_Dynamics_Operations
{
    public class LabelUtils
    {
        public static bool LoadLabelFile(DLabelFileCollection fileCollection, DLabelFile labelFile)
        {
            string id = "", text = "", label = "", description = "", descriptionExisted = "";

            try
            {
                using (StreamReader file = new StreamReader(labelFile.FilePath))
                {
                    var line = file.ReadLine();
                    bool endOfFile = false;
                    descriptionExisted = "";

                    while (line != null)
                    {
                        if (line != "")
                        {
                            string next = file.ReadLine();

                            if (next == null)
                            {
                                next = "";
                                endOfFile = true;
                            }

                            if (next == ";")
                                next = " ;";

                            if (next.Length >= 2 && next.Substring(0, 2) == " ;")
                            {   // This is a description
                                description = next.Substring(2);
                                descriptionExisted = " ;";
                            }
                            else
                            {
                                description = "";
                            }

                            if (line.Substring(0, 2) == " ;" && endOfFile)
                                break;

                            id = line.Substring(0, line.IndexOf("=")).Trim(' ');
                            text = line.Substring(line.IndexOf("=") + 1).Trim(' ');
                            label = line;

                            if (fileCollection.Labels.ContainsKey(id))
                            {   // Existing label id adding new language
                                if (fileCollection.Labels.TryGetValue(id, out Dictionary<string, Label> labelValues))
                                {
                                    if (!labelValues.TryGetValue(labelFile.Language, out Label labelExists))
                                    {
                                        labelValues.Add(labelFile.Language, new Label(text, description, descriptionExisted));
                                    }
                                    else
                                    {
                                        if (fileCollection.Duplicates.TryGetValue(id, out Dictionary<string, Label> duplicateValues))
                                        {
                                            if (fileCollection.Duplicates.ContainsKey(id))
                                            {
                                                if (!duplicateValues.TryGetValue(labelFile.Language, out Label duplicateExists))
                                                {
                                                    duplicateValues.Add(labelFile.Language, new Label(text, description, descriptionExisted));
                                                }
                                            }
                                            else
                                            {   // New label id not yet buffered
                                                fileCollection.Duplicates.Add(id, new Dictionary<string, Label>
                                            {
                                                { labelFile.Language, new Label(text, description, descriptionExisted) }
                                            });
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {   // New label id not yet buffered
                                fileCollection.Labels.Add(id, new Dictionary<string, Label>
                                {
                                    { labelFile.Language, new Label(text, description, descriptionExisted) }
                                });
                            }

                            // Scrub the buffer
                            id = "";
                            description = "";
                            descriptionExisted = "";

                            if (endOfFile)
                                break;

                            if (next.Length >= 2 && next.Substring(0, 2) == " ;")
                            {
                                if (next.Contains("=") && next.Substring(0, 7).ToLower() == " ;label")
                                {   // The next label mixed into this label
                                    line = next.Substring(7);
                                }
                                else
                                    line = file.ReadLine();
                            }
                            else
                                line = next;
                        }
                        else
                            line = file.ReadLine();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                VStudioUtils.LogToGenOutput($"Error loading label language {labelFile.Language}{Environment.NewLine}Id: {id}, Text: {text}, Description: {description}{Environment.NewLine}Exception: {ex}");
                // Return false rather than throw as that kills visual studio
                return false;
            }
        }

        public static DLabelFile GetDLabelFile(DLabelFileCollection labels, string resourceName)
        {
            if (labels != null && labels.Files != null)
            {
                foreach (DLabelFile dlabelfile in labels.Files)
                {
                    if (dlabelfile.ResourceName == resourceName)
                        return dlabelfile;
                }
            }

            return null;
        }

        internal static bool CheckTextAlreadyExists(DLabelFileCollection LabelFileColl, DLabelFile selected, string text, out string labelidExisting)
        {
            if (text != "")
            {
                foreach (var labelid in LabelFileColl.Labels)
                {
                    if (labelid.Value.TryGetValue(selected.Language, out Label label))
                    {
                        if (label.Text.ToLower() == text.ToLower())
                        {
                            labelidExisting = labelid.Key;
                            return true;
                        }
                    }
                }
            }

            labelidExisting = "";
            return false;
        }

        internal static bool CreateOrUseExistingLabel(DLabelFileCollection LabelFileColl, string existingId, string text)
        {
            string labelForMessageBox = $"'{text}', already exists for label{Environment.NewLine}{Environment.NewLine}";
            labelForMessageBox += $"Click 'Yes' to copy label id to clipboard{Environment.NewLine}{Environment.NewLine}";
            labelForMessageBox += $"Click 'No' to create the label{Environment.NewLine}{Environment.NewLine}";
            labelForMessageBox += $"@{LabelFileColl.Id}:{existingId}";

            if (MessageBox.Show(labelForMessageBox, "Label Already Exists", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                return false;
            }
            else
                return true;
        }
    }
}
