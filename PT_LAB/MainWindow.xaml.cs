using System.Windows;
using System.Globalization;
using System.Windows.Data;
using System.ComponentModel;

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

        //private void Open_Click(object sender, RoutedEventArgs e)
        //{
        //    var dlg = new FolderBrowserDialog();
        //    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        var path = dlg.SelectedPath;
        //        _fileExplorer.OpenRoot(path);
        //        DataContext = _fileExplorer;
        //    }
        //}

        public class FileExplorer : ViewModelBase
        {
            public FileExplorer()
            {
                NotifyPropertyChanged(nameof(Lang));
                OpenRootFolderCommand = new RelayCommand(OpenRootFolderExecute);
            }
            public DirectoryInfoViewModel? Root { get; set; }

            public void OpenRoot(string path)
            {
                Root = new DirectoryInfoViewModel();
                Root.Open(path);
            }

            public string Lang
            {
                get { return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName; }
                set
                {
                    if (value != null)
                    {
                        if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName != value)
                        {
                            CultureInfo.CurrentUICulture = new CultureInfo(value);
                            NotifyPropertyChanged();
                        }
                    }
                }
            }

            public RelayCommand OpenRootFolderCommand { get; private set; }

            private void OpenRootFolderExecute(object parameter)
            {
                var description = Strings.Description;
                var dlg = new System.Windows.Forms.FolderBrowserDialog() { Description = description };
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var path = dlg.SelectedPath;
                    OpenRoot(path);
                    NotifyPropertyChanged(nameof(Root));
                }
            }
        }
    }
}
