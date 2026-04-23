using Forms = System.Windows.Forms;

namespace FileScannerApp.Wpf.Helpers;

public static class FolderPicker
{
    public static string? Pick(string? initialPath = null, string description = "Select a folder")
    {
        using var dialog = new Forms.FolderBrowserDialog
        {
            Description = description,
            SelectedPath = string.IsNullOrWhiteSpace(initialPath) ? string.Empty : initialPath,
            ShowNewFolderButton = true
        };

        return dialog.ShowDialog() == Forms.DialogResult.OK ? dialog.SelectedPath : null;
    }
}
