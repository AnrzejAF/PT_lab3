using System;
using System.DirectoryServices;
using System.Globalization;
using System.Windows.Data;
using static PT_LAB.SortOptions;
using Binding = System.Windows.Data.Binding;

namespace PT_LAB
{
    public class SortByToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string parameterString = parameter.ToString();
            if (parameterString.Length != 1)
                return false;

            string enumValueString = value.ToString();
            return enumValueString.StartsWith(parameterString, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null || !(bool)value)
                return Binding.DoNothing;

            string parameterString = parameter.ToString();
            if (parameterString.Length != 1)
                return Binding.DoNothing;

            foreach (var enumValue in Enum.GetValues(typeof(SortByOption)))
            {
                if (enumValue.ToString().StartsWith(parameterString, StringComparison.OrdinalIgnoreCase))
                    return enumValue;
            }

            return Binding.DoNothing;
        }
    }

    public class SortDirectionToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string parameterString = parameter.ToString();
            if (parameterString.Length != 1)
                return false;

            string enumValueString = value.ToString();
            return enumValueString.StartsWith(parameterString, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null || !(bool)value)
                return Binding.DoNothing;

            string parameterString = parameter.ToString();
            if (parameterString.Length != 1)
                return Binding.DoNothing;

            foreach (var enumValue in Enum.GetValues(typeof(SortDirection)))
            {
                if (enumValue.ToString().StartsWith(parameterString, StringComparison.OrdinalIgnoreCase))
                    return enumValue;
            }

            return Binding.DoNothing;
        }
    }
}
