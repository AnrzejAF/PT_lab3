using System.Windows;
using System.Globalization;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Controls;

namespace PT_LAB
{
    public partial class MainWindow : Window
    {
        // Private fields
        private FileExplorer _fileExplorer;

        // Constructor
        public MainWindow()
        {
            InitializeComponent();
            _fileExplorer = new FileExplorer();
            DataContext = _fileExplorer;
            _fileExplorer.PropertyChanged += _fileExplorer_PropertyChanged;
            _fileExplorer.OnOpenFileRequest += _fileExplorer_OnOpenFileRequest;

        }

        private void _fileExplorer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FileExplorer.Lang))
                CultureResources.ChangeCulture(CultureInfo.CurrentUICulture);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void _fileExplorer_OnOpenFileRequest(object sender, FileInfoViewModel viewModel)
        {
            var content = _fileExplorer.GetFileContent(viewModel);
            if (content is string text)
            {
                var textView = new TextBlock { Text = text };
                ContentViewer.Content = textView;
            }
        }
    }
}
