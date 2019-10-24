using DocumentCrossReference.Library;
using DocumentCrossReference.Library.Importers;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DocumentCrossReference.WpfApp
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        private Dictionary<string, List<DocumentTextLocation>> _allTexts;

        public MainView() => InitializeComponent();

        private void BtnBrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            var d = new CommonOpenFileDialog() { Title = "Select Documents Root Folder", IsFolderPicker = true };
            if (d.ShowDialog() != CommonFileDialogResult.Ok) { return; }

            txtFolderPath.Text = d.FileName;
            LoadFolder();
        }

        private void BtnLoadFolder_Click(object sender, RoutedEventArgs e) => LoadFolder();
        private void LoadFolder()
        {
            var dir = txtFolderPath.Text;

            Task.Run(() =>
            {
                var filePaths = Directory.GetFiles(dir, "*.pdf", SearchOption.AllDirectories);

                var indexes = new List<DocumentTextIndex>();
                foreach (var filePath in filePaths)
                {
                    var index = PdfImporter.IndexPdfDocument(filePath);
                    indexes.Add(index);

                    Dispatcher.Invoke(() =>
                    {
                        txtStatus.Text = $"Loaded {filePath.Replace(dir, "")}";
                        _allTexts = indexes
                            .SelectMany(x => x.TextEntries.Values)
                            .SelectMany(x => x)
                            // Index by normalized text
                            .SelectMany(x => x.Text.ToUpperInvariant().Split(';', ',', '.').Select(textPart => new { text = textPart.Trim(), x }))
                            .Where(x => !string.IsNullOrWhiteSpace(x.text))
                            .GroupBy(x => x.text)
                            .ToDictionary(g => g.Key, g => g.Select(x => x.x).ToList());

                        PopulateTerms();
                    });
                }

                txtStatus.Text = "";
            });
        }
        private void TxtFilter_TextChanged(object sender, TextChangedEventArgs e) => PopulateTerms();
        private void PopulateTerms() => lstTerms.ItemsSource = _allTexts.Keys.Where(x => x.Contains(txtFilter.Text)).OrderBy(x => x).ToList();

        private void LstTerms_SelectionChanged(object sender, SelectionChangedEventArgs e) => DisplayTexts(lstTerms.SelectedItem as string ?? "");
        private void DisplayTexts(string text)
        {
            var dir = txtFolderPath.Text;
            var results = _allTexts.ContainsKey(text) ? _allTexts[text] : new List<DocumentTextLocation>();
            lstResults.ItemsSource = results
                .OrderBy(x => x.DocumentFilePath)
                .ThenBy(x => x.PageIndex)
                .Select(x => new DocumentTextLocation_ViewModel() { Value = x, Text = $"{x.DocumentFilePath.Replace(dir, "")} pg={x.PageIndex + 1}" })
                .Distinct()
                .ToList();
        }

        private class DocumentTextLocation_ViewModel
        {
            public string Text { get; set; }
            public DocumentTextLocation Value { get; set; }
            public override string ToString() => Text;
        }

        private void LstResults_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedItem = lstResults.SelectedItem as DocumentTextLocation_ViewModel;
            var filePath = selectedItem.Value.DocumentFilePath;
            Process.Start(filePath);
        }


    }
}
