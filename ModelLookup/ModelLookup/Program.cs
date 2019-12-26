using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
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
        static string NAME = "ModelLookupWebService";
        static List<Dictionary<string, object>> records = new List<Dictionary<string, object>>();

        static void logIt(String msg)
        {
            System.Diagnostics.Trace.WriteLine(msg);
        }
        static void Main(string[] args)
        {
            System.Configuration.Install.InstallContext _args = new System.Configuration.Install.InstallContext(null, args);
            if (_args.IsParameterTrue("start-service"))
            {
                bool own = false;
                System.Threading.EventWaitHandle evt = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset, NAME, out own);
                if (own)
                {
                    // download db
                    download_imei2model();
                    start(evt);
                }
                else
                {
                    // already running.
                }
            }
            else if (_args.IsParameterTrue("kill-service"))
            {
                try
                {
                    System.Threading.EventWaitHandle evt = System.Threading.EventWaitHandle.OpenExisting(NAME);
                    evt.Set();
                }
                catch (Exception) { }
            }
            else
            {

            }
        }
        static string getVersion()
        {
            return System.Diagnostics.Process.GetCurrentProcess().MainModule.FileVersionInfo.FileVersion;
        }
        static void start(System.Threading.EventWaitHandle quit)
        {
            var appSettings = ConfigurationManager.AppSettings;
            try
            {
                Uri baseAddress = new Uri(string.Format("http://localhost:{0}/", appSettings["port"]));
                WebServiceHost svcHost = new WebServiceHost(typeof(Program), baseAddress);
                WebHttpBinding b = new WebHttpBinding();
                b.Name = NAME;
                b.HostNameComparisonMode = HostNameComparisonMode.Exact;
                svcHost.AddServiceEndpoint(typeof(IModelLookupWebService), b, "");
                logIt("WebService is running");
                svcHost.Open();
                quit.WaitOne();
                //System.Console.WriteLine("Press any key to stop.");
                //System.Console.ReadKey();
                svcHost.Close();
            }
            catch(Exception ex)
            {
                logIt(ex.Message);
            }
        }
        static Tuple<int,string,string, string> lookupImei(string imei)
        {
            int ret = -1;
            string msg = "";
            string ret1 = "";
            string ret2 = "";
            string tac = "";
            List<Dictionary<string, object>> ready = new List<Dictionary<string, object>>();
            // check imei
            if(!string.IsNullOrEmpty(imei))
            {
                if(util.check_imei(imei)==0)
                {
                    tac = imei.Substring(0, 8);
                }
                else
                {
                    ret = 2;
                    msg = $"The IMEI ({imei}) format incorrect.";
                }
            }
            else
            {
                ret = 1;
                msg = "Missing input imei";
            }
            // lookup
            if (!string.IsNullOrEmpty(tac))
            {
                try
                {
                    lock (records)
                    {
                        foreach (Dictionary<string, object> r in records)
                        {
                            if (r.ContainsKey("uuid") && string.Compare(r["uuid"].ToString(), tac, true) == 0)
                            {
                                ready.Add(r);
                            }
                        }
                    }
                }
                catch (Exception) { }
                // return
                if (ready.Count > 0)
                {
                    foreach (Dictionary<string, object> r in ready)
                    {
                        if (r.ContainsKey("model") && r.ContainsKey("maker"))
                        {
                            ret1 = r["maker"].ToString();
                            ret2 = r["model"].ToString();
                            ret = 0;
                            msg = $"imie={imei} lookup complete.";
                            break;
                        }
                    }
                }
                else
                {
                    ret = 3;
                    msg = $"imie={imei} not found.";
                }
            }
            return new Tuple<int, string, string, string>(ret, ret1, ret2,msg);
        }
        static void download_imei2model()
        {
            Tuple<bool, Dictionary<string, object>[]> res = getAllCollectDocuments("imei2model");
            if (res.Item1)
            {
                lock (records)
                    records = new List<Dictionary<string, object>>(res.Item2);
            }
        }
        static Tuple<bool, Dictionary<string, object>[]> getAllCollectDocuments(string collectionName, int size = 1000)
        {
            bool ret = false;
            System.Collections.ArrayList retList = new System.Collections.ArrayList();
            try
            {
                int page = 1;
                while (!ret)
                {
                    NameValueCollection q = new NameValueCollection();
                    q.Add("page", page.ToString());
                    q.Add("pagesize", size.ToString());
                    WebClient wc = new WebClient();
                    wc.Credentials = new NetworkCredential("cmc", "cmc1234!");
                    wc.Headers.Add("Content-Type", "application/json");
                    wc.QueryString = q;
                    string s = wc.DownloadString($"http://dc.futuredial.com/cmc/{collectionName}");
                    if (!string.IsNullOrEmpty(s))
                    {
                        var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                        Dictionary<string, object> data = jss.Deserialize<Dictionary<string, object>>(s);
                        if (data.ContainsKey("_returned") && data["_returned"].GetType() == typeof(int))
                        {
                            int i = (int)data["_returned"];
                            if (i == 0) ret = true;
                            else
                            {
                                if (data.ContainsKey("_embedded") && data["_embedded"].GetType() == typeof(Dictionary<string, object>))
                                {
                                    Dictionary<string, object> d = (Dictionary<string, object>)data["_embedded"];
                                    if (d.ContainsKey("doc") && d["doc"].GetType() == typeof(System.Collections.ArrayList))
                                    {
                                        System.Collections.ArrayList al = (System.Collections.ArrayList)d["doc"];
                                        retList.AddRange(al);
                                    }
                                }
                                page++;
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
            return new Tuple<bool, Dictionary<string, object>[]>(ret, (Dictionary<string, object>[])retList.ToArray(typeof(Dictionary<string, object>)));
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
                Tuple<int, string, string,string> mm = lookupImei(imei);
                dic.Add("error", mm.Item1);
                dic.Add("message", mm.Item4);
                if (mm.Item1 == 0)
                {
                    dic.Add("maker", mm.Item2);
                    dic.Add("model", mm.Item3);
                }
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
