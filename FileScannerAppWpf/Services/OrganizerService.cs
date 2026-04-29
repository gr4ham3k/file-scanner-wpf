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
            bool overwriteExisting,
            Database db,
            List<RenamePreview> previews)
        {
            if (!Directory.Exists(sourceFolder))
                throw new DirectoryNotFoundException("Folder źródłowy nie istnieje.");

            if (!Directory.Exists(destinationFolder))
                Directory.CreateDirectory(destinationFolder);

            foreach (var file in files)
            {
                if (!File.Exists(file.Path))
                    continue;

                string extension = file.Extension.ToLowerInvariant();

                if (fileTypes != null && fileTypes.Count > 0 && !fileTypes.Contains(extension))
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

                var preview = previews.FirstOrDefault(p => p.FullPath == file.Path);

                if (preview != null && !string.IsNullOrWhiteSpace(preview.NameAfter))
                {
                    fileName = Path.GetFileNameWithoutExtension(preview.NameAfter);
                }

                string fileExt = file.Extension;

                string targetPath = Path.Combine(targetFolder, fileName + fileExt);

                targetPath = ResolveConflict(fileName, fileExt, targetPath, overwriteExisting, targetFolder);

                try
                {
                    if (operation == "move")
                    {
                        File.Move(file.Path, targetPath);

                        db?.AddOperationLog(new OperationLog
                        {
                            OperationType = OperationType.Move,
                            FileName = Path.GetFileName(targetPath),
                            OldPath = file.Path,
                            NewPath = targetPath,
                            OperationDate = DateTime.Now,
                            CanUndo = true
                        });
                    }
                    else
                    {
                        File.Copy(file.Path, targetPath, overwriteExisting);

                        db?.AddOperationLog(new OperationLog
                        {
                            OperationType = OperationType.Copy,
                            FileName = Path.GetFileName(targetPath),
                            OldPath = file.Path,
                            NewPath = targetPath,
                            OperationDate = DateTime.Now,
                            CanUndo = false
                        });
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
            foreach (var group in FileTypeCatalog.Groups)
            {
                if (group.Value.Contains(ext))
                    return group.Key;
            }

            return "Others";
        }
    }
}