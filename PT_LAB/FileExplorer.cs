using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PT_LAB
{
    public class FileExplorer : ViewModelBase
    {
        public static readonly string[] TextFilesExtensions = new string[] { ".txt", ".ini", ".log" };

        public ICommand OpenFileCommand { get; private set; }

        private SortOptions _currentSortOptions = new SortOptions();

        public SortOptions CurrentSortOptions
        {
            get { return _currentSortOptions; }
            set { _currentSortOptions = value; NotifyPropertyChanged(); }
        }
        public FileExplorer()
        {
            NotifyPropertyChanged(nameof(Lang));
            OpenRootFolderCommand = new RelayCommand(OpenRootFolderExecute);
            SortRootFolderCommand = new RelayCommand(SortRootFolderExecute, CanSortRootFolderExecute);
            OpenFileCommand = new RelayCommand(OpenFileExecute, OpenFileCanExecute);

        }

        public DirectoryInfoViewModel? Root { get; set; }
        public RelayCommand OpenRootFolderCommand { get; private set; }
        public RelayCommand SortRootFolderCommand { get; private set; }

        public void OpenRoot(string path)
        {
            Root = new DirectoryInfoViewModel(this);
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

        private bool CanSortRootFolderExecute(object parameter)
        {
            return Root != null;
        }

        public void SortRootFolderExecute(object parameter)
        {
            SortDialog dlg = new SortDialog(CurrentSortOptions);
            if (dlg.ShowDialog() == true)
            {
                Root.Sort(dlg.SortOptions);
                NotifyPropertyChanged(nameof(Root));
            }
        }
        private bool OpenFileCanExecute(object parameter)
        {
            if (parameter is FileInfoViewModel viewModel)
            {
                var extension = viewModel.Extension?.ToLower();
                return TextFilesExtensions.Contains(extension);
            }
            return false;
        }

        private void OpenFileExecute(object parameter)
        {
            if (parameter is FileInfoViewModel viewModel)
            {
                OnOpenFileRequest?.Invoke(this, viewModel);
            }
        }

        public event EventHandler<FileInfoViewModel> OnOpenFileRequest;

        public object GetFileContent(FileInfoViewModel viewModel)
        {
            var extension = viewModel.Extension?.ToLower();
            if (TextFilesExtensions.Contains(extension))
            {
                return GetTextFileContent(viewModel);
            }
            return null;
        }

        private string GetTextFileContent(FileInfoViewModel viewModel)
        {
            return System.IO.File.ReadAllText(viewModel.Model.FullName);
        }

    }
}
