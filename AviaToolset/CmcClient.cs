using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AviaToolset
{
    class CmcClient
    {
        public static int sendTransactionToVerizon(System.Collections.Specialized.StringDictionary args)
        {
            int ret = -1;
            Program.logIt("sendTransactionToVerizon: ++");
            string avia_dir = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "AVIA");
            string tool = System.IO.Path.Combine(avia_dir, "hydra", "hydraTransaction.exe");
            Dictionary<string, object> data = null;
            string url = args.ContainsKey("verizonurl") ? args["verizonurl"] : "";
            if (args.ContainsKey("json") && System.IO.File.Exists(args["json"]))
            {
                try
                {
                    var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                    data = jss.Deserialize<Dictionary<string, object>>(System.IO.File.ReadAllText(args["json"]));
                }
                catch (Exception ex)
                {
                    Program.logIt($"sendTransaction: {ex.Message}");
                    ret = 1;
                }
            }
            if (!string.IsNullOrEmpty(url) && data != null && System.IO.File.Exists(tool) && System.IO.File.Exists(System.IO.Path.Combine(avia_dir, "config.ini")))
            {
                utility.IniFile config = new utility.IniFile(System.IO.Path.Combine(avia_dir, "config.ini"));
                if (System.IO.File.Exists(tool))
                {
                    var ms = new MemoryStream();
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    using (XmlWriter xmlWriter = XmlWriter.Create(ms, settings))
                    {
                        xmlWriter.WriteStartDocument();
                        xmlWriter.WriteStartElement("root");
                        xmlWriter.WriteElementString("ESN", data.ContainsKey("Index") ? data["Index"].ToString() : "1234567890");
                        xmlWriter.WriteElementString("ProcessingStation", System.Environment.MachineName);
                        xmlWriter.WriteStartElement("AutomationResult");
                        xmlWriter.WriteStartElement("TRANDATA");
                        xmlWriter.WriteElementString("HEXESN", data.ContainsKey("Index") ? data["Index"].ToString() : "1234567890");
                        int error_code = 1;
                        if (data.ContainsKey("Grade"))
                        {
                            error_code = 1;
                            xmlWriter.WriteElementString("GRADE", data.ContainsKey("Grade").ToString());
                        }
                        else
                        {
                            xmlWriter.WriteElementString("GRADE", "Fail");
                            error_code = 0;
                        }
                        xmlWriter.WriteElementString("CRACK", "");
                        xmlWriter.WriteElementString("POWER", "TRUE");
                        xmlWriter.WriteElementString("RESULT", error_code==1? "PASS": "FAIL");
                        xmlWriter.WriteElementString("TIMESTAMP", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss"));
                        xmlWriter.WriteEndElement();
                        xmlWriter.WriteEndElement();
                        xmlWriter.WriteEndElement();
                        xmlWriter.WriteEndDocument();
                        xmlWriter.Flush();
                        xmlWriter.Close();
                    }
                    try
                    {
                        Program.logIt($"try to send transaction to url: {url}");
                        WebClient wc = new WebClient();
                        byte[] res = wc.UploadData(url, ms.ToArray());
                        string s = System.Text.Encoding.UTF8.GetString(res);
                        Program.logIt($" to send transaction to url: {url}");
                    }
                    catch(Exception ex)
                    {
                        Program.logIt(ex.Message);
                        Program.logIt(ex.StackTrace);
                    }

                    ret = 0;
                }
            }
            Program.logIt($"sendTransactionToVerizon: -- ret={ret}");
            return ret;
        }
        public static int sendTransaction(System.Collections.Specialized.StringDictionary args)
        {
            int ret = -1;
            Program.logIt($"sendTransaction: ++ {args["json"]}");
            string avia_dir = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "AVIA");
            string tool = System.IO.Path.Combine(avia_dir, "hydra", "hydraTransaction.exe");
            utility.IniFile ini = new utility.IniFile(System.IO.Path.Combine(avia_dir, "AviaDevice.ini"));
            Dictionary<string, object> data = null;
            if (args.ContainsKey("json") && System.IO.File.Exists(args["json"]))
            {
                try
                {
                    var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                    data = jss.Deserialize<Dictionary<string, object>>(System.IO.File.ReadAllText(args["json"]));
                    string imei = ini.GetString("device", "imei", "");
                    if (!string.IsNullOrEmpty(imei))
                    {
                        data["Index"] = imei;
                    }
                }
                catch (Exception ex)
                {
                    Program.logIt($"sendTransaction: {ex.Message}");
                    ret = 1;
                }
            }
            if (data != null && System.IO.File.Exists(tool) && System.IO.File.Exists(System.IO.Path.Combine(avia_dir,"config.ini")))
            {
                utility.IniFile config = new utility.IniFile(System.IO.Path.Combine(avia_dir, "config.ini"));

                // save label.xml
                prepare_label_xml(config, data);

                string vzw_url = config.GetString("avia", "verizonurl", "");
                if (!string.IsNullOrEmpty(vzw_url))
                {
                    // send transaction to verizon interface.
                    if (!args.ContainsKey("verizonurl"))
                        args.Add("verizonurl", vzw_url);
                    sendTransactionToVerizon(args);
                }

                if (System.IO.File.Exists(tool))
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    using (XmlWriter xmlWriter = XmlWriter.Create("test.xml", settings))
                    {
                        xmlWriter.WriteStartDocument();
                        xmlWriter.WriteStartElement("TransLog");
                        xmlWriter.WriteStartElement("FDEMT_TransactionRecord");
                        xmlWriter.WriteElementString("company", config.GetString("config", "companyid", "9"));
                        xmlWriter.WriteElementString("site", config.GetString("config", "siteid", "1"));
                        xmlWriter.WriteElementString("operator", config.GetString("config", "userid", "144"));
                        xmlWriter.WriteElementString("productid", config.GetString("config", "productid", "41"));
                        xmlWriter.WriteElementString("solutionid", config.GetString("config", "solutionid", "34"));
                        xmlWriter.WriteElementString("workstationName", System.Environment.MachineName);
                        xmlWriter.WriteElementString("sourcePhoneID", "PST_APE_UNIVERSAL_USB_FD");
                        xmlWriter.WriteElementString("sourceMake", "Apple");
                        xmlWriter.WriteElementString("sourceModel", data.ContainsKey("ModelName") ? data["ModelName"].ToString() : "");
                        xmlWriter.WriteElementString("esnNumber", data.ContainsKey("Index") ? data["Index"].ToString() : "1234567890");
                        xmlWriter.WriteElementString("StartTime", data.ContainsKey("InspectionTime") ? data["InspectionTime"].ToString() : DateTime.Now.ToString("G"));
                        xmlWriter.WriteElementString("CriteriaFileName", data.ContainsKey("CriteriaFileName") ? System.IO.Path.GetFileName(data["CriteriaFileName"].ToString()) : "");
                        int error_code = 1;
                        if (data.ContainsKey("Grade"))
                        {
                            error_code = 1;
                            string s = data["Grade"].ToString();
                            string s1 = ini.GetString("override", "grade", s);
                            xmlWriter.WriteElementString("grade", s1);
                            if (string.IsNullOrEmpty(s1))
                                s1 = "D";
                            ini.WriteValue("device", "grade", s1);
                        }
                        else
                        {
                            error_code = 0;
                            ini.WriteValue("device", "grade", "D");
                        }
                        xmlWriter.WriteElementString("errorCode", error_code.ToString());
                        xmlWriter.WriteElementString("timeCreated", DateTime.Now.ToString("G"));
                        xmlWriter.WriteEndElement();
                        xmlWriter.WriteEndElement();
                        xmlWriter.WriteEndDocument();
                        xmlWriter.Flush();
                        xmlWriter.Close();
                    }

                    Process p = new Process();
                    p.StartInfo.FileName = tool;
                    p.StartInfo.Arguments = $"-add -config={config.Path} -xml=test.xml";
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.Start();
                    p.WaitForExit();
                    ret = 0;
                }
            }
            if (args.ContainsKey("json"))
            {
                try
                {
                    System.IO.File.Delete(args["json"]);
                }
                catch (Exception) { }
            }
            Program.logIt($"sendTransaction: -- ret={ret}");
            return ret;
        }
        static void prepare_label_xml(utility.IniFile config, Dictionary<string,object> data, string labelname="label.xml")
        {
            string fn = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "AVIA", labelname);
            try
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                using (XmlWriter xmlWriter = XmlWriter.Create(fn, settings))
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("labelinfo");
                    xmlWriter.WriteStartElement("label");
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteStartElement("device");
                    xmlWriter.WriteElementString("meid_imei", data.ContainsKey("Index") ? data["Index"].ToString() : "1234567890");
                    xmlWriter.WriteElementString("modelnumber", data.ContainsKey("ModelName") ? data["ModelName"].ToString() : "");
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteStartElement("runtime");
                    xmlWriter.WriteElementString("iCloud", data.ContainsKey("Grade") ? data["Grade"].ToString() : "");
                    xmlWriter.WriteElementString("meid", data.ContainsKey("Index") ? data["Index"].ToString() : "1234567890");
                    xmlWriter.WriteElementString("modelnumber", data.ContainsKey("ModelName") ? data["ModelName"].ToString() : "");
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndDocument();
                    xmlWriter.Flush();
                    xmlWriter.Close();
                }
            }
            catch (Exception) { }
        }
        public static int cmc_login(System.Collections.Specialized.StringDictionary args)
        {
            int ret = -1;
            Program.logIt("cmc_login: ++");
            string avia_dir = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "AVIA");
            string tool = System.IO.Path.Combine(avia_dir, "hydra", "hydralogin.exe");
            string xml_file = System.IO.Path.Combine(avia_dir, "hydra", "hydralogin.xml");
            utility.IniFile config = new utility.IniFile(System.IO.Path.Combine(avia_dir, "config.ini"));
            if (System.IO.File.Exists(tool) && args.ContainsKey("u") && args.ContainsKey("p"))
            {
                try
                {
                    System.IO.File.Delete(xml_file);
                }
                catch (Exception) { }
                Process p = new Process();
                p.StartInfo.FileName = tool;
                p.StartInfo.Arguments = $"-u={args["u"]} -p={args["p"]}";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.EnvironmentVariables.Add("APSTHOME", avia_dir);
                p.Start();
                p.WaitForExit();
                //ret = p.ExitCode;
                if (p.ExitCode == 0 && System.IO.File.Exists(xml_file))
                {
                    // parse hydralogin xml
                    XmlDocument xml = new XmlDocument();
                    try
                    {
                        xml.Load(xml_file);
                        if (xml.DocumentElement != null)
                        {
                            if (xml.DocumentElement["id"] != null)
                            {
                                config.WriteValue("config", "userid", xml.DocumentElement["id"].InnerText);
                                ret = 0;
                            }
                        }
                    }
                    catch (Exception) { }
                }
            }
            Program.logIt($"cmc_login: -- ret={ret}");
            return ret;
        }
    }
}
