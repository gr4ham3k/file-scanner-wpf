using FileScannerApp.Models;
using FileScannerApp.Services;
using FileScannerApp.Wpf.Helpers;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using MessageBox = System.Windows.MessageBox;

namespace FileScannerApp.Wpf;

public partial class MainWindow : Window
{
    private readonly Database database;
    private readonly ScanService scanService;
    private readonly FileOperationsService fileOperationsService;
    private List<FileData> currentFiles = [];
    private string? selectedPath;

    public MainWindow()
    {
        InitializeComponent();

        var config = AppConfig.Load();
        database = new Database();
        scanService = new ScanService(config, database);
        fileOperationsService = new FileOperationsService(database);

        FilterComboBox.ItemsSource = FileTypeCatalog.Filters;
        FilterComboBox.SelectedIndex = 0;
        PreviewContent.Content = PreviewFactory.Create(null);
    }

    private void SelectFolder_Click(object sender, RoutedEventArgs e)
    {
        var folder = FolderPicker.Pick(selectedPath, "Select a folder to browse");
        if (folder != null)
        {
            LoadFolder(folder);
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(selectedPath))
        {
            SelectFolder_Click(sender, e);
            return;
        }

        LoadFolder(selectedPath);
    }

    private void LoadFolder(string path)
    {
        try
        {
            selectedPath = path;
            SelectedPathTextBlock.Text = path;
            currentFiles = FileScannerService.Map(FileScannerService.Scan(path));
            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Folder error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyFilter()
    {
        var filter = FilterComboBox.SelectedItem as FileTypeFilter;
        IEnumerable<FileData> filtered = currentFiles;

        if (filter != null && filter.Extensions.Count > 0)
        {
            filtered = filtered.Where(file => filter.Extensions.Contains(file.Extension.ToLowerInvariant()));
        }

        FilesGrid.ItemsSource = filtered.ToList();
        UpdateStatus();
    }

    private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded)
        {
            ApplyFilter();
        }
    }

    private void FilesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var file = FilesGrid.SelectedItem as FileData;
        PreviewContent.Content = PreviewFactory.Create(file?.Path);
        UpdateStatus();
    }

    private List<FileData> GetSelectedFiles()
    {
        return FilesGrid.SelectedItems.Cast<FileData>().ToList();
    }

    private async void Scan_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(selectedPath))
        {
            MessageBox.Show(this, "Select a folder first.", "Scan", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new ScanOptionsWindow(selectedPath) { Owner = this };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        selectedPath = dialog.SelectedFolder;
        SelectedPathTextBlock.Text = selectedPath;

        var files = FileScannerService.Map(FileScannerService.Scan(selectedPath));
        if (dialog.FileTypes.Count > 0)
        {
            files = files.Where(file => dialog.FileTypes.Contains(file.Extension.ToLowerInvariant())).ToList();
        }

        if (files.Count == 0)
        {
            MessageBox.Show(this, "No files matched the selected scan criteria.", "Scan", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        int scanId = scanService.CreateScan(selectedPath, files.Count);
        ScanProgressBar.Visibility = Visibility.Visible;
        ScanProgressBar.Minimum = 0;
        ScanProgressBar.Maximum = files.Count;
        ScanProgressBar.Value = 0;
        ProgressTextBlock.Text = "Scan in progress...";

        int threats = await scanService.ScanFilesAsync(
            files,
            scanId,
            progress =>
            {
                Dispatcher.Invoke(() =>
                {
                    ScanProgressBar.Value = progress.Current;
                    ProgressTextBlock.Text = $"Scanning {progress.Current}/{progress.Total}: {progress.CurrentFile}";
                });

                return Task.CompletedTask;
            });

        ScanProgressBar.Visibility = Visibility.Collapsed;
        ProgressTextBlock.Text = $"Scan completed. Threats found: {threats}";
        LoadFolder(selectedPath);

        MessageBox.Show(this, $"Scan completed. Found {threats} threat(s).", "Scan", MessageBoxButton.OK, MessageBoxImage.Information);
        new ScanResultsWindow(scanId) { Owner = this }.Show();
    }

    private void Organize_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(selectedPath))
        {
            MessageBox.Show(this, "Select a folder first.", "Organize", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new OrganizeWindow(selectedPath) { Owner = this };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var files = FileScannerService.Map(FileScannerService.Scan(dialog.SelectedFolder));

        Organizer.OrganizeFiles(
            files,
            dialog.SelectedFolder,
            dialog.SelectedDestination,
            dialog.FileTypes,
            dialog.OperationMode,
            dialog.Options.CreateSubfolders,
            dialog.Options.OverwriteExisting);

        MessageBox.Show(this, "Files organized.", "Organize", MessageBoxButton.OK, MessageBoxImage.Information);
        LoadFolder(dialog.SelectedFolder);

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = dialog.SelectedDestination,
            UseShellExecute = true
        });
    }

    private void RenameBatch_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new RenameWindow(selectedPath) { Owner = this };
        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.SelectedFolder))
        {
            LoadFolder(dialog.SelectedFolder);
        }
    }

    private void History_Click(object sender, RoutedEventArgs e)
    {
        var win = new HistoryWindow();
        win.Show();
    }

    private void DeleteSelected_Click(object sender, RoutedEventArgs e)
    {
        var selectedFiles = GetSelectedFiles();
        if (selectedFiles.Count == 0)
        {
            return;
        }

        PreviewFactory.ClearPreview(PreviewContent);

        var result = MessageBox.Show(
            this,
            $"Delete {selectedFiles.Count} file(s)?",
            "Confirm delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        var paths = selectedFiles.Select(file => file.Path).ToList();
        var summary = fileOperationsService.DeleteFiles(paths);

        currentFiles = currentFiles.Where(file => !paths.Contains(file.Path)).ToList();
        ApplyFilter();

        MessageBox.Show(
            this,
            $"Delete completed.\n\nDeleted: {summary.deleted}\nFailed: {summary.failed}",
            "Delete",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void MoveSelected_Click(object sender, RoutedEventArgs e)
    {
        var selectedFiles = GetSelectedFiles();
        if (selectedFiles.Count == 0)
        {
            return;
        }

        var destination = FolderPicker.Pick(selectedPath, "Select destination folder");
        if (destination == null)
        {
            return;
        }

        PreviewFactory.ClearPreview(PreviewContent);

        var result = fileOperationsService.MoveFiles(selectedFiles.Select(file => file.Path).ToList(), destination);
        LoadFolder(selectedPath!);

        MessageBox.Show(
            this,
            $"Move completed.\n\nMoved: {result.moved}\nSkipped: {result.skipped}",
            "Move",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void RenameSelected_Click(object sender, RoutedEventArgs e)
    {
        var file = FilesGrid.SelectedItem as FileData;
        if (file == null)
        {
            return;
        }

        var dialog = new TextInputWindow("Rename file", "Rename selected file", file.Name) { Owner = this };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        PreviewFactory.ClearPreview(PreviewContent);

        var result = fileOperationsService.RenameFile(file.Path, dialog.Value);
        if (!result.success)
        {
            MessageBox.Show(this, result.error, "Rename", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        LoadFolder(selectedPath!);
    }

    private void UpdateStatus()
    {
        int total = FilesGrid.Items.Count;
        int selected = FilesGrid.SelectedItems.Count;

        TotalTextBlock.Text = $"Total: {total}";
        SelectedTextBlock.Text = $"Selected: {selected}";
    }
}
