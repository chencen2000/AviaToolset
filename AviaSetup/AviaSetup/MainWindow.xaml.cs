using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
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
        async void load_data()
        {
            //MessageBox.Show("wait for debugger", "debug");
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

            bool ok = await Task.Run(() =>
            {
                bool ret = false;
                // check D drive
                try
                {
                    DriveInfo d = new DriveInfo("D");
                    if (d.DriveType != DriveType.CDRom && d.AvailableFreeSpace > ((long)10 * 1024 * 1024 * 1024))
                        ret = true;
                }
                catch (Exception)
                {
                    //MessageBox.Show("Not Found D drive.");
                    //Application.Current.Shutdown(3);
                    ret = false;
                }
                return ret;
            });
            if (!ok)
            {
                MessageBox.Show("Not Found D drive.");
                Application.Current.Shutdown(3);
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
                Application.Current.Shutdown(2);
            }
            //try
            //{
            //    DriveInfo d = new DriveInfo("D");
            //}
            //catch (Exception)
            //{
            //    MessageBox.Show("Not Found D drive.");
            //    Application.Current.Shutdown(3);
            //}


            // show progress and wait for installation
            UserControl2 uc2 = new UserControl2();
            root.Children.Clear();
            root.Children.Add(uc2);
            uc2.setStatusText("Start preparing.");
            int ret = await Task.Run(() =>
            {
                return install(sn_result.Item2);
            });
            // done
            if (ret!=0)
            {
                // fail to install.
                MessageBox.Show("Fail to install");
            }
            else
            {
                //Application.Current.Shutdown(0);
            }
            Application.Current.Shutdown(ret);
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
        int install(Dictionary<string,object> args)
        {
            int ret = 1;
            //for(int i = 0; i < 10; i++)
            //{
            //    update_status($"step: {i + 1}");
            //    System.Threading.Thread.Sleep(1000);
            //}
            // start install. 
            String root = @"D:\BZVisualInspect";
            System.IO.Directory.CreateDirectory(root);
            // download 
            update_status($"Download CMC from server.");
            string cmc_zip = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "cmc.zip");
            try
            {
                System.IO.File.Delete(cmc_zip);
                WebClient wc = new WebClient();
                wc.Credentials = new NetworkCredential("fd_clean", "fd_clean340!");
                wc.DownloadFile("ftp://ftp8.futuredial.com/SmartGrading/cmc.zip", cmc_zip);
            }
            catch (Exception) { }
            if (System.IO.File.Exists(cmc_zip))
            {
                update_status($"unzip to {root}.");
                ZipFile.ExtractToDirectory(cmc_zip, root);
                // save config.ini
                update_status($"Generate config.ini");
                utility.IniFile config = new utility.IniFile(System.IO.Path.Combine(root, "config.ini"));
                foreach(KeyValuePair<string,object> kvp in args)
                {
                    config.WriteValue("config", kvp.Key, kvp.Value.ToString());
                }
                // create shortcuts: 
                update_status($"Create shortcut");
                {
                    System.IO.Directory.CreateDirectory(System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "FutureDial"));
                    WshShell shell = new WshShell();
                    string fn = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), "SMARTGradeDownlaoder.lnk");
                    IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(fn);
                    shortcut.Description = "SMART Grade Downloader";
                    shortcut.IconLocation = System.IO.Path.Combine(root, "icon1.ico");
                    shortcut.TargetPath = System.IO.Path.Combine(root, "FDAcorn.exe");
                    shortcut.Arguments = "-StartDownLoad";
                    shortcut.WorkingDirectory = root;
                    shortcut.Save();
                    //
                    fn = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "SMARTGrade.lnk");
                    shortcut = (IWshShortcut)shell.CreateShortcut(fn);
                    shortcut.Description = "SMART Grade";
                    shortcut.IconLocation = System.IO.Path.Combine(root, "icon1.ico");
                    shortcut.TargetPath = System.IO.Path.Combine(root, "AviaUI.exe");
                    shortcut.Arguments = "";
                    shortcut.WorkingDirectory = root;
                    shortcut.Save();
                }
                // 
                {
                    string fn = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "FutureDial");
                    System.IO.Directory.CreateDirectory(fn);
                }
                //
                {
                    System.Environment.SetEnvironmentVariable("APSTHOME", root);
                    System.Environment.SetEnvironmentVariable("APSTHOME", root, EnvironmentVariableTarget.Machine);
                }
                // disable UAC
                {
                    RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    if (rk != null)
                    {
                        rk.SetValue("EnableLUA", 0);
                        rk.Close();
                    }
                }
                ret = 0;
            }
            else
                ret = 4;

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
