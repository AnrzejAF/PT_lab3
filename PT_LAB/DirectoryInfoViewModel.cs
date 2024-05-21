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
        public ObservableCollection<FileSystemInfoViewModel> Items { get; private set; }
         = new ObservableCollection<FileSystemInfoViewModel>();
        public DirectoryInfo DirectoryInfo => (DirectoryInfo)Model;

        public string Extension => string.Empty; // Katalogi nie mają rozszerzeń

        public long Size => Items.Count; // Możesz też implementować rekurencyjne zliczanie elementów

        private SortOptions _sortOptions;


        public DateTime Date => DirectoryInfo.LastWriteTime;

        public bool Open(string path)
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
                    DirectoryInfoViewModel itemViewModel = new DirectoryInfoViewModel();
                    itemViewModel.Model = dirInfo;
                    Items.Add(itemViewModel);

                    itemViewModel.Open(dirName);
                }

                foreach (var fileName in Directory.GetFiles(path))
                {
                    var fileInfo = new FileInfo(fileName);
                    FileInfoViewModel itemViewModel = new FileInfoViewModel();
                    itemViewModel.Model = fileInfo;
                    Items.Add(itemViewModel);
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
                    if (e is FileSystemEventArgs fileEvent && File.Exists(fileEvent.FullPath))
                    {
                        var fileInfo = new FileInfo(fileEvent.FullPath);
                        FileInfoViewModel itemViewModel = new FileInfoViewModel();
                        itemViewModel.Model = fileInfo;
                        Items.Add(itemViewModel);
                    }
                    else if (e is FileSystemEventArgs directoryEvent && Directory.Exists(directoryEvent.FullPath))
                    {
                        var dirInfo = new DirectoryInfo(directoryEvent.FullPath);
                        DirectoryInfoViewModel itemViewModel = new DirectoryInfoViewModel();
                        itemViewModel.Model = dirInfo;
                        Items.Add(itemViewModel);
                        itemViewModel.Open(directoryEvent.FullPath);
                    }
                    break;
                case WatcherChangeTypes.Deleted:
                    string deletedPath = e.FullPath;
                    FileSystemInfoViewModel deletedItem = Items.FirstOrDefault(item => item.Model.FullName == deletedPath);
                    if (deletedItem != null)
                    {
                        Items.Remove(deletedItem);
                    }
                    break;
                case WatcherChangeTypes.Changed:
                    string changedPath = e.FullPath;
                    FileSystemInfoViewModel changedItem = Items.FirstOrDefault(item => item.Model.FullName == changedPath);
                    if (changedItem != null)
                    {
                        if (changedItem is FileInfoViewModel fileViewModel)
                        {
                            var fileInfo = new FileInfo(changedPath);
                            fileViewModel.Model = fileInfo;
                        }
                        else if (changedItem is DirectoryInfoViewModel dirViewModel)
                        {
                            var dirInfo = new DirectoryInfo(changedPath);
                            dirViewModel.Model = dirInfo;
                        }
                    }
                    break;
                case WatcherChangeTypes.Renamed:
                    string oldPath = (e as RenamedEventArgs).OldFullPath;
                    string newPath = e.FullPath;


                    FileSystemInfoViewModel renamedItem = Items.FirstOrDefault(item => item.Model.FullName == oldPath);
                    if (renamedItem != null)
                    {
                        if (renamedItem is FileInfoViewModel fileViewModel)
                        {
                            var fileInfo = new FileInfo(newPath);
                            FileInfoViewModel itemViewModel = new FileInfoViewModel();
                            itemViewModel.Model = fileInfo;
                            Items.Add(itemViewModel);
                        }
                        else if (renamedItem is DirectoryInfoViewModel dirViewModel)
                        {
                            var dirInfo = new DirectoryInfo(newPath);
                            DirectoryInfoViewModel itemViewModel = new DirectoryInfoViewModel();
                            itemViewModel.Model = dirInfo;
                            Items.Add(itemViewModel);
                            itemViewModel.Open(newPath);
                        }
                        Items.Remove(renamedItem);
                    }
                    break;
            }
            // sort Root
            Sort(_sortOptions);
        }

        public List<DirectoryInfoViewModel>? SubDirectories { get; set; }

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

        public string Name { get; private set; } = string.Empty;

        private FileSystemWatcher Watcher;
    }

}
