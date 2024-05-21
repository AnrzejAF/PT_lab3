﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace PT_LAB
{
    public class FileInfoViewModel : FileSystemInfoViewModel
    {
        public FileInfo FileInfo => (FileInfo)Model;

        public string Extension => FileInfo.Extension;

        public long Size => FileInfo.Length;

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

    }
}
