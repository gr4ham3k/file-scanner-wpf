using FileScannerApp.Wpf.Helpers;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace FileScannerApp.Wpf;

public partial class ScanOptionsWindow : Window
{
    public string SelectedFolder { get; private set; }
    public List<string> FileTypes { get; } = [];

    public ScanOptionsWindow(string? selectedFolder)
    {
        InitializeComponent();
        SelectedFolder = selectedFolder ?? string.Empty;
        FolderTextBox.Text = SelectedFolder;
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var folder = FolderPicker.Pick(SelectedFolder, "Select a folder to scan");
        if (folder != null)
        {
            SelectedFolder = folder;
            FolderTextBox.Text = folder;
        }
    }

    private void Start_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FolderTextBox.Text))
        {
            MessageBox.Show(this, "Select a folder first.", "Scan", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SelectedFolder = FolderTextBox.Text;

        FileTypes.Clear();

        if (ExecutablesCheckBox.IsChecked == true)
            FileTypes.AddRange(FileTypeCatalog.Groups["Executables"]);

        if (DocumentsCheckBox.IsChecked == true)
            FileTypes.AddRange(FileTypeCatalog.Groups["Documents"]);

        if (ImagesCheckBox.IsChecked == true)
            FileTypes.AddRange(FileTypeCatalog.Groups["Images"]);

        if (VideosCheckBox.IsChecked == true)
            FileTypes.AddRange(FileTypeCatalog.Groups["Videos"]);

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
