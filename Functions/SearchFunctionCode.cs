using EnvDTE;
using Microsoft.Dynamics.AX.Metadata.Extensions.Reports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Functions_for_Dynamics_Operations.Functions
{
    internal class SearchFunctionCode
    {
        internal readonly string IncludedFolders = "axclass,axdataentityview,axform,axformpart,axinfopart,axmap,axquery,axtable,axtile,axview";
        public DirectoryInfo DirectoryInfo;
        public string TextToSearch;

        public SearchFunctionCode(DirectoryInfo directoryInfo, string textToSearch)
        {
            DirectoryInfo = directoryInfo;
            TextToSearch = textToSearch;
        }

        /// <summary>
        /// Run the logic of the search code function 
        /// </summary>
        /// <returns>list containing file found with the search term</returns>
        public List<CodeSearchFound> RunLogic()
        {
            List<CodeSearchFound> codeObjects = new List<CodeSearchFound>();
            List<string> values = IncludedFolders.Split(',').ToList();

            if (TextToSearch == "" || TextToSearch.Replace(" ", "").Length == 0)
                return codeObjects;
            // Search the files in the directory
            foreach (var directory in DirectoryInfo.GetDirectories())
            {
                // Only search the folders that are included
                if (values.Contains(directory.Name.ToLower()))
                {
                    SearchFiles(codeObjects, directory);
                }            
            }

            return codeObjects;
        }

        protected void SearchFiles(List<CodeSearchFound> codeObjects, DirectoryInfo subDirectory)
        {
            foreach (FileInfo file in subDirectory.GetFiles())
            {
                if (file.Extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    string contents = File.ReadAllText(file.FullName);

                    int sourceCodeStartIndex = contents.IndexOf("<sourcecode>", StringComparison.OrdinalIgnoreCase);
                    int sourceCodeEndIndex = contents.IndexOf("</sourcecode>", StringComparison.OrdinalIgnoreCase);

                    if (sourceCodeStartIndex != -1 && sourceCodeEndIndex != -1)
                    {
                        // Extract the source code content
                        string sourceCodeContent = contents.Substring(sourceCodeStartIndex, sourceCodeEndIndex - sourceCodeStartIndex);

                        // Search for the text within the source code content
                        int searchTextIndex = sourceCodeContent.IndexOf(TextToSearch, StringComparison.OrdinalIgnoreCase);
                        if (searchTextIndex != -1)
                        {
                            // Calculate the snippet start index to ensure it doesn't go below 0
                            int snippetStartIndex = Math.Max(0, searchTextIndex - 50);
                            // Calculate the snippet length to ensure it doesn't exceed the content's length
                            int snippetLength = Math.Min(TextToSearch.Length + 100, sourceCodeContent.Length - snippetStartIndex);

                            string snippet = sourceCodeContent.Substring(snippetStartIndex, snippetLength);

                            codeObjects.Add(new CodeSearchFound()
                            {
                                ObjectName = file.Name.Replace(".xml", ""),
                                ObjectType = subDirectory.Name,
                                FullName = file.FullName,
                                Snippet = snippet
                            });
                        }
                    }
                }
            }
        }
    }

    internal class CodeSearchFound
    {
        public string ObjectType { get; set; }
        public string ObjectName { get; set; }  
        public string Snippet { get; set; }
        public string FullName { get; set; }
    }

    internal class CodeSearchController_Original
    {
        public string TextToSearch;
        public string SearchType;

        internal class Threader
        {
            internal DirectoryInfo DirectoryInfo;
            public string TextToSearch;

            public List<CodeSearchFound> DoWork()
            {
                return new SearchFunctionCode(DirectoryInfo, TextToSearch).RunLogic();
            }
        }

        private List<List<CodeSearchFound>> ParallelForEach(List<Threader> threads)
        {   // Limit the number of threads that can concurrently run
            List<List<CodeSearchFound>> codeSearch = new List<List<CodeSearchFound>>();

            ParallelLoopResult loopResult = Parallel.ForEach(threads, new ParallelOptions() { MaxDegreeOfParallelism = 100 }, worker => codeSearch.Add(worker.DoWork()));

            return codeSearch;
        }

        public List<CodeSearchFound> FindCode()
        {
            List<Threader> threads = new List<Threader>();

            if (RuntimeHost.IsCloudHosted())
            {
                (string modelStoreFolder, string frameworkDirectory) = RuntimeHost.GetModelStoreAndFrameworkDirectories();
                // Search the custom models we have created
                SearchForCode(modelStoreFolder, threads);
                // Search the standard runtime models
                SearchForCode(frameworkDirectory, threads);
            }
            else
            {
                string folder = Microsoft.Dynamics.Framework.Tools.Configuration.ConfigurationHelper.CurrentConfiguration.ModelStoreFolder;
                if (!folder.IsNullOrEmpty())
                {
                    SearchForCode(folder, threads);
                }
            } 

            List<CodeSearchFound> finalList = new List<CodeSearchFound>();

            foreach (List<CodeSearchFound> codeFound in ParallelForEach(threads))
            {
                if (codeFound != null)
                    finalList = finalList.Concat(codeFound).ToList();
            }

            return finalList;
        }

        internal void SearchForCode(string directory, List<Threader> threads)
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
                            threads.Add(new Threader() { DirectoryInfo = model, TextToSearch = TextToSearch });
                        }
                    }
                }
            }
        }

        public CodeSearchController_Original(string textToSearch)
        {
            TextToSearch = textToSearch;
        }
    }

    internal class CodeSearchController
    {
        public string TextToSearch { get; private set; }
        public string SearchType { get; set; }

        internal class Threader
        {
            internal DirectoryInfo DirectoryInfo { get; set; }
            public string TextToSearch { get; set; }

            public Task<List<CodeSearchFound>> DoWorkAsync()
            {
                return Task.Run(() => new SearchFunctionCode(DirectoryInfo, TextToSearch).RunLogic());
            }
        }

        static async Task<List<List<CodeSearchFound>>> ParallelForEachAsync(List<Threader> threads)
        {
            var tasks = threads.Select(worker => worker.DoWorkAsync());

            List<CodeSearchFound>[] allResults = await Task.WhenAll(tasks);
            
            return allResults.ToList();
        }

        public async Task FindCodeAsync(DataGridView dataGridView)
        {
            List<Threader> threads = new List<Threader>();

            if (RuntimeHost.IsCloudHosted())
            {
                (string modelStoreFolder, string frameworkDirectory) = RuntimeHost.GetModelStoreAndFrameworkDirectories();
                // Search the custom models we have created
                SearchForCode(modelStoreFolder, threads);
                // Search the standard runtime models
                SearchForCode(frameworkDirectory, threads);
            }
            else
            {
                string folder = Microsoft.Dynamics.Framework.Tools.Configuration.ConfigurationHelper.CurrentConfiguration.ModelStoreFolder;
                if (!folder.IsNullOrEmpty())
                {
                    SearchForCode(folder, threads);
                }
            }

            var codeSearchResults = await ParallelForEachAsync(threads);

            // Flatten the list of lists into a single list
            dataGridView.DataSource = codeSearchResults.SelectMany(result => result ?? new List<CodeSearchFound>()).ToList();

            GridUtils.SetGridLayoutSearchCode(dataGridView);

            VStudioUtils.LogToGenOutput($"Searching finished");
        }

        internal void SearchForCode(string directory, List<Threader> threads)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(directory);

            foreach (DirectoryInfo directories in directoryInfo.GetDirectories())
            {
                if (directories.Name.ToLower() != "$tf" && directories.Name.ToLower() != "bin")
                {
                    foreach (DirectoryInfo model in directories.GetDirectories())
                    {
                        if (model.Name.ToLower() != "bin" && model.Name.ToLower() != "descriptor" && model.Name.ToLower() != "reports" &&
                            model.Name.ToLower() != "resources" && model.Name.ToLower() != "webcontent" && model.Name.ToLower() != "xppmetadata" &&
                            model.Name.ToLower() != ".pkgrefgen")
                        {
                            threads.Add(new Threader() { DirectoryInfo = model, TextToSearch = TextToSearch });
                        }
                    }
                }
            }
        }

        public CodeSearchController(string textToSearch)
        {
            TextToSearch = textToSearch;
        }
    }

}
