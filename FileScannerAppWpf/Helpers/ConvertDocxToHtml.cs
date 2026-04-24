using DocumentFormat.OpenXml.Packaging;
using OpenXmlPowerTools;
using System.Xml.Linq;
using System.IO;

namespace FileScannerApp.Wpf.Helpers
{
    internal class ConvertDocxToHtml
    {
        public static string Convert(string filePath)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".docx");

            File.Copy(filePath, tempPath, true);

            try
            {
                using var doc = WordprocessingDocument.Open(tempPath, true);

                var settings = new HtmlConverterSettings()
                {
                    PageTitle = "Preview"
                };

                XElement html = HtmlConverter.ConvertToHtml(doc, settings);

                return html.ToString();
            }
            finally
            {
                if (File.Exists(tempPath))
                {

                    File.Delete(tempPath);
                }
            }
        }
    }
}
