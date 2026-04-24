using FileScannerApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileScannerApp.Services
{
    public class UndoService
    {
        public static bool Undo(OperationLog log)
        {
            if (log == null)
                return false;

            if (string.IsNullOrEmpty(log.OldPath) || string.IsNullOrEmpty(log.NewPath))
                return false;

            if (!File.Exists(log.NewPath))
                return false;

            string oldDir = Path.GetDirectoryName(log.OldPath);

            if (!Directory.Exists(oldDir))
                Directory.CreateDirectory(oldDir);

            File.Move(log.NewPath, log.OldPath);

            return true;
        }
    }
}
