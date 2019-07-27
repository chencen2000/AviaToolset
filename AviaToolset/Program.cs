using System;
using System.Collections.Generic;
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
            }
            return ret;
        }
    }
}
