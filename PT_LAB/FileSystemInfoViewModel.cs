using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT_LAB
{
    public class FileSystemInfoViewModel : ViewModelBase
    {
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

        public string Name => Model.Name;

        protected FileSystemInfoViewModel(ViewModelBase owner)
        {
            Owner = owner;
        }

        public ViewModelBase Owner { get; private set; }
    }
}
