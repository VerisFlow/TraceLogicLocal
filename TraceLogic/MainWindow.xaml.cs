using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TraceLogic.Core.Exporting;
using TraceLogic.Core.Models;
using TraceLogic.Core.Parsing;


namespace TraceLogic
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private TraceAnalysisResult? _analysisResult;
        public TraceAnalysisResult? AnalysisResult
        {
            get => _analysisResult;
            set
            {
                _analysisResult = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Custom Title Bar Logic
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { this.DragMove(); }
        private void Close_Click(object sender, RoutedEventArgs e) { Application.Current.Shutdown(); }
        private void Minimize_Click(object sender, RoutedEventArgs e) { this.WindowState = WindowState.Minimized; }
        private void Maximize_Restore_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }
        #endregion

        #region File Processing Logic
        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Trace files (*.trc)|*.trc|All files (*.*)|*.*", Title = "Select a Hamilton Venus Trace File" };
            if (openFileDialog.ShowDialog() == true) { ProcessFile(openFileDialog.FileName); }
        }
        private void MainContent_Drop(object sender, DragEventArgs e)
        {
            DragDropOverlay.Visibility = Visibility.Collapsed;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var trcFile = files.FirstOrDefault(f => Path.GetExtension(f).Equals(".trc", StringComparison.OrdinalIgnoreCase));
                if (trcFile != null) { ProcessFile(trcFile); }
                else { MessageBox.Show("Please drop a valid .trc file.", "Invalid File Type", MessageBoxButton.OK, MessageBoxImage.Warning); }
            }
        }
        private void MainContent_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) { e.Effects = DragDropEffects.Copy; DragDropOverlay.Visibility = Visibility.Visible; }
            else { e.Effects = DragDropEffects.None; }
        }
        private void MainContent_DragLeave(object sender, DragEventArgs e) { DragDropOverlay.Visibility = Visibility.Collapsed; }
        private void ProcessFile(string filePath)
        {
            this.AnalysisResult = null; 
            WelcomeMessage.Visibility = Visibility.Visible;
            DataTabs.Visibility = Visibility.Collapsed;

            var parser = new TraceFileParser();
            var analysisResult = parser.Parse(filePath);

            if (analysisResult.Errors.Any())
            {
                StatusTextBlock.Text = "Error processing file.";
                MessageBox.Show(string.Join("\n", analysisResult.Errors), "Parsing Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.AnalysisResult = analysisResult;

            StatusTextBlock.Text = $"Successfully parsed {AnalysisResult.LiquidTransfers.Count} liquid transfer events from {AnalysisResult.FileName}.";
            WelcomeMessage.Visibility = Visibility.Collapsed;
            DataTabs.Visibility = Visibility.Visible;
            DataTabs.SelectedIndex = 0; // Focus on the new tab
        }
        #endregion

        #region Export and UI Logic

        /// <summary>
        /// Handles the click event for the new About button.
        /// </summary>
        private void About_Click(object sender, RoutedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox
            {
                // This ensures the About Box opens centered over the MainWindow
                Owner = this
            };
            aboutBox.ShowDialog();
        }

        /// <summary>
        /// Handles the click event for the Export button.
        /// Logic is simplified to only export to CSV.
        /// </summary>
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (AnalysisResult?.LiquidTransfers == null || !AnalysisResult.LiquidTransfers.Any())
            {
                MessageBox.Show("There is no data to export.", "Export Data", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                // Filter is now hardcoded for CSV only
                Filter = "CSV File (*.csv)|*.csv",
                Title = "Export Liquid Transfer Data",
                FileName = $"{Path.GetFileNameWithoutExtension(AnalysisResult.FileName)}_Export"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Get the currently visible and ordered columns from the DataGrid
                    var columnsToExport = LiquidTransferGrid.Columns
                        .Where(c => c.Visibility == Visibility.Visible)
                        .OrderBy(c => c.DisplayIndex)
                        .Select(c => new DataGridColumnInfo
                        {
                            Header = c.Header?.ToString() ?? string.Empty,
                            PropertyName = (c.ClipboardContentBinding as System.Windows.Data.Binding)?.Path.Path ?? string.Empty
                        })
                        .ToList();

                    // Directly call the CSV export method
                    DataExporter.ExportToCsv(AnalysisResult.LiquidTransfers, columnsToExport, saveFileDialog.FileName);

                    MessageBox.Show($"Data successfully exported to:\n{saveFileDialog.FileName}", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred during export:\n{ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Handles the Checked/Unchecked events for the column visibility checkboxes.
        /// </summary>
        private void ColumnCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag != null 
                && int.TryParse(checkBox.Tag.ToString(), out int columnIndex))
            {
                if (columnIndex >= 0 && columnIndex < LiquidTransferGrid.Columns.Count)
                {
                    LiquidTransferGrid.Columns[columnIndex].Visibility = (checkBox.IsChecked == true)
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
            }
        }

        #endregion

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

