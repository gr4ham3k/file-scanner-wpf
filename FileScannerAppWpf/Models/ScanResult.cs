using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileScannerApp.Models
{
    public class ScanResult
    {
        public int Id { get; set; }
        public int ScanId { get; set; }
        public string FileName { get; set; }
        public string Status { get; set; }
        public string ApiResponse { get; set; }

    }
}
