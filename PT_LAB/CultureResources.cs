using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PT_LAB
{
    public class CultureResources
    {
        public Strings GetStringsInstance()
        {
            return new Strings();
        }
        private static ObjectDataProvider _provider;
        public static ObjectDataProvider ResourceProvider
        {
            get
            {
                if (_provider == null)
                    _provider =
                    (ObjectDataProvider)System.Windows.Application.Current.FindResource("Strings");
                return _provider;
            }
        }

        public static void ChangeCulture(CultureInfo culture)
        {
            ResourceProvider.Refresh();
        }
    }
}
