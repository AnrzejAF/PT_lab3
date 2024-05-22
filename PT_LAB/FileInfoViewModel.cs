using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Input;

namespace PT_LAB
{
    public class FileInfoViewModel : FileSystemInfoViewModel
    {
        public FileInfo FileInfo => (FileInfo)Model;

        //public string Extension => FileInfo.Extension;

        public long Size => FileInfo.Length;

        public string Name => FileInfo.Name;

        public DateTime Date => FileInfo.LastWriteTime;

        private ImageSource _image;

        public ImageSource Image
        {
            get => _image;
            set
            {
                _image = value;
                NotifyPropertyChanged(nameof(Image));
            }
        }

        public new FileInfo Model
        {
            get => (FileInfo)base.Model;
            set
            {
                base.Model = value;
                if (value != null)
                {
                    LoadImage(value);
                }
            }
        }

        private void LoadImage(FileInfo fileInfo)
        {
            var fileType = fileInfo.Extension.ToLower();
            switch (fileType)
            {
                case ".txt":
                    Image = new BitmapImage(new Uri("pack://application:,,,/Images/TxtImage.png"));
                    break;
                case ".png":
                    Image = new BitmapImage(new Uri("pack://application:,,,/Images/PictureImage.png"));
                    break;
                case ".jpg":
                    Image = new BitmapImage(new Uri("pack://application:,,,/Images/PictureImage.png"));
                    break;
                default:
                    Image = new BitmapImage(new Uri("pack://application:,,,/Images/FileImage.png"));
                    break;
            }
        }

        public ICommand OpenFileCommand { get; private set; }

        public FileInfoViewModel(ViewModelBase owner) : base(owner)
        {
            OpenFileCommand = new RelayCommand(OpenFileExecute, OpenFileCanExecute);
        }

        public string Extension => Path.GetExtension(Model.FullName);

        private bool OpenFileCanExecute(object parameter)
        {
            return OwnerExplorer?.OpenFileCommand.CanExecute(parameter) ?? false;
        }

        private void OpenFileExecute(object parameter)
        {
            OwnerExplorer?.OpenFileCommand.Execute(parameter);
        }

        public FileExplorer OwnerExplorer
        {
            get
            {
                var owner = Owner;
                while (owner is DirectoryInfoViewModel ownerDirectory)
                {
                    if (ownerDirectory.Owner is FileExplorer explorer)
                        return explorer;
                    owner = ownerDirectory.Owner;
                }
                return null;
            }
        }
    }



}
