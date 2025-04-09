using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace Functions_for_Dynamics_Operations
{
    internal class UpdateCache
    {
        int folderNumber;

        public void ClearUpdateRetailSDKCache()
        {
            if (new DirectoryInfo(@"C:\RetailSDK\Update\").GetDirectories().Length > 0 ||
                new DirectoryInfo(@"C:\RetailServer\").GetDirectories().FirstOrDefault().Name.ToLower().Contains("backup") ||
                new DirectoryInfo(@"C:\RetailCloudPos\").GetDirectories().FirstOrDefault().Name.ToLower().Contains("backup"))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show($"Old upgrade version cache was found{Environment.NewLine}Process 'Ok' to delete", "Clear old update files", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    ScrubOldCache();
                }
            }
        }

        protected void ScrubOldCache()
        {
            foreach (DirectoryInfo directory in new DirectoryInfo(@"C:\RetailSDK\Update\").GetDirectories())
            {
                ClearLowestDirectory(RenameLongFolderName(directory));
            }

            foreach (DirectoryInfo directory in new DirectoryInfo(@"C:\RetailServer\").GetDirectories())
            {
                if (directory.Name.ToLower().Contains("backup"))
                {
                    ClearLowestDirectory(RenameLongFolderName(directory));
                }
            }

            foreach (DirectoryInfo directory in new DirectoryInfo(@"C:\RetailCloudPos\").GetDirectories())
            {
                if (directory.Name.ToLower().Contains("backup"))
                {
                    ClearLowestDirectory(RenameLongFolderName(directory));
                }
            }
        }

        protected DirectoryInfo RenameLongFolderName(DirectoryInfo directoryInfo)
        {
            if (directoryInfo.Name.Length > 25)
            {
                folderNumber++;

                string folderNew = directoryInfo.FullName.Replace(directoryInfo.Name, $"FOLDER{folderNumber}");

                directoryInfo.MoveTo(folderNew);
                // Get the just moved directory
                directoryInfo = new DirectoryInfo(folderNew);
            }

            return directoryInfo;
        }

        protected void ClearLowestDirectory(DirectoryInfo directoryInfo)
        {
            DirectoryInfo[] directories = directoryInfo.GetDirectories();

            if (directories.Length > 0)
            {   // First process the sub directories
                foreach (DirectoryInfo directory in directories)
                {
                    ClearLowestDirectory(RenameLongFolderName(directory));
                }
            }

            // Delete the files in this folder
            foreach (FileInfo fileInfo in directoryInfo.GetFiles())
            {
                if (!File.Exists(fileInfo.FullName))
                {
                    // Filename is too long
                    string newFileName = directoryInfo.FullName + @"\DEL" + fileInfo.Extension;

                    fileInfo.MoveTo(newFileName);

                    if (File.Exists(newFileName))
                    {
                        File.Delete(newFileName);
                    }
                }
                else
                {
                    fileInfo.Delete();
                }
            }

            // Let the folder empty
            directoryInfo.Delete();
           
        }
    }
}
