using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ModelLookup
{
    class Test
    {
        static void logIt(string msg)
        {
            System.Diagnostics.Trace.WriteLine(msg);
        }
        static void Main(string[] args)
        {
            //test();
            samsung_test1();
        }
        static void test()
        {
            logIt($"cwd: {System.Environment.CurrentDirectory}");
            //apple_tac_upload();
            //dump_collection("CCTest");
            //upload_data();
            util.check_imei("490154203237518");
            //try
            //{
            //    string str = System.IO.File.ReadAllText("ready.json");
            //    var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            //    Dictionary<string, object>[] records = jss.Deserialize<Dictionary<string, object>[]>(str);
            //    upload_data(records);
            //}
            //catch (Exception)
            //{

            //}
        }
        static void samsung_test1()
        {

        }
        static void upload_data(Dictionary<string,object>[] records)
        {
            try
            {
                foreach (Dictionary<string, object> d in records)
                {
                    var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                    string s = jss.Serialize(d);
                    try
                    {
                        WebClient wc = new WebClient();
                        wc.Credentials = new NetworkCredential("cmc", "cmc1234!");
                        wc.Headers.Add("Content-Type", "application/json");
                        s = wc.UploadString("http://dc.futuredial.com/cmc/imei2model", s);
                    }
                    catch (Exception ex)
                    {
                        logIt(ex.Message);
                    }
                    //break;
                }
            }
            catch (Exception)
            {

            }
        }
        static void dump_collection(string collectionName)
        {
            Tuple<bool, int, DateTime> col_cnt = getCollectionCount(collectionName);
            Tuple<bool, Dictionary<string, object>[]> records = getAllCollectDocuments(collectionName, 1000);
            Dictionary<string, object> local_data = new Dictionary<string, object>();
            local_data.Add("lastmodify", col_cnt.Item3.ToString("s"));
            local_data.Add("doc", records.Item2);
            var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            System.IO.File.WriteAllText("test.json", jss.Serialize(local_data));
        }
        static Tuple<bool, Dictionary<string,object>[]> getAllCollectDocuments(string collectionName, int size = 1000)
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
                        if(data.ContainsKey("_returned") && data["_returned"].GetType()==typeof(int))
                        {
                            int i = (int)data["_returned"];
                            if (i == 0) ret = true;
                            else
                            {
                                if (data.ContainsKey("_embedded") && data["_embedded"].GetType() == typeof(Dictionary<string, object>))
                                {
                                    Dictionary<string, object> d = (Dictionary<string, object>)data["_embedded"];
                                    if(d.ContainsKey("doc") && d["doc"].GetType() == typeof(System.Collections.ArrayList))
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
        static Tuple<bool,int,DateTime> getCollectionCount(string collectionName)
        {
            bool ret = false;
            int count = 0;
            DateTime t = DateTime.Now;
            try
            {
                NameValueCollection q = new NameValueCollection();
                q.Add("pagesize", "0");
                q.Add("count", "");
                WebClient wc = new WebClient();
                wc.Credentials = new NetworkCredential("cmc", "cmc1234!");
                wc.Headers.Add("Content-Type", "application/json");
                wc.QueryString = q;
                string s = wc.DownloadString($"http://dc.futuredial.com/cmc/{collectionName}");
                if (!string.IsNullOrEmpty(s))
                {
                    var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                    Dictionary<string, object> data = jss.Deserialize<Dictionary<string, object>>(s);
                    if(data.ContainsKey("_size") && data["_size"].GetType()==typeof(int) &&
                        data.ContainsKey("_lastupdated_on"))
                    {
                        count = (int)data["_size"];
                        s = data["_lastupdated_on"].ToString();
                        if (DateTime.TryParse(s, out t))
                            ret = true;
                    }
                }
            }
            catch (Exception) { }
            return new Tuple<bool, int, DateTime>(ret, count, t);
        }
        static void apple_tac_upload()
        {
            var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> data = jss.Deserialize<Dictionary<string, object>>(System.IO.File.ReadAllText(@"data\iphone_tac.json"));
            List<Dictionary<string, object>> ready = new List<Dictionary<string, object>>();
            utility.IniFile ini = new utility.IniFile(@"data\iphone_model.ini");
            foreach (KeyValuePair<string, object> kvp in data)
            {
                Dictionary<string, object> d = (Dictionary<string, object>)kvp.Value;
                Dictionary<string, object> db = new Dictionary<string, object>();
                db.Add("uuid", kvp.Key);
                db.Add("cmc_maker", "Apple");
                db.Add("cmc_model", d["model"]);
                db.Add("maker", "Apple");
                db.Add("model", ini.GetString("Apple", d["model"].ToString(), d["model"].ToString()));
                ready.Add(db);
            }
            string s = jss.Serialize(ready.ToArray());
            System.IO.File.WriteAllText("test.json", s);

            foreach(Dictionary<string,object> d in ready)
            {
                s = jss.Serialize(d);
                try
                {
                    WebClient wc = new WebClient();
                    wc.Credentials = new NetworkCredential("cmc", "cmc1234!");
                    wc.Headers.Add("Content-Type", "application/json");
                    s = wc.UploadString("http://dc.futuredial.com/cmc/CCTest", s);
                }
                catch (Exception ex) 
                {
                    logIt(ex.Message);
                }
                //break;
            }
        }
    }
}
