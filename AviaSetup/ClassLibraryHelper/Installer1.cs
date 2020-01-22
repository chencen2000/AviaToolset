using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClassLibraryHelper
{
    [RunInstaller(true)]
    public partial class Installer1 : System.Configuration.Install.Installer
    {
        public Installer1()
        {
            InitializeComponent();
        }
        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
        }
        protected override void OnCommitted(IDictionary savedState)
        {
            base.OnCommitted(savedState);
        }
        protected override void OnCommitting(IDictionary savedState)
        {
            //MessageBox.Show("Wait for attach", "debug");
            Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string root = System.IO.Path.GetDirectoryName(a.Location);
            string tool = System.IO.Path.Combine(root, "AviaSetup.exe");
            if (System.IO.File.Exists(tool))
            {
                Process p = Process.Start(tool);
                p.WaitForExit();
                int i = p.ExitCode;
                if (i == 0)
                {
                    // success
                    MessageBox.Show($"Please reboot your computer.", "Complete");
                }
                else
                {
                    // fail to setup, rollback
                    string err = "Faile to setup";
                    if (i == 1) err = "Fail to install";
                    else if (i == 2) err = "Serial No incorrect.";
                    else if (i == 3) err = "Not Found D drive.";
                    else err = "Faile to setup";
                    throw new InstallException(err);
                }
            }
            else
            {
                MessageBox.Show($"Cannot found {tool}", "error");
            }
            base.OnCommitting(savedState);
        }
    }
}
