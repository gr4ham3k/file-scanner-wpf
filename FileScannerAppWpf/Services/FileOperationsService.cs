using FileScannerApp.Models;
using FileScannerApp.Wpf.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace FileScannerApp.Services
{
    public class FileOperationsService
    {
        private readonly Database db;

        public FileOperationsService(Database db)
        {
            this.db = db;
        }

        public (int moved, int failed) DeleteFiles(List<string> paths)
        {
            int movedCount = 0;
            int failedCount = 0;

            var binRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");

            if (!Directory.Exists(binRoot))
                Directory.CreateDirectory(binRoot);

            foreach (var path in paths)
            {
                try
                {
                    if (!File.Exists(path))
                    {
                        failedCount++;
                        continue;
                    }

                    string fileName = Path.GetFileName(path);
                    string uniqueName = $"{Guid.NewGuid()}_{fileName}";
                    string destPath = Path.Combine(binRoot, uniqueName);

                    File.Move(path, destPath);

                    db.AddOperationLog(new OperationLog
                    {
                        OperationType = OperationType.Delete,
                        FileName = fileName,
                        OldPath = path,
                        NewPath = destPath,
                        OperationDate = DateTime.Now,
                        CanUndo = true
                    });

                    movedCount++;
                }
                catch
                {
                    failedCount++;
                }
            }

            return (movedCount, failedCount);
        }

        public (int moved, int skipped, List<(string oldPath, string newPath)> updatedPaths)
        MoveFiles(List<string> paths, string targetFolder)
        {

            int movedCount = 0;
            int skippedCount = 0;

            var updated = new List<(string, string)>();

            foreach (var oldPath in paths)
            {
                try
                {
                    if (!File.Exists(oldPath))
                    {
                        skippedCount++;
                        continue;
                    }

                    string fileName = Path.GetFileName(oldPath);
                    string newPath = Path.Combine(targetFolder, fileName);

                    if (File.Exists(newPath))
                    {
                        skippedCount++;
                        continue;
                    }

                    File.Move(oldPath, newPath);


                    db.AddOperationLog(new OperationLog
                    {
                        OperationType = OperationType.Move,
                        FileName = fileName,
                        OldPath = oldPath,
                        NewPath = newPath,
                        OperationDate = DateTime.Now,
                        CanUndo = true
                    });

                    updated.Add((oldPath, newPath));
                    movedCount++;
                }
                catch
                {
                    skippedCount++;
                }
            }

            return (movedCount, skippedCount, updated);
        }

        public (bool success, string newPath, string error) RenameFile(string oldPath, string newName)
        {
            try
            {
                if (!File.Exists(oldPath))
                    return (false, null, "File does not exist.");

                string directory = Path.GetDirectoryName(oldPath);
                string extension = Path.GetExtension(oldPath);

                if (!newName.EndsWith(extension))
                    newName += extension;

                string newPath = Path.Combine(directory, newName);

                if (File.Exists(newPath))
                    return (false, null, "File with this name already exists.");

                File.Move(oldPath, newPath);

                db.AddOperationLog(new OperationLog
                {
                    OperationType = OperationType.Rename,
                    FileName = newName,
                    OldPath = oldPath,
                    NewPath = newPath,
                    OperationDate = DateTime.Now,
                    CanUndo = true
                });

                return (true, newPath, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        public void CleanupBin()
        {
            var binRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");

            if (!Directory.Exists(binRoot))
                return;

            foreach (var file in Directory.GetFiles(binRoot))
            {
                try
                {
                    File.Delete(file);

                    db.MarkAsDeletedPermanently(file);
                }
                catch
                {
                    
                }
            }
        }

        public void RestoreFile(OperationLog log)
        {
            if (string.IsNullOrEmpty(log.NewPath) || !File.Exists(log.NewPath))
                return;

            try
            {
                string? targetDir = Path.GetDirectoryName(log.OldPath);

                if (string.IsNullOrEmpty(targetDir))
                    return;

                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);

                File.Move(log.NewPath, log.OldPath);

                db.AddOperationLog(new OperationLog
                {
                    OperationType = OperationType.Move,
                    FileName = log.FileName,
                    OldPath = log.NewPath,
                    NewPath = log.OldPath,
                    OperationDate = DateTime.Now,
                    CanUndo = false
                });
            }
            catch
            {

            }
        }
    }
}
