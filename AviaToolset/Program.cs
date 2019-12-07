using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

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
                ret = CmcClient.sendTransaction_BZ(_args.Parameters);
                //ret = CmcClient.sendTransaction(_args.Parameters);
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
            else if (_args.IsParameterTrue("cleanEnv"))
            {
                //ret = PrepareEnv.startup(_args.Parameters);
            }
            else if (_args.IsParameterTrue("oecontrol"))
            {
                //OEControl.OE_App_3_0_2_0();
                //OEControl.startup(_args.Parameters);
                OEControl.start(_args.Parameters);
            }
            else
            {
                test();
                //OEControl.start();
            }

            return ret;
        }

        static void test()
        {
            try
            {
                string tool = @"C:\ProgramData\Futuredial\Avia\Evaoi_3.1.1.2\evaoi.xml";
                if (System.IO.File.Exists(tool))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(tool);
                    if (doc.DocumentElement != null)
                    {
                        string model_dir = doc?.DocumentElement?["system"]?["ModelDir"]?.InnerText;
                        if (System.IO.Directory.Exists(model_dir))
                        {

                        }
                        XmlNode n = doc.DocumentElement.SelectSingleNode("work_station/item[name='BACK']/system");
                        if (n != null)
                        {
                            string s = n["PixelSize"]?.InnerText;
                            float f;
                            if (float.TryParse(s, out f))
                            {

                            }
                        }
                    }
                }
            }
            catch (Exception) { }


        }
    }
}
