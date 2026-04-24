using FileScannerApp.Models;
using FileScannerApp.Wpf.Helpers;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace FileScannerApp.Wpf;

public partial class OrganizeWindow : Window
{
    public string SelectedFolder { get; private set; }
    public string SelectedDestination { get; private set; } = string.Empty;
    public string OperationMode => CopyRadioButton.IsChecked == true ? "copy" : "move";
    public OrganizeOptions Options { get; private set; } = new();
    public List<string> FileTypes { get; } = [];

    public OrganizeWindow(string? folder)
    {
        InitializeComponent();
        SelectedFolder = folder ?? string.Empty;
        SourceTextBox.Text = SelectedFolder;
    }

    private void BrowseSource_Click(object sender, RoutedEventArgs e)
    {
        var folder = FolderPicker.Pick(SelectedFolder, "Select source folder");
        if (folder != null)
        {
            SelectedFolder = folder;
            SourceTextBox.Text = folder;
        }
    }

    private void BrowseDestination_Click(object sender, RoutedEventArgs e)
    {
        var folder = FolderPicker.Pick(SelectedDestination, "Select destination folder");
        if (folder != null)
        {
            SelectedDestination = folder;
            DestinationTextBox.Text = folder;
        }
    }

    private void Organize_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SourceTextBox.Text) || string.IsNullOrWhiteSpace(DestinationTextBox.Text))
        {
            MessageBox.Show(this, "Select both source and destination folders.", "Organize", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SelectedFolder = SourceTextBox.Text;
        SelectedDestination = DestinationTextBox.Text;
        Options = new OrganizeOptions
        {
            CreateSubfolders = CreateSubfoldersCheckBox.IsChecked == true,
            OverwriteExisting = OverwriteExistingCheckBox.IsChecked == true
        };

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
