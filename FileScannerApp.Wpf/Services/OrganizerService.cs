using FileScannerApp.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace FileScannerApp
{
    public static class Organizer
    {
        public static string OrganizeFiles(
            List<FileData> files,
            string sourceFolder,
            string destinationFolder,
            List<string> fileTypes,
            string operation,
            bool createSubfolders,
            bool overwriteExisting)
        {
            if (!Directory.Exists(sourceFolder))
                throw new DirectoryNotFoundException("Folder źródłowy nie istnieje.");

            if (!Directory.Exists(destinationFolder))
                Directory.CreateDirectory(destinationFolder);

            foreach (var file in files)
            {
                if (!File.Exists(file.Path))
                    continue;

                string extension = file.Extension.ToLower();
                bool filterEnabled = fileTypes != null && fileTypes.Count > 0;

                if (filterEnabled && !fileTypes.Contains(extension))
                    continue;

                string targetFolder = destinationFolder;

                if (createSubfolders)
                {
                    string typeFolder = GetTypeFolder(extension);
                    targetFolder = Path.Combine(destinationFolder, typeFolder);

                    if (!Directory.Exists(targetFolder))
                        Directory.CreateDirectory(targetFolder);
                }

                string fileName = Path.GetFileNameWithoutExtension(file.Name);
                string fileExt = file.Extension;
                string targetPath = Path.Combine(targetFolder, fileName + fileExt);

                targetPath = ResolveConflict(fileName, fileExt, targetPath, overwriteExisting, targetFolder);

                try
                {
                    if (operation == "move")
                    {
                        if (File.Exists(targetPath))
                            File.Delete(targetPath);

                        File.Move(file.Path, targetPath);
                    }
                    else
                    {
                        File.Copy(file.Path, targetPath, overwriteExisting);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return destinationFolder;
        }

        private static string ResolveConflict(string fileName, string fileExt, string targetPath,
            bool overwriteExisting, string targetFolder)
        {
            if (!File.Exists(targetPath))
                return targetPath;

            if (overwriteExisting)
                return targetPath;

            int counter = 1;
            string newPath;

            do
            {
                newPath = Path.Combine(targetFolder, $"{fileName}({counter}){fileExt}");
                counter++;
            }
            while (File.Exists(newPath));

            return newPath;
        }

        private static string GetTypeFolder(string ext)
        {
            switch (ext)
            {
                case ".exe":
                case ".msi":
                case ".bat":
                    return "Executables";
                case ".pdf":
                case ".docx":
                case ".txt":
                    return "Documents";
                case ".jpg":
                case ".png":
                case ".bmp":
                case ".gif":
                    return "Images";
                case ".mp4":
                case ".avi":
                case ".mkv":
                    return "Videos";
                default:
                    return "Others";
            }
        }
    }
}
