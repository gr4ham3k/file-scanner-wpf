using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileScannerApp
{
    public class AppConfig
    {
        public string VirusTotalApiKey { get; private set; }

        public static AppConfig Load()
        {
            return new AppConfig
            {
                VirusTotalApiKey = Environment.GetEnvironmentVariable("VIRUSTOTAL_API_KEY")
            };
        }
    }
}
