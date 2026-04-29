using FileScannerApp.Models;
using FileScannerApp.Wpf.Helpers;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;

namespace FileScannerApp.Wpf;

public partial class BatchTools : Window
{
    private List<RenamePreview> previews = [];
    private readonly Database database = new();

    public string? SelectedFolder { get; private set; }
    public string SelectedDestination { get; private set; } = string.Empty;
    public string OperationMode => CopyRadioButton.IsChecked == true ? "copy" : "move";
    public OrganizeOptions Options { get; private set; } = new();
    public List<string> FileTypes { get; } = [];

    public BatchTools(string? folder)
    {
        InitializeComponent();

        SelectedFolder = folder;
        FolderTextBox.Text = folder ?? string.Empty;
        PatternTextBox.Text = "{name}_{counter}";

        if (!string.IsNullOrWhiteSpace(folder))
        {
            LoadFiles();
        }
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var folder = FolderPicker.Pick(SelectedFolder, "Select source folder");
        if (folder != null)
        {
            SelectedFolder = folder;
            FolderTextBox.Text = folder;
            LoadFiles();
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

    private void LoadFiles()
    {
        if (string.IsNullOrWhiteSpace(SelectedFolder))
        {
            previews = [];
            PreviewGrid.ItemsSource = null;
            return;
        }

        previews = RenameService.LoadPreview(SelectedFolder);
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (previews.Count == 0)
        {
            PreviewGrid.ItemsSource = null;
            return;
        }

        int counter = 1;

        foreach (var item in previews)
        {
            item.NameAfter = RenameService.GenerateNewName(
                PatternTextBox.Text,
                item.FullPath,
                counter,
                GetSelectedOption());

            counter++;
        }

        PreviewGrid.ItemsSource = null;
        PreviewGrid.ItemsSource = previews;
    }

    private string GetSelectedOption()
    {
        if (UpperRadioButton.IsChecked == true) return "upper";
        if (LowerRadioButton.IsChecked == true) return "lower";
        if (CapitalizeRadioButton.IsChecked == true) return "capitalize";
        return "none";
    }

    private void PatternTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (IsLoaded)
        {
            UpdatePreview();
        }
    }

    private void CaseOption_Checked(object sender, RoutedEventArgs e)
    {
        if (IsLoaded)
        {
            UpdatePreview();
        }
    }

    private void TokensListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (TokensListBox.SelectedItem is ListBoxItem item)
        {
            string token = item.Content?.ToString() ?? "";
            int caretIndex = PatternTextBox.CaretIndex;

            PatternTextBox.Text = PatternTextBox.Text.Insert(caretIndex, token);
            PatternTextBox.CaretIndex = caretIndex + token.Length;
            PatternTextBox.Focus();
        }
    }

    private void ApplyChanges_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SelectedFolder))
        {
            MessageBox.Show(this, "Select a source folder first.", "Batch tools", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (previews.Count == 0)
        {
            MessageBox.Show(this, "No files to process.", "Batch tools", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

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

        try
        {
            bool hasOrganizeStep = !string.IsNullOrWhiteSpace(DestinationTextBox.Text);
            int organizedFiles = 0;

            if (hasOrganizeStep)
            {
                SelectedDestination = DestinationTextBox.Text;

                var files = FileScannerService.Map(FileScannerService.Scan(SelectedFolder));
                if (FileTypes.Count > 0)
                {
                    files = files.Where(file => FileTypes.Contains(file.Extension.ToLowerInvariant())).ToList();
                }

                organizedFiles = files.Count;

                Organizer.OrganizeFiles(files, SelectedFolder, SelectedDestination, FileTypes, OperationMode,
                                        Options.CreateSubfolders, Options.OverwriteExisting, database, previews);

                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = SelectedDestination,
                    UseShellExecute = true
                });
            }

            string summary = $"Organize step: {(hasOrganizeStep ? $"{OperationMode} {organizedFiles} file(s)" : "not used")}";

            MessageBox.Show(this, summary, "Batch tools", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Batch tools", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
