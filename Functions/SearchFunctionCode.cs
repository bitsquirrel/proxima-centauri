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

        public SearchFunctionCode(DirectoryInfo directoryInfo, string textToSearch, bool ignoreComments)
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
                    // string contents = File.ReadAllText(file.FullName);
                    // Buff the contents to avoid multiple reads
                    ParseCodeSearchFound parseCodeSearch = new ParseCodeSearchFound(File.ReadAllLines(file.FullName));
                    // Only match actual code, not comments
                    List<string> linesMatched = parseCodeSearch.CheckIfFoundIsNotAComment(TextToSearch);
                    // If no lines matched, skip to the next file
                    if (linesMatched.Count == 0)
                        continue; // Skip this file if the search text is not found in any line

                    foreach (string snippet in linesMatched)
                    {
                        codeObjects.Add(new CodeSearchFound()
                        {
                            ObjectName = file.Name.Replace(".xml", ""),
                            ObjectType = subDirectory.Name,
                            FullName = file.FullName,
                            Snippet = snippet.Trim() 
                        });
                    }

                    /*
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
                            // Check if the line containing the search text is not a comment
                            if (!parseCodeSearch.CheckIfFoundIsNotAComment(TextToSearch))
                                continue; // Skip this file if the search text is in a comment
                            // Calculate the snippet start index to ensure it doesn't go below 0
                            int snippetStartIndex = Math.Max(0, searchTextIndex - 50);
                            // Calculate the snippet length to ensure it doesn't exceed the content's length
                            int snippetLength = Math.Min(TextToSearch.Length + 100, sourceCodeContent.Length - snippetStartIndex);

                            string snippet = sourceCodeContent.Substring(snippetStartIndex, snippetLength);

                            if (snippet.Contains("//"))
                                System.Threading.Thread.Sleep(0);

                            codeObjects.Add(new CodeSearchFound()
                            {
                                ObjectName = file.Name.Replace(".xml", ""),
                                ObjectType = subDirectory.Name,
                                FullName = file.FullName,
                                Snippet = snippet
                            });
                        }
                    }
                    */
                }
            }
        }
    }

    internal class ParseCodeSearchFound
    {
        internal string[] Lines { get; set; }

        public ParseCodeSearchFound(string[] lines)
        {
            Lines = lines;
        }

        internal List<string> CheckIfFoundIsNotAComment(string textToFindInLine) // We are only interested in the lines that contain the text to find, ignoring comments
        {
            // Check if the text to find is in any of the lines, ignoring case
            return Lines.Where(line => line.ToLower().Contains(textToFindInLine.ToLower()))
                // Then filter out comment lines as they are not relevant for the search
                .Where(line => !line.TrimStart().Contains("//")
                && !line.TrimStart().Contains("/*") 
                && !line.TrimStart().Contains("*/")
                && !line.Trim().StartsWith("*")).ToList();
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
            public bool IgnoreComments;

            public List<CodeSearchFound> DoWork()
            {
                return new SearchFunctionCode(DirectoryInfo, TextToSearch, IgnoreComments).RunLogic();
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
            public bool IgnoreComments { get; set; }

            public Task<List<CodeSearchFound>> DoWorkAsync()
            {
                return Task.Run(() => new SearchFunctionCode(DirectoryInfo, TextToSearch, IgnoreComments).RunLogic());
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
