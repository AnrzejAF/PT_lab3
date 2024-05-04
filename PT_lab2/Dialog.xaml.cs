using System.IO;
using System.Windows;

namespace Lab2
{
    /// <summary>
    /// Interaction logic for CreateDialog.xaml
    /// </summary>
    public partial class Dialog : Window
    {
        public string CreatedName { get; private set; }
        public FileAttributes CreatedAttributes
        {
            get
            {
                FileAttributes attributes = default;
                if (ReadOnlyCheckBox.IsChecked == true) attributes |= FileAttributes.ReadOnly;
                if (HiddenCheckBox.IsChecked == true) attributes |= FileAttributes.Hidden;
                if (SystemCheckBox.IsChecked == true) attributes |= FileAttributes.System;
                return attributes;
            }
        }
        public bool IsFile => FileRadioButton.IsChecked == true;

        public Dialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.CreatedName = NameTextBox.Text;
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}