using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileScannerApp.Models
{
    public class ScanProgress
    {
        public int Current { get; set; }
        public int Total { get; set; }
        public string CurrentFile { get; set; }
        public int ThreatsFound { get; set; }
    }
}
