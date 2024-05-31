using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Specialized;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows;


namespace PT_LAB
{
    public class FileExplorer : ViewModelBase
    {

        private CancellationTokenSource _cancellationTokenSource;

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                NotifyPropertyChanged();
            }
        }

        public static readonly string[] TextFilesExtensions = new string[] { ".txt", ".ini", ".log" };

        public ICommand OpenFileCommand { get; private set; }

        private SortOptions _currentSortOptions = new SortOptions();

        public SortOptions CurrentSortOptions
        {
            get { return _currentSortOptions; }
            set { _currentSortOptions = value; NotifyPropertyChanged(); }
        }

        private DirectoryInfoViewModel _root;
        public DirectoryInfoViewModel Root
        {
            get => _root;
            set
            {
                if (_root != null)
                {
                    _root.PropertyChanged -= Root_PropertyChanged;
                }
                _root = value;
                if (_root != null)
                {
                    _root.PropertyChanged += Root_PropertyChanged;
                }
                NotifyPropertyChanged();
            }
        }
        public RelayCommand OpenRootFolderCommand { get; private set; }
        public RelayCommand SortRootFolderCommand { get; private set; }



        public FileExplorer()
        {
            NotifyPropertyChanged(nameof(Lang));
            OpenRootFolderCommand = new RelayCommand(OpenRootFolderExecuteAsync);
            SortRootFolderCommand = new RelayCommand(SortRootFolderExecute, CanSortRootFolderExecute);
            OpenFileCommand = new RelayCommand(OpenFileExecute, OpenFileCanExecute);

            if (Root != null)
            {
                Root.PropertyChanged += Root_PropertyChanged;
                Root.Items.CollectionChanged += Items_CollectionChanged;
            }
        }

        public void OpenRoot(string path)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Root = new DirectoryInfoViewModel(this);
                Root.Open(path);
                NotifyPropertyChanged(nameof(Root));
            });
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

        private async void OpenRootFolderExecuteAsync(object parameter)
        {
            var description = Strings.Description;
            var dlg = new System.Windows.Forms.FolderBrowserDialog() { Description = description };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var path = dlg.SelectedPath;
                StatusMessage = "Loading directory...";
                NotifyPropertyChanged(nameof(StatusMessage));
                await Task.Run(() =>
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        OpenRoot(path);
                        NotifyPropertyChanged(nameof(Root));
                        StatusMessage = "Ready";
                        NotifyPropertyChanged(nameof(StatusMessage));
                    });
                });
            }
        }

        private bool CanSortRootFolderExecute(object parameter)
        {
            return Root != null;
        }

        public async void SortRootFolderExecute(object parameter)
        {
            SortDialog dlg = new SortDialog(CurrentSortOptions);
            if (dlg.ShowDialog() == true)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                StatusMessage = "Sorting directory...";
                NotifyPropertyChanged(nameof(StatusMessage));
                await Task.Delay(10);

                try
                {
                    await Root.SortAsync(dlg.SortOptions, _cancellationTokenSource.Token);
                    if (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        StatusMessage = "Directory sorted";
                        NotifyPropertyChanged(nameof(StatusMessage));
                        await Task.Delay(500);
                    }
                }
                catch (OperationCanceledException)
                {
                    StatusMessage = "Sorting cancelled.";
                    NotifyPropertyChanged(nameof(StatusMessage));
                    await Task.Delay(500);
                }
                finally
                {
                    _cancellationTokenSource = null;
                    NotifyPropertyChanged(nameof(StatusMessage));
                }
            }
        }

        public ICommand CancelCommand => new RelayCommand(Cancel);

        private void Cancel(object parameter)
        {
            _cancellationTokenSource?.Cancel();
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


        private void Root_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "StatusMessage" && sender is DirectoryInfoViewModel viewModel)
            {
                this.StatusMessage = viewModel.StatusMessage;
            }
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems.Cast<FileSystemInfoViewModel>())
                    {
                        item.PropertyChanged += Item_PropertyChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems.Cast<FileSystemInfoViewModel>())
                    {
                        item.PropertyChanged -= Item_PropertyChanged;
                    }
                    break;
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "StatusMessage" && sender is FileSystemInfoViewModel viewModel)
            {
                this.StatusMessage = viewModel.StatusMessage;
            }
        }
    }
}
