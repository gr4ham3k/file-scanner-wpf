using Mammoth;


namespace FileScannerApp.Wpf.Helpers
{
    internal class ConvertDocxToHtml
    {
        public static string Convert(string filePath)
        {
            var converter = new DocumentConverter();
            var result = converter.ConvertToHtml(filePath);

            return result.Value;
        }
    }
}
