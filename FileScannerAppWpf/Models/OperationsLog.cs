using System;

namespace FileScannerApp.Models
{
    public class OperationLog
    {
        public int Id { get; set; }

        public OperationType OperationType { get; set; }

        public string FileName { get; set; }

        public string OldPath { get; set; }

        public string NewPath { get; set; }

        public DateTime OperationDate { get; set; }

        public bool CanUndo { get; set; }
    }
}