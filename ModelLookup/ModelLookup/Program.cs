using System;
using System.Collections;
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
        static DateTime lastmodeified = DateTime.MinValue;

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
                    //Task.Run(() => download_imei2model());
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
        static void saveDB()
        {
            string dir = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

        }
        static void loadDB()
        {
            string fn = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "imei2model.json");
            if (System.IO.File.Exists(fn))
            {
                try
                {
                    var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                    jss.MaxJsonLength = 20971520; // 20M
                    Dictionary<string, object> db = jss.Deserialize<Dictionary<string, object>>(System.IO.File.ReadAllText(fn));
                    if (db.ContainsKey("lastmodified") && db.ContainsKey("doc"))
                    {
                        DateTime t;
                        if (DateTime.TryParse(db["lastmodified"].ToString(), out t))
                        {
                            if (t > lastmodeified)
                            {
                                lastmodeified = t;
                                lock(records)
                                {
                                    ArrayList al = (ArrayList)db["doc"];
                                    records = new List<Dictionary<string, object>>((Dictionary<string, object>[])al.ToArray(typeof(Dictionary<string, object>)));
                                }
                            }
                        }
                    }
                }
                catch (Exception) { }
            }

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
                download_imei2model();
                loadDB();
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
                if (lastmodeified > DateTime.MinValue)
                {
                    bool inBlacklist = false;
                    try
                    {
                        lock (records)
                        {
                            foreach (Dictionary<string, object> r in records)
                            {
                                if (r.ContainsKey("uuid"))
                                {
                                    if (string.Compare(r["uuid"].ToString(), "blacklist") == 0)
                                    {
                                        // in black list?
                                        if (r.ContainsKey("blacklist"))
                                        {
                                            ArrayList al = (ArrayList)r["blacklist"];
                                            if(al.Contains(tac))
                                            {
                                                inBlacklist = true;
                                                break;
                                            }
                                        }
                                    }
                                    else if(string.Compare(r["uuid"].ToString(), tac) == 0)
                                    {
                                        ready.Add(r);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception) { }
                    // return
                    if (inBlacklist)
                    {
                        ret = 5;
                        msg = $"imie={imei} TAC={tac} in the blacklist.";
                    }
                    else
                    {
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
                                    // model name mapping
                                    Tuple<string, string> mm = mappingModelByMaker(ret1, ret2);
                                    ret1 = mm.Item1;
                                    ret2 = mm.Item2;
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
                }
                else
                {
                    ret = 4;
                    msg = $"local DB not ready. please lookup late.";
                }
            }
            return new Tuple<int, string, string, string>(ret, ret1, ret2,msg);
        }
        static Tuple<string,string> mappingModelByMaker(string maker, string model)
        {
            string ret1 = maker;
            string ret2 = model;
            string fn = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "mappingModel.ini");
            if (System.IO.File.Exists(fn))
            {
                utility.IniFile ini = new utility.IniFile(fn);
                ret2 = ini.GetString(maker, model, model);
            }
            return new Tuple<string, string>(ret1, ret2);
        }
        static void download_imei2model()
        {
            Tuple<bool, Dictionary<string, object>[], DateTime> res = getAllCollectDocuments("imei2model");
            if (res.Item1)
            {
                lock (records)
                    records = new List<Dictionary<string, object>>(res.Item2);
                // save a local copy
                try
                {
                    {
                        Dictionary<string, object> lc = new Dictionary<string, object>();
                        lc.Add("lastmodified", res.Item3.ToString("s"));
                        lc.Add("doc", res.Item2);
                        var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                        jss.MaxJsonLength = 20971520; // 20M
                        string db = jss.Serialize(lc);
                        string dir = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                        System.IO.File.WriteAllText(System.IO.Path.Combine(dir,"imei2model.json"), db);
                    }
                }
                catch (Exception) { }
            }
        }
        static Tuple<bool, Dictionary<string, object>[], DateTime> getAllCollectDocuments(string collectionName, int size = 1000)
        {
            DateTime retT = DateTime.MinValue;
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
                        if (data.ContainsKey("_lastupdated_on") && data["_lastupdated_on"].GetType() == typeof(string))
                        {
                            DateTime t;
                            if (DateTime.TryParse(data["_lastupdated_on"].ToString(), out t))
                            {
                                if (t > retT) retT = t;
                            }
                                
                        }
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
            return new Tuple<bool, Dictionary<string, object>[], DateTime>(ret, (Dictionary<string, object>[])retList.ToArray(typeof(Dictionary<string, object>)), retT);
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
                    dic.Add("lastmodify", lastmodeified.ToString("s"));
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
