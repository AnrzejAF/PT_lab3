using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT_LAB
{
    public class FileExplorer : ViewModelBase
    {
        public FileExplorer()
        {
            NotifyPropertyChanged(nameof(Lang));
            OpenRootFolderCommand = new RelayCommand(OpenRootFolderExecute);
            SortRootFolderCommand = new RelayCommand(SortRootFolderExecute, CanSortRootFolderExecute);
        }

        public DirectoryInfoViewModel? Root { get; set; }
        public RelayCommand OpenRootFolderCommand { get; private set; }
        public RelayCommand SortRootFolderCommand { get; private set; }

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
            SortDialog dlg = new SortDialog();
            if (dlg.ShowDialog() == true)
            {
                Root.Sort(dlg.SortOptions);
                NotifyPropertyChanged(nameof(Root));
            }
        }
    }
}
