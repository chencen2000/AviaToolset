using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviaToolset
{
    class Program
    {
        public static void logIt(string msg)
        {
            System.Diagnostics.Trace.WriteLine($"[AviaToolset]: {msg}");
        }
        [STAThread]
        static int Main(string[] args)
        {
            int ret = -1;
            System.Configuration.Install.InstallContext _args = new System.Configuration.Install.InstallContext(null, args);
            if(_args.IsParameterTrue("debug"))
            {
                Console.WriteLine("Wait for debugger, press any key to continue.");
                Console.ReadKey();
            }
            if (_args.IsParameterTrue("transaction"))
            {
                ret = CmcClient.sendTransaction(_args.Parameters);
                //ret = CmcClient.sendTransactionToVerizon(_args.Parameters);
            }
            else if (_args.IsParameterTrue("login"))
            {
                ret = CmcClient.cmc_login(_args.Parameters);
            }
            else if (_args.IsParameterTrue("prepareEnv"))
            {
                ret = PrepareEnv.startup(_args.Parameters);
            }
            else if (_args.IsParameterTrue("oecontrol"))
            {
                //OEControl.OE_App_3_0_2_0();
                //OEControl.startup(_args.Parameters);
                OEControl.start(_args.Parameters);
            }
            else
            {
                //test();
                //OEControl.start();
            }

            return ret;
        }

        static void test()
        {
            Process p = null;
            try
            {
                utility.IniFile config = new utility.IniFile(System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "Avia", "AviaDevice.ini"));
                string app = config.GetString("config", "ui", "");
                if (System.IO.File.Exists(app))
                {
                    p = new Process();
                    p.StartInfo.FileName = app;
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(app);
                    p.Start();
                }
            }
            catch (Exception)
            {
                p = null;
            }
            if (p != null)
            {
                p.WaitForInputIdle();
            }
        }
    }
}
