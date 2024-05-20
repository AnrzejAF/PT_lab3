using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PT_LAB
{
    public enum SortBy
    {
        Alphabetical,
        ByExtension,
        BySize,
        ByDate
    }

    public enum SortDirection
    {
        Ascending,
        Descending
    }

    public class SortOptions : ViewModelBase
    {
        private SortBy _sortBy;
        private SortDirection _direction;

        public SortBy SortBy
        {
            get => _sortBy;
            set
            {
                if (_sortBy != value)
                {
                    _sortBy = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public SortDirection Direction
        {
            get => _direction;
            set
            {
                if (_direction != value)
                {
                    _direction = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
