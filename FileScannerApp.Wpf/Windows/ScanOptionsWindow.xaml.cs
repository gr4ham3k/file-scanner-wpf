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
        Collect("Executables", ExecutablesCheckBox.IsChecked == true);
        Collect("Documents", DocumentsCheckBox.IsChecked == true);
        Collect("Images", ImagesCheckBox.IsChecked == true);
        Collect("Videos", VideosCheckBox.IsChecked == true);
        DialogResult = true;
    }

    private void Collect(string groupName, bool enabled)
    {
        if (enabled && FileTypeCatalog.Groups.TryGetValue(groupName, out var extensions))
        {
            FileTypes.AddRange(extensions);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
