﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace PT_LAB
{
    public class DirectoryInfoViewModel : FileSystemInfoViewModel
    {
        private DispatchedObservableCollection<FileSystemInfoViewModel> _items;

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


        private FileSystemWatcher Watcher;
        private SortOptions _sortOptions;

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
        public async Task<bool> Open(string path, SortOptions sortOptions = null)
        {
            bool result = false;

            try
            {
                StatusMessage = "Loading directory:";
                NotifyPropertyChanged(nameof(StatusMessage));

                Watcher = new FileSystemWatcher(path);

                Watcher.Created += OnFileSystemChanged;
                Watcher.Renamed += OnFileSystemChanged;
                Watcher.Deleted += OnFileSystemChanged;
                Watcher.Changed += OnFileSystemChanged;
                Watcher.Error += Watcher_Error;
                Watcher.EnableRaisingEvents = true;

                foreach (var dirName in Directory.GetDirectories(path))
                {
                    var dirInfo = new DirectoryInfo(dirName);
                    var itemViewModel = new DirectoryInfoViewModel(this)
                    {
                        Model = dirInfo
                    };
                    Items.Add(itemViewModel);

                    itemViewModel.Open(dirName, sortOptions);
                }

                foreach (var fileName in Directory.GetFiles(path))
                {
                    var fileInfo = new FileInfo(fileName);
                    var itemViewModel = new FileInfoViewModel(this)
                    {
                        Model = fileInfo
                    };
                    Items.Add(itemViewModel);
                }

                if (sortOptions != null)
                {
                    _sortOptions = sortOptions;
                    Task task = SortAsync(_sortOptions);
                }
                StatusMessage = "Directory loaded";
                NotifyPropertyChanged(nameof(StatusMessage));
                await Task.Delay(500);
                result = true;
            }
            catch (Exception ex)
            {
                Exception = ex;
                StatusMessage = "Error loading directory";
                NotifyPropertyChanged(nameof(StatusMessage));
                await Task.Delay(500);

            }

            return result;
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            Exception = e.GetException();
        }

        public void OnFileSystemChanged(object sender, FileSystemEventArgs e)
        {
            StatusMessage = $"File system changed: {e.ChangeType} {e.FullPath}";
            NotifyPropertyChanged(nameof(StatusMessage));
            Task.Delay(500);
            App.Current.Dispatcher.Invoke(() => OnFileSystemChanged(e));
        }

        private async void OnFileSystemChanged(FileSystemEventArgs e)
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
                        Items.Add(itemViewModel);
                    }
                    else if (Directory.Exists(e.FullPath))
                    {
                        var dirInfo = new DirectoryInfo(e.FullPath);
                        var itemViewModel = new DirectoryInfoViewModel(this)
                        {
                            Model = dirInfo
                        };
                        Items.Add(itemViewModel);
                        itemViewModel.Open(e.FullPath, _sortOptions);
                    }
                    break;
                case WatcherChangeTypes.Deleted:
                    var deletedItem = Items.FirstOrDefault(item => item.Model.FullName == e.FullPath);
                    if (deletedItem != null)
                    {
                        Items.Remove(deletedItem);
                    }
                    break;
                case WatcherChangeTypes.Changed:
                    // Optional: Handle changes if needed
                    break;
                case WatcherChangeTypes.Renamed:
                    var renamedEventArgs = e as RenamedEventArgs;
                    if (renamedEventArgs != null)
                    {
                        var oldItem = Items.FirstOrDefault(item => item.Model.FullName == renamedEventArgs.OldFullPath);
                        if (oldItem != null)
                        {
                            Items.Remove(oldItem);
                        }

                        if (File.Exists(renamedEventArgs.FullPath))
                        {
                            var fileInfo = new FileInfo(renamedEventArgs.FullPath);
                            var itemViewModel = new FileInfoViewModel(this)
                            {
                                Model = fileInfo
                            };
                            Items.Add(itemViewModel);
                        }
                        else if (Directory.Exists(renamedEventArgs.FullPath))
                        {
                            var dirInfo = new DirectoryInfo(renamedEventArgs.FullPath);
                            var itemViewModel = new DirectoryInfoViewModel(this)
                            {
                                Model = dirInfo
                            };
                            Items.Add(itemViewModel);
                            itemViewModel.Open(renamedEventArgs.FullPath, _sortOptions);
                        }
                    }
                    break;
            }

            SortAsync(_sortOptions);
        }

        public async Task SortAsync(SortOptions options)
        {
            _sortOptions = options;
            StatusMessage = "Sorting directory...";
            await Task.Delay(1000);
            NotifyPropertyChanged(nameof(StatusMessage));

            await Task.Run(() =>
            {
                var directories = Items.OfType<DirectoryInfoViewModel>().ToList();
                var files = Items.OfType<FileInfoViewModel>().ToList();

                var comparer = new FileSystemInfoComparer(options.SortBy, options.Direction);

                directories.Sort(comparer);
                files.Sort(comparer);

                App.Current.Dispatcher.Invoke(() =>
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

                Debug.WriteLine($"Sorting directory on thread {Thread.CurrentThread.ManagedThreadId}");

                var tasks = directories.Select(dir =>
                {
                    return Task.Run(async () =>
                    {
                        Debug.WriteLine($"Starting sort of {dir.Model.FullName} on thread {Thread.CurrentThread.ManagedThreadId}");
                        await dir.SortAsync(options);
                        Debug.WriteLine($"Finished sort of {dir.Model.FullName} on thread {Thread.CurrentThread.ManagedThreadId}");
                    });
                }).ToArray();

                Task.WaitAll(tasks);
            });

            StatusMessage = $"Sorted by {options.SortBy} {options.Direction}";
            await Task.Delay(1000);
            NotifyPropertyChanged(nameof(StatusMessage));
        }
        public Exception Exception { get; private set; }
    }

}
