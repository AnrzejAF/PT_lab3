using System.DirectoryServices;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace PT_LAB
{
    /// <summary>
    /// Interaction logic for CreateDialog.xaml
    /// </summary>
    public partial class SortDialog : Window
    {
        public SortOptions SortOptions { get; set; }

        public SortDialog(SortOptions sortOptions)
        {
            InitializeComponent();
            SortOptions = sortOptions ?? new SortOptions();
            this.DataContext = SortOptions;
        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }

}