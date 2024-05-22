using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design.Serialization;
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
        public ObservableCollection<FileSystemInfoViewModel> Items { get; private set; } = new ObservableCollection<FileSystemInfoViewModel>();
        private FileSystemWatcher Watcher;
        private SortOptions _sortOptions;

        public DirectoryInfoViewModel(ViewModelBase owner) : base(owner)
        {
        }

        public bool Open(string path, SortOptions sortOptions = null)
        {
            bool result = false;

            try
            {
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
                    Sort(_sortOptions);
                }

                result = true;
            }
            catch (Exception ex)
            {
                Exception = ex;
            }

            return result;
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            Exception = e.GetException();
        }

        public void OnFileSystemChanged(object sender, FileSystemEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() => OnFileSystemChanged(e));
        }

        private void OnFileSystemChanged(FileSystemEventArgs e)
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

            Sort(_sortOptions);
        }

        public void Sort(SortOptions options)
        {
            _sortOptions = options;

            // Oddzielnie sortuj katalogi i pliki
            var directories = Items.OfType<DirectoryInfoViewModel>().ToList();
            var files = Items.OfType<FileInfoViewModel>().ToList();

            // Użyj klasy porównującej do sortowania
            var comparer = new FileSystemInfoComparer(options.SortBy, options.Direction);

            directories.Sort(comparer);
            files.Sort(comparer);

            // Wyczyszczenie i ponowne dodanie posortowanych elementów do kolekcji Items
            Items.Clear();

            foreach (var dir in directories)
            {
                Items.Add(dir);
                dir.Sort(options); // Rekurencyjne sortowanie podkatalogów
            }

            foreach (var file in files)
            {
                Items.Add(file);
            }
        }

        public Exception Exception { get; private set; }
    }

}
