using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AviaToolset
{
    class CmcClient
    {
        public static int sendTransaction(System.Collections.Specialized.StringDictionary args)
        {
            int ret = -1;
            Program.logIt("sendTransaction: ++");
            string avia_dir = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "AVIA");
            string tool = System.IO.Path.Combine(avia_dir, "hydra", "hydraTransaction.exe");
            Dictionary<string, object> data = null;
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
            if (data != null && System.IO.File.Exists(tool) && System.IO.File.Exists(System.IO.Path.Combine(avia_dir,"config.ini")))
            {
                utility.IniFile config = new utility.IniFile(System.IO.Path.Combine(avia_dir, "config.ini"));
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
                        xmlWriter.WriteElementString("StartTime", data.ContainsKey("InspectionTime") ? data["InspectionTime"].ToString() : "");
                        xmlWriter.WriteElementString("CriteriaFileName", data.ContainsKey("CriteriaFileName") ? System.IO.Path.GetFileName(data["CriteriaFileName"].ToString()) : "");
                        int error_code = 1;
                        if (data.ContainsKey("Grade"))
                        {
                            error_code = 1;
                            xmlWriter.WriteElementString("grade", data.ContainsKey("Grade") ? data["Grade"].ToString() : "");
                        }
                        else
                        {
                            error_code = 0;
                        }
                        xmlWriter.WriteElementString("errorCode", error_code.ToString());
                        xmlWriter.WriteElementString("timeCreated", DateTime.Now.ToString("o"));
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
    }
}
