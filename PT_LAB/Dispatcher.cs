using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PT_LAB
{
    public class DispatchedObservableCollection<T> : ObservableCollection<T>
    {
        private readonly Dispatcher _dispatcher;

        public DispatchedObservableCollection()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_dispatcher.CheckAccess())
            {
                base.OnCollectionChanged(e);
            }
            else
            {
                _dispatcher.Invoke(() => base.OnCollectionChanged(e));
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (_dispatcher.CheckAccess())
            {
                base.OnPropertyChanged(e);
            }
            else
            {
                _dispatcher.Invoke(() => base.OnPropertyChanged(e));
            }
        }
    }

}
