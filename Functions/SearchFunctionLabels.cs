using EnvDTE;
using Functions_for_Dynamics_Operations.Functions;
using Functions_for_Dynamics_Operations.Objects;
using Microsoft.Dynamics.AX.Metadata.Extensions.Reports;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Settings.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Functions_for_Dynamics_Operations
{
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

    internal class LabelSearchUtils
    {
        internal List<string> SearchForLabels()
        {
            List<string> languages = new List<string>();

            if (RuntimeHost.IsCloudHosted())
            {
                (string modelStoreFolder, string frameworkDirectory) = RuntimeHost.GetModelStoreAndFrameworkDirectories();

                GetLabelLanguages(modelStoreFolder, languages);

                GetLabelLanguages(frameworkDirectory, languages);
            }
            else
            {
                string directory = Microsoft.Dynamics.Framework.Tools.Configuration.ConfigurationHelper.CurrentConfiguration.ModelStoreFolder;

                GetLabelLanguages(directory, languages);
            }

            return languages;
        }

        internal void GetLabelLanguages(string directory, List<string> languages)
        {
            if (!directory.IsNullOrEmpty())
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(directory);

                foreach (DirectoryInfo directories in directoryInfo.GetDirectories())
                {
                    if (Directory.Exists($@"{directories.FullName}\{directories.Name}\AxLabelFile\LabelResources"))
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo($@"{directories.FullName}\{directories.Name}\AxLabelFile\LabelResources");

                        foreach (DirectoryInfo labelDirectory in dirInfo.GetDirectories())
                        {
                            if (!languages.Contains(labelDirectory.Name.ToLower()))
                            {
                                languages.Add(labelDirectory.Name.ToLower());
                            }
                        }
                    }
                }
            }
        }
    }

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

        public async Task FindLabels()
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
