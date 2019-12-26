using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ModelLookup
{
    [ServiceContract]
    public interface IModelLookupWebService
    {
        [OperationContract]
        [WebGet(UriTemplate = "test?name={name}")]
        Stream Test(string name);

        [OperationContract]
        [WebGet(UriTemplate = "lookup?imei={imei}")]
        Stream Lookup(string imei);
    }

    class Program :IModelLookupWebService
    {
        static void logIt(String msg)
        {
            System.Diagnostics.Trace.WriteLine(msg);
        }
        static void Main(string[] args)
        {
            start();
        }
        static string getVersion()
        {
            return System.Diagnostics.Process.GetCurrentProcess().MainModule.FileVersionInfo.FileVersion;
        }
        static void start()
        {
            var appSettings = ConfigurationManager.AppSettings;
            try
            {
                Uri baseAddress = new Uri(string.Format("http://localhost:{0}/", appSettings["port"]));
                WebServiceHost svcHost = new WebServiceHost(typeof(Program), baseAddress);
                WebHttpBinding b = new WebHttpBinding();
                b.Name = "ModelLookupWebService";
                b.HostNameComparisonMode = HostNameComparisonMode.Exact;
                svcHost.AddServiceEndpoint(typeof(IModelLookupWebService), b, "");
                logIt("WebService is running");
                svcHost.Open();
                System.Console.WriteLine("Press any key to stop.");
                System.Console.ReadKey();
                svcHost.Close();
            }
            catch(Exception ex)
            {
                logIt(ex.Message);
            }
        }
        #region IModelLookupWebService 
        public Stream Test(string name)
        {
            Stream ret = null;
            try
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("function", "Test");
                dic.Add("name", name);
                dic.Add("Version", getVersion());
                System.ServiceModel.Web.WebOperationContext op = System.ServiceModel.Web.WebOperationContext.Current;
                op.OutgoingResponse.Headers.Add("Content-Type", "application/json");
                JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                string s = jss.Serialize(dic);
                ret = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(s));
            }
            catch (Exception) { }
            return ret;
        }

        public Stream Lookup(string imei)
        {
            Stream ret = null;
            try
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("function", "Lookup");
                dic.Add("imei", imei);
                dic.Add("tac", imei.Substring(0,8));
                dic.Add("Version", getVersion());
                System.ServiceModel.Web.WebOperationContext op = System.ServiceModel.Web.WebOperationContext.Current;
                op.OutgoingResponse.Headers.Add("Content-Type", "application/json");
                JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                string s = jss.Serialize(dic);
                ret = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(s));
            }
            catch (Exception) { }
            return ret;
        }
        #endregion
    }
}
