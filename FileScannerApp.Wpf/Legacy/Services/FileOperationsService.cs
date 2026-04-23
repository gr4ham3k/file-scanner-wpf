using FileScannerApp.Models;
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

        public (int deleted, int failed) DeleteFiles(List<string> paths)
        {
            int deletedCount = 0;
            int failedCount = 0;

            foreach (var path in paths)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    db.AddOperationLog(new OperationLog
                    {
                        OperationType = OperationType.Delete,
                        FileName = Path.GetFileName(path),
                        OldPath = path,
                        NewPath = null,
                        OperationDate = DateTime.Now,
                        CanUndo = false
                    });

                    deletedCount++;
                }
                catch
                {
                    failedCount++;
                }
            }

            return (deletedCount, failedCount);
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
    }
}
