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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public Action<string> onOkClicked = null;
        public UserControl1()
        {
            InitializeComponent();
            this.Loaded += delegate
            {
                load_data();
            };
        }
        void load_data()
        {
            FocusManager.SetFocusedElement(this, tbSerialNo);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (onOkClicked != null)
                onOkClicked(tbSerialNo.Text);
        }
    }
}
