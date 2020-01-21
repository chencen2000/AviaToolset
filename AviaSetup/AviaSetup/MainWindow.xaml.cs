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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AviaSetup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += delegate 
            {
                load_data();
            };
        }
        void load_data()
        {
            FocusManager.SetFocusedElement(tbSerialNo.Parent, tbSerialNo);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // OK clicked
            string sn = tbSerialNo.Text;
            try
            {
                Guid g = Guid.Parse(sn);
            }
            catch (Exception)
            {
                MessageBox.Show("Serial no incorrect.");
                return;
            }

            // serail no format is ok.

        }
    }
}
