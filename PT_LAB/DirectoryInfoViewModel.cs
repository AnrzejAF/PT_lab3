using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PT_LAB
{
    public class DirectoryInfoViewModel : FileSystemInfoViewModel
    {
        private DispatchedObservableCollection<FileSystemInfoViewModel> _items;
        private bool _isLoaded = false;
        private FileSystemWatcher _watcher;
        private SortOptions _sortOptions;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public DispatchedObservableCollection<FileSystemInfoViewModel> Items
        {
            get => _items;
            set
            {
                if (_items != value)
                {
                    _items = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public DirectoryInfoViewModel(ViewModelBase owner) : base(owner)
        {
            Items = new DispatchedObservableCollection<FileSystemInfoViewModel>();
            Items.CollectionChanged += Items_CollectionChanged;
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

        public async Task LoadChildrenAsync()
        {
            if (_isLoaded)
                return;

            _isLoaded = true;

            try
            {
                if (Model == null)
                {
                    StatusMessage = "Error: Model is not set.";
                    NotifyPropertyChanged(nameof(StatusMessage));
                    return;
                }

                StatusMessage = $"Loading directory: {Model.FullName}";
                NotifyPropertyChanged(nameof(StatusMessage));

                var directories = await Task.Run(() => Directory.GetDirectories(Model.FullName));
                var files = await Task.Run(() => Directory.GetFiles(Model.FullName));

                foreach (var dir in directories)
                {
                    var dirInfo = new DirectoryInfo(dir);
                    var itemViewModel = new DirectoryInfoViewModel(this) { Model = dirInfo };
                    App.Current.Dispatcher.Invoke(() => Items.Add(itemViewModel));
                    await itemViewModel.LoadChildrenAsync();
                }

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    var itemViewModel = new FileInfoViewModel(this) { Model = fileInfo };
                    App.Current.Dispatcher.Invoke(() => Items.Add(itemViewModel));
                }

                StatusMessage = $"Loaded directory: {Model.FullName}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading directory: {ex.Message}";
            }
            finally
            {
                NotifyPropertyChanged(nameof(StatusMessage));
            }
        }

        public async Task<bool> Open(string path, SortOptions sortOptions = null)
        {
            bool result = false;

            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    StatusMessage = "Error: Path is not provided.";
                    NotifyPropertyChanged(nameof(StatusMessage));
                    return false;
                }

                Model = new DirectoryInfo(path);

                StatusMessage = "Loading directory:";
                NotifyPropertyChanged(nameof(StatusMessage));

                _watcher = new FileSystemWatcher(path)
                {
                    EnableRaisingEvents = true
                };
                _watcher.Created += OnFileSystemChanged;
                _watcher.Renamed += OnFileSystemChanged;
                _watcher.Deleted += OnFileSystemChanged;
                _watcher.Changed += OnFileSystemChanged;
                _watcher.Error += Watcher_Error;

                await LoadChildrenAsync();

                if (sortOptions != null)
                {
                    _sortOptions = sortOptions;
                    await SortAsync(_sortOptions, _cancellationTokenSource.Token);
                }

                StatusMessage = "Directory loaded";
                NotifyPropertyChanged(nameof(StatusMessage));
                result = true;
            }
            catch (Exception ex)
            {
                Exception = ex;
                StatusMessage = "Error loading directory";
                NotifyPropertyChanged(nameof(StatusMessage));
            }

            return result;
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            Exception = e.GetException();
        }

        private async void OnFileSystemChanged(object sender, FileSystemEventArgs e)
        {
            await Task.Delay(500);
            await App.Current.Dispatcher.InvokeAsync(async () => await OnFileSystemChangedAsync(e));
        }

        private async Task OnFileSystemChangedAsync(FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    if (File.Exists(e.FullPath))
                    {
                        var fileInfo = new FileInfo(e.FullPath);
                        var itemViewModel = new FileInfoViewModel(this)
                        {
                            Model = fileInfo
                        };
                        App.Current.Dispatcher.Invoke(() => Items.Add(itemViewModel));
                    }
                    else if (Directory.Exists(e.FullPath))
                    {
                        var dirInfo = new DirectoryInfo(e.FullPath);
                        var itemViewModel = new DirectoryInfoViewModel(this)
                        {
                            Model = dirInfo
                        };
                        App.Current.Dispatcher.Invoke(() => Items.Add(itemViewModel));
                        await itemViewModel.LoadChildrenAsync();
                    }
                    break;
                case WatcherChangeTypes.Deleted:
                    var deletedItem = Items.FirstOrDefault(item => item.Model.FullName == e.FullPath);
                    if (deletedItem != null)
                    {
                        App.Current.Dispatcher.Invoke(() => Items.Remove(deletedItem));
                    }
                    break;
                case WatcherChangeTypes.Renamed:
                    var renamedEventArgs = e as RenamedEventArgs;
                    if (renamedEventArgs != null)
                    {
                        var oldItem = Items.FirstOrDefault(item => item.Model.FullName == renamedEventArgs.OldFullPath);
                        if (oldItem != null)
                        {
                            App.Current.Dispatcher.Invoke(() => Items.Remove(oldItem));
                        }

                        if (File.Exists(renamedEventArgs.FullPath))
                        {
                            var fileInfo = new FileInfo(renamedEventArgs.FullPath);
                            var itemViewModel = new FileInfoViewModel(this)
                            {
                                Model = fileInfo
                            };
                            App.Current.Dispatcher.Invoke(() => Items.Add(itemViewModel));
                        }
                        else if (Directory.Exists(renamedEventArgs.FullPath))
                        {
                            var dirInfo = new DirectoryInfo(renamedEventArgs.FullPath);
                            var itemViewModel = new DirectoryInfoViewModel(this)
                            {
                                Model = dirInfo
                            };
                            App.Current.Dispatcher.Invoke(() => Items.Add(itemViewModel));
                            await itemViewModel.LoadChildrenAsync();
                        }
                    }
                    break;
            }

            await SortAsync(_sortOptions, _cancellationTokenSource.Token);
        }

        public async Task SortAsync(SortOptions options, CancellationToken cancellationToken)
        {
            _sortOptions = options;
            StatusMessage = "Sorting directory...";
            NotifyPropertyChanged(nameof(StatusMessage));

            var threadIds = new ConcurrentBag<int>();

            try
            {
                await Task.Run(async () =>
                {
                    var directories = Items.OfType<DirectoryInfoViewModel>().ToList();
                    var files = Items.OfType<FileInfoViewModel>().ToList();

                    var comparer = new FileSystemInfoComparer(options.SortBy, options.Direction);

                    directories.Sort(comparer);
                    files.Sort(comparer);

                    await App.Current.Dispatcher.InvokeAsync(() =>
                    {
                        Items.Clear();
                        foreach (var dir in directories)
                        {
                            Items.Add(dir);
                        }

                        foreach (var file in files)
                        {
                            Items.Add(file);
                        }
                    });

                    var tasks = directories.Select(dir =>
                    {
                        return Task.Factory.StartNew(async () =>
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }
                            await dir.SortAsync(options, cancellationToken);
                        }, TaskCreationOptions.LongRunning).Unwrap();
                    }).ToArray();

                    await Task.WhenAll(tasks);
                }, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    StatusMessage = "Sorting cancelled.";
                }
                else
                {
                    StatusMessage = $"Sorted by {options.SortBy} {options.Direction}";
                }
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Sorting cancelled.";
            }
            finally
            {
                NotifyPropertyChanged(nameof(StatusMessage));
            }
        }

        public void CancelSort()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource(); // Reset the token source for future use
        }

        public Exception Exception { get; private set; }
    }
}
