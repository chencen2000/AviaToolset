using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
            test();
            //samsung_test1();
            //samsung_upload();
        }
        static void test()
        {
            logIt($"cwd: {System.Environment.CurrentDirectory}");
            //apple_tac_upload();
            //dump_collection("CCTest");
            //upload_data();
            //util.check_imei("490154203237518");
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
#if false
            // download imei2model db
            Tuple<bool, Dictionary<string, object>[], DateTime> res = getAllCollectDocuments("imei2model");
            if (res.Item1)
            {
                Dictionary<string, object> d = new Dictionary<string, object>();
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                d.Add("lastmodified", res.Item3.ToString("s"));
                d.Add("doc", res.Item2);
                System.IO.File.WriteAllText("imei2model.json", jss.Serialize(d));
            }
#endif
#if true
            var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            jss.MaxJsonLength = 20971520;
            Dictionary<string, object> db = jss.Deserialize<Dictionary<string, object>>(System.IO.File.ReadAllText("imei2model.json"));
            Dictionary<string, int> counts = new Dictionary<string, int>();
            if (db.ContainsKey("doc"))
            {
                ArrayList al = (ArrayList)db["doc"];
                foreach(Dictionary<string,object> r in al)
                {
                    if (r.ContainsKey("model") && r.ContainsKey("maker"))
                    {
                        string k = $"{r["maker"]}-{r["model"]}";
                        if (counts.ContainsKey(k))
                        {
                            int i = counts[k];
                            counts[k] = i + 1;
                        }
                        else
                        {
                            counts.Add(k, 1);
                        }
                    }
                }
            }
            foreach(KeyValuePair<string,int> kvp in counts)
            {
                logIt($"{kvp.Key}: {kvp.Value}");
            }
#endif
        }
        static void upload_backlist()
        {
            string[] backlist = { "99000337",
                                    "99000115",
                                    "86127303",
                                    "35990204",
                                    "35939406",
                                    "35894602",
                                    "35585807",
                                    "00499901"};
            try
            {

            }
            catch (Exception) { }


        }
        static void samsung_upload()
        {
            var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            ArrayList records = jss.Deserialize<ArrayList>(System.IO.File.ReadAllText("Samsung_GalaxyS6Edge.json"));
            foreach(Dictionary<string,object> r in records)
            {
                if (r.ContainsKey("uuid"))
                {
                    string str = r["uuid"].ToString();
                    int count = 0;
                    try
                    {
                        NameValueCollection q = new NameValueCollection();
                        q.Add("filter", $"{{\"uuid\": \"{str}\"}}");
                        WebClient wc = new WebClient();
                        wc.Credentials = new NetworkCredential("cmc", "cmc1234!");
                        wc.Headers.Add("Content-Type", "application/json");
                        wc.QueryString = q;
                        string s = wc.DownloadString($"http://dc.futuredial.com/cmc/imei2model");
                        if (!string.IsNullOrEmpty(s))
                        {
                            Dictionary<string, object> res = jss.Deserialize<Dictionary<string, object>>(s);
                            if (res.ContainsKey("_returned") && res["_returned"].GetType() == typeof(int))
                                count = (int)res["_returned"];
                            if (count > 0)
                            {
                                logIt("=========================");
                                logIt($"{str} duplicated!!");
                                logIt(s);
                                logIt("=========================");
                            }
                        }
                    }
                    catch (Exception) { }

                    if (count == 0)
                    {
                        str = jss.Serialize(r);
                        try
                        {
                            WebClient wc = new WebClient();
                            wc.Credentials = new NetworkCredential("cmc", "cmc1234!");
                            wc.Headers.Add("Content-Type", "application/json");
                            str = wc.UploadString("http://dc.futuredial.com/cmc/imei2model", str);
                        }
                        catch (Exception ex)
                        {
                            logIt(ex.Message);
                        }
                    }
                }
            }
        }
        static void samsung_test1()
        {
            var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            string str;
            Regex reg = new Regex(@"SM-G925");
            string target_ret_maker = "Samsung";
            string target_ret_model = "GalaxyS6Edge";

            List<Dictionary<string, object>> target_model = new List<Dictionary<string, object>>();
            // first step: all target model to a list.
            if (true)
            {
                str = System.IO.File.ReadAllText(@"data\samsung_fromdb.json");
                ArrayList al = jss.Deserialize<ArrayList>(str);
                foreach (Dictionary<string, object> r in al)
                {
                    if (r.ContainsKey("model"))
                    {
                        if (reg.IsMatch(r["model"].ToString()))
                        {
                            target_model.Add(r);
                        }
                    }
                }
                str = jss.Serialize(target_model);
                //System.IO.File.WriteAllText("test.json", str);
            }

            // second, group by tac number
            Dictionary<string, object> target_model_bytac = new Dictionary<string, object>();
            foreach(Dictionary<string,object> r in target_model)
            {
                if (r.ContainsKey("tac"))
                {
                    str = r["tac"].ToString();
                    if (target_model_bytac.ContainsKey(str))
                    {
                        ArrayList a = (ArrayList)target_model_bytac[str];
                        a.Add(r);
                    }
                    else
                    {
                        ArrayList a = new ArrayList();
                        a.Add(r);
                        target_model_bytac.Add(str, a);
                    }
                }
            }
            //System.IO.File.WriteAllText("samsung_G930_bytac.json", jss.Serialize(target_model_bytac));

            // third, count number by tac
            Dictionary<string, object> all_samsung = new Dictionary<string, object>();
            str = System.IO.File.ReadAllText(@"data\samsung_bytac.json");
            all_samsung = jss.Deserialize<Dictionary<string, object>>(str);
            Dictionary<string, int[]> target_summary = new Dictionary<string, int[]>();
            foreach(string tac in target_model_bytac.Keys)
            {
                int[] summary = new int[3];
                target_summary.Add(tac, summary);
                if (all_samsung.ContainsKey(tac))
                {
                    ArrayList al = (ArrayList)all_samsung[tac];
                    foreach(Dictionary<string,object> r in al)
                    {
                        int cnt = r.ContainsKey("cnt") ? (int)r["cnt"] : 0;
                        summary[0] += cnt;
                        if (r.ContainsKey("model"))
                        {
                            if (reg.IsMatch(r["model"].ToString()))
                            {
                                summary[1] += cnt;
                            }
                            else
                            {
                                summary[2] += cnt;
                            }
                        }
                    }
                }
            }
            //System.IO.File.WriteAllText("test.json", jss.Serialize(ret));
            double th = 90.0;
            target_model.Clear();
            foreach(KeyValuePair<string,int[]> kvp in target_summary)
            {
                double d = 100.0 * kvp.Value[1] / kvp.Value[0];
                if(d>=th)
                {
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    dic.Add("uuid", kvp.Key);
                    dic.Add("maker", target_ret_maker);
                    dic.Add("model", target_ret_model);
                    target_model.Add(dic);
                }
            }
            System.IO.File.WriteAllText($"{target_ret_maker}_{target_ret_model}.json", jss.Serialize(target_model));
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
        //static void dump_collection(string collectionName)
        //{
        //    Tuple<bool, int, DateTime> col_cnt = getCollectionCount(collectionName);
        //    Tuple<bool, Dictionary<string, object>[]> records = getAllCollectDocuments(collectionName, 1000);
        //    Dictionary<string, object> local_data = new Dictionary<string, object>();
        //    local_data.Add("lastmodify", col_cnt.Item3.ToString("s"));
        //    local_data.Add("doc", records.Item2);
        //    var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
        //    System.IO.File.WriteAllText("test.json", jss.Serialize(local_data));
        //}
        static Tuple<bool, Dictionary<string,object>[], DateTime> getAllCollectDocuments(string collectionName, int size = 1000)
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
                        if (DateTime.MinValue==retT && data.ContainsKey("_lastupdated_on") && data["_lastupdated_on"].GetType() == typeof(string))
                        {
                            DateTime t;
                            if (DateTime.TryParse(data["_lastupdated_on"].ToString(), out t))
                                retT = t;
                        }                           
                        if (data.ContainsKey("_returned") && data["_returned"].GetType()==typeof(int))
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
            return new Tuple<bool, Dictionary<string, object>[], DateTime>(ret, (Dictionary<string, object>[])retList.ToArray(typeof(Dictionary<string, object>)), retT);
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
