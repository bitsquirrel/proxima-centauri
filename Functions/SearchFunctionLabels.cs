using EnvDTE;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Functions_for_Dynamics_Operations.Functions;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using System;

namespace Functions_for_Dynamics_Operations
{
    internal class LabelThread
    {
        public string TextToSearch;
        public string SearchType;
        public string Language;
        public string Filename; // Filename of the label file to search in

        public Task<List<DLabelSearch>> DoWorkAsync()
        {
            return Task.Run(() => new LabelSearcher(TextToSearch, SearchType, Language, Filename).RunLogic());
        }
    }

    internal class LabelSearcherController
    {
        public List<List<DLabelSearch>> dLabelSearches;
        public DataGridView DataGridView;
        public string TextToSearch;
        public string SearchType;
        public string Language;

        public LabelSearcherController(DataGridView dataGridView, string language, string textToSearch, string searchType)
        {
            DataGridView = dataGridView;
            TextToSearch = textToSearch;
            SearchType = searchType;
            Language = language;
        }

        static async Task<List<List<DLabelSearch>>> ParallelForEachAsync(List<LabelThread> threads)
        {
            var tasks = threads.Select(worker => worker.DoWorkAsync());

            List<DLabelSearch>[] allResults = await Task.WhenAll(tasks);

            return allResults.ToList();
        }

        public async Task FindLabelsAsync()
        {
            List<LabelThread> threads = new List<LabelThread>();

            Dictionary<string, List<string>> languagesAndFileNames = new Dictionary<string, List<string>>();
            // Get the languages and file names from the model store and framework directories
            new LabelSearchUtils().GetLabelLanguages(languagesAndFileNames);
            // Default to en-us if no language is selected
            List<string> filenames = languagesAndFileNames[string.IsNullOrEmpty(Language) ? "en-us" : Language.ToLower()];
            if (filenames == null || filenames.Count == 0)
            {
                VStudioUtils.LogToGenOutput($"No label files found for language: {Language}");
                return;
            }
            // Iterate through the filenames and create threads for each label file
            foreach (string filename in filenames)
            {
                // Create a new thread for each label file
                threads.Add(new LabelThread()
                {
                    TextToSearch = TextToSearch,
                    SearchType = SearchType,
                    Language = Language,
                    Filename = filename
                });
            }
            // Search using parallel threads
            var codeSearchResults = await ParallelForEachAsync(threads);
            // Flatten the list of lists into a single list
            DataGridView.DataSource = codeSearchResults.SelectMany(result => result ?? new List<DLabelSearch>()).ToList();

            GridUtils.SetGridLayoutSearch(DataGridView);

            VStudioUtils.LogToGenOutput($"Searching finished");
        }
    }

    internal class LabelSearcher
    {
        public string TextToSearch;
        public string SearchType;
        public string Language;
        public string Filename; // Filename of the label file to search in

        public LabelSearcher(string textToSearch, string searchType, string language, string filename)
        {
            TextToSearch = textToSearch;
            SearchType = searchType;
            Language = language;
            Filename = filename;
        }

        public List<DLabelSearch> RunLogic()
        {
            Microsoft.Dynamics.AX.Metadata.Service.IMetaModelService modelService = DesignMetaModelService.Instance.CurrentMetaModelService;

            List<DLabelSearch> labels = new List<DLabelSearch>();

            AxLabelFile labelFile = modelService.GetLabelFile(Filename);
            if (labelFile != null)
            {
                // Get the model info for the label file
                ModelInfoCollection modelInfoColl = modelService.GetLabelFileModelInfo(Filename);
                // fo now we just use the first model info
                using (Stream content = modelService.MetadataProvider.LabelFiles.GetContent(labelFile, modelInfoColl.FirstOrDefault()))
                {
                    if (content != null)
                    {
                        using (StreamReader reader = new StreamReader(content))
                        {
                            string contentString = reader.ReadToEnd();
                            bool foundMatch = false;
                            // Does the content contain the text to search for?
                            if (SearchType == "Exact")
                            {
                                if (contentString.Contains(TextToSearch))
                                    foundMatch = true;
                            }
                            else
                            {
                                if (contentString.ToLower().Contains(TextToSearch.ToLower()))
                                    foundMatch = true;
                            }

                            if (!foundMatch)
                            {
                                // No match found, return empty list
                                return labels;
                            }
                            // Split the content into lines
                            List<string> lines = contentString.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
                            // Fetch all the matches from the lines for the text to search
                            foreach (string labelMatch in lines.Where(a => a.ToLower().Contains(TextToSearch.ToLower())))
                            {
                                if (labelMatch.Trim().StartsWith(";"))
                                {
                                    // This is a comment, skip it
                                    continue;
                                }
                                // Try pick up the text after the '='
                                string id = labelMatch.Substring(0, labelMatch.IndexOf("=")).Trim();
                                string text = labelMatch.Substring(labelMatch.IndexOf("=") + 1).Trim();
                                // Buff the label search object
                                labels.Add(new DLabelSearch(Filename, id)
                                {
                                    Text = text
                                });
                            }
                        }
                    }
                }
            }

            return labels;
        }
    }

    internal class LabelSearchUtils
    {
        internal Dictionary<string, List<string>> SearchForLabelsDict()
        {
            // If you want to get the languages from the model store and framework directories, uncomment the following code
            Dictionary<string, List<string>> languagesAndFileNames = new Dictionary<string, List<string>>();
            GetLabelLanguages(languagesAndFileNames);
            return languagesAndFileNames;
        }

        internal List<string> SearchForLabels()
        {
            // Get the languages from the current metadata service, as this is more accurate than the old way of getting the languages
            List<string> languages = new List<string>();
            GetLabelLanguages(languages);
            return languages;
        }

        internal void GetLabelLanguages(List<string> languages)
        {
            Microsoft.Dynamics.AX.Metadata.Service.IMetaModelService modelService = Microsoft.Dynamics.Framework.Tools.MetaModel.Core.DesignMetaModelService.Instance.CurrentMetaModelService;
            // 1) Eagerly load all paths so PLINQ can partition efficiently.
            List<string> allFiles = (List<string>)modelService.GetLabelFileNames();

            // 2) In parallel, pull off everything after the *last* '_' in the filename.
            var distinctLangs = allFiles
                .AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .Select(Path.GetFileNameWithoutExtension)          // "…_ar-BH"
                .Select(name =>
                {
                    int idx = name.LastIndexOf('_');
                    return idx >= 0 && idx + 1 < name.Length
                        ? name.Substring(idx + 1)                  // "ar-BH"
                        : null;
                })
                .Where(lang => !string.IsNullOrEmpty(lang))
                .Distinct()
                .OrderBy(lang => lang)                              // optional: sort a→z
                .ToList();

            // 3) Buff the results into a list of distinct languages.
            Console.WriteLine($"Found {distinctLangs.Count} distinct language(s):");
            foreach (var lang in distinctLangs)
            {
                if (!languages.Contains(lang.ToLower()))
                {
                    languages.Add(lang.ToLower());
                }
            }
        }

        internal void GetLabelLanguages(Dictionary<string, List<string>> languagesAndFileNames)
        {
            Microsoft.Dynamics.AX.Metadata.Service.IMetaModelService modelService = Microsoft.Dynamics.Framework.Tools.MetaModel.Core.DesignMetaModelService.Instance.CurrentMetaModelService;
            // 1) Suppose you already have all your .xml paths in a List<string>:
            List<string> allFiles = (List<string>)modelService.GetLabelFileNames();

            // 2) Group them by the suffix after the last '_' in the filename:
            var filesByLang = allFiles
                .GroupBy(filePath =>
                {
                    // strip off directory + extension
                    string name = Path.GetFileNameWithoutExtension(filePath);
                    int idx = name.LastIndexOf('_');
                    return (idx >= 0 && idx + 1 < name.Length)
                        ? name.Substring(idx + 1)
                        : String.Empty;
                })
                // drop any that didn’t match your “_lang” pattern
                .Where(g => !String.IsNullOrEmpty(g.Key));

            // 3) Build your Dictionary<string, List<string>>
            Dictionary<string, List<string>> dict = filesByLang
                .ToDictionary(
                    grp => grp.Key,        // language code, e.g. "en-US", "ar-BH", "it"
                    grp => grp.ToList()    // list of all file paths for that language
                );

            // 4) Now you’ve got:
            //    - dict.Keys  => your distinct languages
            //    - dict[lang] => the List<string> of files in that language
            Console.WriteLine("Languages found:");
            foreach (var lang in dict.Keys.OrderBy(l => l))
            {
                if (!languagesAndFileNames.ContainsKey(lang.ToLower()))
                {
                    languagesAndFileNames.Add(lang.ToLower(), dict[lang]);
                }
                else
                {
                    languagesAndFileNames[lang.ToLower()].AddRange(dict[lang]);
                }
            }
        }
    }

    // This class is no longer used, but is kept for reference
    internal class LabelSearchFunc
    {
        public DirectoryInfo DirectoryInfo;
        public string TextToSearch;
        public string SearchType;
        public string Language;

        public LabelSearchFunc(DirectoryInfo directoryInfo, string language, string textToSearch, string searchType)
        {
            DirectoryInfo = directoryInfo;
            TextToSearch = textToSearch;
            SearchType = searchType;
            Language = language;
        }

        public List<DLabelSearch> RunLogic()
        {
            List<DLabelSearch> labels = new List<DLabelSearch>();

            if (TextToSearch == "" || TextToSearch.Replace(" ", "").Length == 0)
                return labels;

            foreach (FileInfo file in DirectoryInfo.GetFiles())
            {
                if (file.Extension.ToLower() != ".txt")
                    continue;

                string fileName = Path.GetFileName(file.FullName);
                string fileId = fileName.Substring(0, fileName.IndexOf('.'));

                string line = "";
                // Using the stream allows for multi fetching in the loop
                string file_name = file.FullName.Length > 260 ? @"\\?\" + file.FullName : file.FullName;
                // IF the filename exceeds 260 characters then we need to amend the file name
                using (StreamReader fileStream = new StreamReader(file_name))
                {
                    while (line != null)
                    {
                        if (line == "")
                            line = fileStream.ReadLine();

                        // End of file
                        if (line == null)
                            break;

                        if (line.IsNullOrEmpty() || line == " ")
                        {
                            line = "";
                            continue;
                        }

                        if (line.Contains("=") && (line.Length >= 2 && line.Substring(0, 2) != " ;"))
                        {
                            // Try pick up the description
                            var possibleDescription = fileStream.ReadLine();

                            // This is a label - closest we will get
                            if (line.ToLower().Contains(TextToSearch.ToLower()))
                            {
                                string id = line.Substring(0, line.IndexOf("="));

                                // Get the label
                                DLabelSearch dLabel = new DLabelSearch(fileId, id)
                                {
                                    Text = line.Substring(line.IndexOf("=") + 1)
                                };

                                if (possibleDescription != null && possibleDescription.Length >= 2 && possibleDescription.Substring(0, 2) == " ;" && !possibleDescription.Contains("="))
                                {
                                    dLabel.Description = possibleDescription.Replace(" ;", "");
                                }

                                if (SearchType == "Exact")
                                {   // Exact match of the label being searched
                                    if (dLabel.Text.ToLower() == TextToSearch.ToLower())
                                        labels.Add(dLabel);
                                }
                                else
                                    labels.Add(dLabel);
                            }
                            else if (SearchType != "Exact" && possibleDescription != null && possibleDescription.Length >= 2 && possibleDescription.Substring(0, 2) == " ;" && !possibleDescription.Contains("="))
                            {
                                // The comment matches our search
                                if (possibleDescription.ToLower().Contains(TextToSearch.ToLower()))
                                {
                                    string id = line.Substring(0, line.IndexOf("="));

                                    // Get the label
                                    DLabelSearch dLabel = new DLabelSearch(fileId, id)
                                    {
                                        Text = line.Substring(line.IndexOf("=") + 1),
                                        Description = possibleDescription.Replace(" ;", "")
                                    };

                                    labels.Add(dLabel);
                                }
                            }

                            // The end of file
                            if (possibleDescription == null)
                                break;

                            // Scrub the line for the next label - either used from description or the next line
                            line = "";

                            if (possibleDescription.Contains("=") && (possibleDescription.Length >= 2 && possibleDescription.Substring(0, 2) != " ;"))
                            {
                                // This was thought to be a description - rework it as a label
                                line = possibleDescription;
                            }
                        }
                        else
                        {
                            // This is caused by multiple comments on labels - looks like a bug during merge from Microsoft on the en-us file
                            line = "";
                        }
                    }
                }
            }

            return labels;
        }
    }

    // This controller is not adequate for the current implementation, but is kept for reference
    internal class LabelSearchController
    {
        public List<List<DLabelSearch>> dLabelSearches;
        public DataGridView DataGridView;
        public string TextToSearch;
        public string SearchType;
        public string Language;

        internal class Threader
        {
            internal DirectoryInfo DirectoryInfo;
            public string TextToSearch;
            public string SearchType;
            public string Language;

            public Task<List<DLabelSearch>> DoWorkAsync()
            {
                return Task.Run(() => new LabelSearchFunc(DirectoryInfo, Language, TextToSearch, SearchType).RunLogic());
            }
        }

        static async Task<List<List<DLabelSearch>>> ParallelForEachAsync(List<Threader> threads)
        {
            var tasks = threads.Select(worker => worker.DoWorkAsync());

            List<DLabelSearch>[] allResults = await Task.WhenAll(tasks);

            return allResults.ToList();
        }

        public async Task FindLabelsAsync()
        {
            List<Threader> threads = new List<Threader>();

            if (RuntimeHost.IsCloudHosted())
            {
                (string modelStoreFolder, string frameworkDirectory) = RuntimeHost.GetModelStoreAndFrameworkDirectories();
                // Search the custom models we have created
                SearchForLabels(modelStoreFolder, threads);
                // Search the standard runtime models
                SearchForLabels(frameworkDirectory, threads);
            }
            else
            {
                // The current config works perfectly for local instances
                string folder = Microsoft.Dynamics.Framework.Tools.Configuration.ConfigurationHelper.CurrentConfiguration.ModelStoreFolder;
                if (!folder.IsNullOrEmpty())
                {
                    SearchForLabels(folder, threads);
                }
            }
            // Search using parallel threads
            var codeSearchResults = await ParallelForEachAsync(threads);
            // Flatten the list of lists into a single list
            DataGridView.DataSource = codeSearchResults.SelectMany(result => result ?? new List<DLabelSearch>()).ToList();

            GridUtils.SetGridLayoutSearch(DataGridView);

            VStudioUtils.LogToGenOutput($"Searching finished");
        }

        internal void SearchForLabels(string directory, List<Threader> threads)
        {
            if (!directory.IsNullOrEmpty())
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(directory);

                foreach (DirectoryInfo directories in directoryInfo.GetDirectories())
                {
                    if (directories.Name.ToLower() != "$tf" && directories.Name.ToLower() != "bin")
                    {
                        foreach (DirectoryInfo model in directories.GetDirectories())
                        {
                            if (model.Name.ToLower() != "bin" && model.Name.ToLower() != "descriptor" && model.Name.ToLower() != "reports" && model.Name.ToLower() != "resources"
                                && model.Name.ToLower() != "webcontent" && model.Name.ToLower() != "xppmetadata" && model.Name.ToLower() != ".pkgrefgen")
                            {
                                string result = FindDirectory(model.FullName, "AxLabelFile");
                                if (!result.IsNullOrEmpty())
                                {
                                    if (Directory.Exists($@"{result}\LabelResources\{Language}"))
                                    {
                                        DirectoryInfo dirInfo = new DirectoryInfo($@"{result}\LabelResources\{Language}");

                                        threads.Add(new Threader() { DirectoryInfo = dirInfo, Language = Language, TextToSearch = TextToSearch, SearchType = SearchType });
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal string FindDirectory(string currentDirectory, string targetDirectoryName)
        {
            try
            {
                foreach (string directory in Directory.GetDirectories(currentDirectory))
                {
                    if (Path.GetFileName(directory).Equals(targetDirectoryName, StringComparison.OrdinalIgnoreCase))
                    {
                        return directory;
                    }

                    string result = FindDirectory(directory, targetDirectoryName);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Handle the exception if access to a directory is denied
                VStudioUtils.LogToGenOutput($"Access denied to directory: {currentDirectory}");
            }

            return null;
        }

        public LabelSearchController(DataGridView dataGridView, string language, string textToSearch, string searchType)
        {
            DataGridView = dataGridView;
            TextToSearch = textToSearch;
            SearchType = searchType;
            Language = language;
        }
    }
}
