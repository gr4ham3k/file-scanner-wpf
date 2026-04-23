using FileScannerApp.Models;
using FileScannerApp.Wpf.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;

namespace FileScannerApp.Wpf;

public partial class RenameWindow : Window
{
    private List<RenamePreview> previews = [];

    public string? SelectedFolder { get; private set; }

    public RenameWindow(string? folder)
    {
        InitializeComponent();
        SelectedFolder = folder;
        FolderTextBox.Text = folder ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(folder))
        {
            LoadFiles();
        }
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var folder = FolderPicker.Pick(SelectedFolder, "Select folder to rename");
        if (folder != null)
        {
            SelectedFolder = folder;
            FolderTextBox.Text = folder;
            LoadFiles();
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
                item.NameBefore,
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
        UpdatePreview();
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
            PatternTextBox.Text += item.Content?.ToString();
            PatternTextBox.CaretIndex = PatternTextBox.Text.Length;
            PatternTextBox.Focus();
        }
    }

    private void Rename_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SelectedFolder))
        {
            MessageBox.Show(this, "Select a folder first.", "Rename", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            RenameService.RenameFiles(previews);
            MessageBox.Show(this, "Rename completed.", "Rename", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Rename", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
