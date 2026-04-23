using FileScannerApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileScannerApp
{
    public static class RenameService
    {
        public static string GenerateNewName(string pattern, string originalPath, int counter, string option)
        {
            string name = Path.GetFileNameWithoutExtension(originalPath);
            string ext = Path.GetExtension(originalPath);
            string date = File.GetCreationTime(originalPath).ToString("yyyy-MM-dd");

            string newName = pattern
                .Replace("{name}", name)
                .Replace("{ext}", ext.Replace(".", ""))
                .Replace("{date}", date)
                .Replace("{counter}", counter.ToString());

            switch (option)
            {
                case "upper":
                    newName = newName.ToUpper();
                    break;

                case "lower":
                    newName = newName.ToLower();
                    break;

                case "capitalize":
                    if (!string.IsNullOrEmpty(newName))
                        newName = char.ToUpper(newName[0]) + newName.Substring(1).ToLower();
                    break;
            }

            return newName + ext;
        }

        public static List<RenamePreview> LoadPreview(string path)
        {
            var files = FileScannerService.Scan(path);

            return files.Select(f => new RenamePreview
            {
                FullPath = f.FullName,
                NameBefore = Path.GetFileName(f.FullName),
                NameAfter = ""
            }).ToList();
        }

        public static void RenameFiles(List<RenamePreview> files)
        {
            foreach (var item in files)
            {
                string directory = Path.GetDirectoryName(item.FullPath);
                string newPath = Path.Combine(directory, item.NameAfter);

                if (!File.Exists(item.FullPath))
                    continue;

                if (File.Exists(newPath))
                    throw new Exception($"File exists: {newPath}");

                File.Move(item.FullPath, newPath);
            }
        }

    }
}