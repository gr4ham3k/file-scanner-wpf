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
            string extNoDot = ext.TrimStart('.');

            if (string.IsNullOrWhiteSpace(pattern))
                return name + ext;

            string created = File.GetCreationTime(originalPath).ToString("yyyy-MM-dd");
            string modified = File.GetLastWriteTime(originalPath).ToString("yyyy-MM-dd");

            string folder = new DirectoryInfo(Path.GetDirectoryName(originalPath)).Name;
            string paddedCounter = counter.ToString("000");

            string newName = pattern
                .Replace("{name}", name)
                .Replace("{created}", created)
                .Replace("{modified}", modified)
                .Replace("{counter:000}", paddedCounter)
                .Replace("{counter}", paddedCounter)
                .Replace("{folder}", folder)
                .Replace("{ext}", extNoDot);

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
    }
}