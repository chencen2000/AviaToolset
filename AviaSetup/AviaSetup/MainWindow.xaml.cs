using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
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
        System.Windows.Controls.Grid root=null;
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
            //FocusManager.SetFocusedElement(tbSerialNo.Parent, tbSerialNo);
            UserControl1 uc1 = new UserControl1();
            //uc1.onOkClicked = o => 
            //{
            //    start_install(o);
            //};
            uc1.onOkClicked = new Action<string>(start_install);
            if (this.Content.GetType() == typeof(System.Windows.Controls.Grid))
            {
                System.Windows.Controls.Grid g = (System.Windows.Controls.Grid)this.Content;
                root = g;
                g.Children.Clear();
                g.Children.Add(uc1);
            }
        }

        async void start_install(string sn)
        {
            if (string.IsNullOrEmpty(sn))
                sn = "521eb3dd-47f0-40ef-9b54-30466dfe6cc7";
            Tuple<bool, Dictionary<string, object>> sn_result = get_psresult(sn);
            if (!sn_result.Item1)
            {
                MessageBox.Show("Serial No incorrect.");
            }

            // show progress and wait for installation
            UserControl2 uc2 = new UserControl2();
            root.Children.Clear();
            root.Children.Add(uc2);
            uc2.setStatusText("Start preparing.");
            bool ret = await Task.Run(() =>
            {
                return install();
            });
            // done
            if (!ret)
            {
                // fail to install.
                MessageBox.Show("Fail to install");
                Application.Current.Shutdown(1);
            }
            else
            {
                Application.Current.Shutdown(0);
            }
        }
        void update_status(string msg)
        {
            this.Dispatcher.Invoke(delegate 
            {
                if (root.Children[0].GetType() == typeof(UserControl2))
                {
                    UserControl2 uc2 = (UserControl2)root.Children[0];
                    uc2.setStatusText(msg);
                }
            });
        }
        bool install()
        {
            bool ret = false;
            for(int i = 0; i < 10; i++)
            {
                update_status($"step: {i + 1}");
                System.Threading.Thread.Sleep(1000);
            }            
            return ret;
        }
        Tuple<bool, Dictionary<string,object>> get_psresult(string sn)
        {
            bool ret = false;
            Dictionary<string, object> ret2 = null;
            try
            {
                Dictionary<string, object> q = new Dictionary<string, object>();
                q.Add("_id", sn);
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                WebClient wc = new WebClient();
                wc.Headers.Add("Content-Type", "application/json");
                NameValueCollection qs = new NameValueCollection();
                qs.Add("criteria", jss.Serialize(q));
                wc.QueryString = qs;
                string r = wc.DownloadString("https://ps.futuredial.com/profiles/clients/_find");
                if (!string.IsNullOrEmpty(r))
                {
                    q = jss.Deserialize<Dictionary<string, object>>(r);
                    if(q.ContainsKey("ok") && q["ok"]?.GetType()==typeof(int) && (int)q["ok"] == 1)
                    {
                        ArrayList l = (ArrayList)q["results"];
                        if (l.Count == 1)
                        {
                            ret = true;
                            ret2 = (Dictionary<string, object>)l[0];
                        }
                    }
                }
            }
            catch (Exception) { }
            return new Tuple<bool, Dictionary<string, object>>(ret, ret2);
        }
    }
}
