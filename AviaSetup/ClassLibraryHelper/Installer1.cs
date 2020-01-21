using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
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
            MessageBox.Show("test");
            base.OnCommitting(savedState);
        }
    }
}
