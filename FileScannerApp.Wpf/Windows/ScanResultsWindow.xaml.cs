using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MediaColor = System.Windows.Media.Color;

namespace FileScannerApp.Wpf;

public partial class ScanResultsWindow : Window
{
    private readonly int scanId;
    private readonly Database database = new();

    public ScanResultsWindow(int scanId)
    {
        InitializeComponent();
        this.scanId = scanId;

        Loaded += (_, _) =>
        {
            ResultsGrid.ItemsSource = database.GetFilesForScan(this.scanId);
        };
    }

    private void ResultsGrid_LoadingRow(object sender, DataGridRowEventArgs e)
    {
        var item = e.Row.Item;
        var property = item.GetType().GetProperty("Malicious", BindingFlags.Public | BindingFlags.Instance);
        var value = property?.GetValue(item)?.ToString();

        e.Row.Background = value == "Yes"
            ? new SolidColorBrush(MediaColor.FromRgb(254, 205, 211))
            : new SolidColorBrush(MediaColor.FromRgb(220, 252, 231));
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
