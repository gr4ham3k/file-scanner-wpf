using FileScannerApp.Models;
using FileScannerApp.Services;
using System.Windows;
using System.Windows.Controls;
using MessageBox = System.Windows.MessageBox;

namespace FileScannerApp.Wpf;

public partial class HistoryWindow : Window
{
    private OperationLog? selectedLog;
    private readonly Action _refreshMain;

    public HistoryWindow(Action refreshMain)
    {
        InitializeComponent();
        _refreshMain = refreshMain;

        Loaded += (_, _) =>
        {
            UndoButton.IsEnabled = false;
            LoadScans();
            LoadOperations();
        };
    }

    private void LoadScans()
    {
        var db = new Database();
        var scans = db.GetAllScansWithFiles();

        ScansTreeView.Items.Clear();

        foreach (var scanGroup in scans.GroupBy(s => (int)s.ScanId).OrderByDescending(group => group.Key))
        {
            dynamic first = scanGroup.First();
            var root = new TreeViewItem
            {
                Header = $"Scan #{first.ScanId} | {first.ScanDate} | {first.FilesCount} files | Threats: {first.ThreatsFound} | {first.ScanStatus}"
            };

            foreach (var file in scanGroup.Where(item => item.FileName != null))
            {
                root.Items.Add(new TreeViewItem
                {
                    Header = $"{file.FileName} | Status: {file.FileStatus} | Malicious: {file.Malicious}"
                });
            }

            root.IsExpanded = true;
            ScansTreeView.Items.Add(root);
        }
    }

    private void LoadOperations()
    {
        var db = new Database();
        var logs = db.GetOperationsLog();

        OperationsTreeView.Items.Clear();

        foreach (var log in logs)
        {
            var root = new TreeViewItem
            {
                Header = $"[{log.OperationDate:yyyy-MM-dd HH:mm}] {log.OperationType} | {log.FileName}",
                Tag = log
            };

            if (!string.IsNullOrWhiteSpace(log.OldPath))
            {
                root.Items.Add(new TreeViewItem { Header = "Old: " + log.OldPath });
            }

            if (!string.IsNullOrWhiteSpace(log.NewPath))
            {
                root.Items.Add(new TreeViewItem { Header = "New: " + log.NewPath });
            }

            OperationsTreeView.Items.Add(root);
        }
    }

    private void OperationsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        selectedLog = (e.NewValue as TreeViewItem)?.Tag as OperationLog;
        UndoButton.IsEnabled = selectedLog?.CanUndo == true;
    }

    private void Undo_Click(object sender, RoutedEventArgs e)
    {
        if (selectedLog == null)
        {
            return;
        }

        bool success;
        try
        {
            success = UndoService.Undo(selectedLog);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Undo failed: " + ex.Message, "History", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (!success)
        {
            MessageBox.Show(this, "Undo failed.", "History", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        new Database().DeleteOperationLog(selectedLog.Id);
        selectedLog = null;
        UndoButton.IsEnabled = false;
        LoadOperations();

        _refreshMain?.Invoke();

        MessageBox.Show(this, "Undo completed.", "History", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
