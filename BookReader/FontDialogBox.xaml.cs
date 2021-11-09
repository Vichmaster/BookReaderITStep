using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Project11
{
    /// <summary>
    /// Interaction logic for FontDialogBox.xaml
    /// </summary>
    public partial class FontDialogBox : Window
    {
        private FlowDocument _par;
        public FontDialogBox(FlowDocument Par)
        {
            InitializeComponent();
            _par = Par;
            for (double i = 10; i <= 48; i+=2)
            {
                fontSizeTextBox.Items.Add(i);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            _par.FontFamily = (FontFamily)fontFamilyTextBox.SelectedItem;
            _par.FontSize = (double)fontSizeTextBox.SelectedItem;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
