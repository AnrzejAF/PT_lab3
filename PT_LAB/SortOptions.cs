using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PT_LAB
{
    public class SortOptions : INotifyPropertyChanged
    {
        public enum SortByOption { Name, Extension, Size, Date }
        public enum SortDirectionOption { Ascending, Descending }

        // add default values
        public SortOptions()
        {
            SortBy = SortByOption.Name;
            Direction = SortDirectionOption.Ascending;
        }

        private SortByOption _sortBy;
        public SortByOption SortBy
        {
            get { return _sortBy; }
            set { _sortBy = value; OnPropertyChanged(); }
        }

        private SortDirectionOption _direction;
        public SortDirectionOption Direction
        {
            get { return _direction; }
            set { _direction = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
