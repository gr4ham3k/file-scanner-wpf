using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileScannerApp.Models
{
    public class Scan
    {
        public int Id {  get; set; }
        public DateTime ScanDate { get; set; }
        public string ScanPath { get; set; }
        public int FilesCount { get; set; }
        public int ThreatsFound {  get; set; }
        public string Status { get; set; }

    }
}
