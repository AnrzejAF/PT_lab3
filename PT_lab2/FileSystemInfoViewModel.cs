using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab2
{
    public class FileSystemInfoViewModel : ViewModelBase
    {
        public DateTime LastWriteTime
        {
            get { return _lastWriteTime; }
            set
            {
                if (_lastWriteTime != value)
                {
                    _lastWriteTime = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private DateTime _lastWriteTime;

        public String Caption { get; set; }

        public FileSystemInfo Model
        {
            get { return _fileSystemInfo; }
            set
            {
                if (_fileSystemInfo != value)
                {
                    _fileSystemInfo = value;
                    this.LastWriteTime = value.LastWriteTime;
                    this.Caption = value.Name;

                    NotifyPropertyChanged();
                }
            }
        }
        private FileSystemInfo ? _fileSystemInfo;
    }
}
