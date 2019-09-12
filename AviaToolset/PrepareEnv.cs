using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace AviaToolset
{
    class PrepareEnv
    {
        public static int cleanEnv(System.Collections.Specialized.StringDictionary args)
        {
            int ret = -1;
            string dir = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("fdhome"), "avia");
            System.Environment.CurrentDirectory = dir;
            // 1. start FDPhoneRecognition.exe -kill-tcpserver
            string tool = System.IO.Path.Combine(dir, "FDPhoneRecognition.exe");
            if (System.IO.File.Exists(tool))
            {
                Process p = new Process();
                p.StartInfo.FileName = tool;
                p.StartInfo.Arguments = $"-kill-tcpserver";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
            }
            // 2. 
            return ret;
        }
        /// <summary>
        /// prepare env for AVIA product
        /// 
        /// save flag in avia.ini, 
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int startup(System.Collections.Specialized.StringDictionary args)
        {
            int ret = -1;
            string dir = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("fdhome"), "avia");
            System.Environment.CurrentDirectory = dir;
            // 1. start FDPhoneRecognition.exe -start-tcpserver
            string tool = System.IO.Path.Combine(dir, "FDPhoneRecognition.exe");
            if(System.IO.File.Exists(tool))
            {
                Process p = new Process();
                p.StartInfo.FileName = tool;
                p.StartInfo.Arguments = $"-start-tcpserver";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
            }
            // 2. start AviaToolset.exe -OEControl
            utility.IniFile config = new utility.IniFile(System.IO.Path.Combine(dir, "config.ini"));
            tool = config.GetString("ui", "app", @"evaoi-3.1.0.3\evaoi-3.1.0.3.exe");
            tool = System.IO.Path.GetFullPath(tool);
            if (System.IO.File.Exists(tool))
            {
                Process p = new Process();
                p.StartInfo.FileName = tool;
                p.StartInfo.Arguments = $"-ControlMode";
                p.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(tool);
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                p.WaitForInputIdle();
            }
            // 3. prepare models and save into aviadevice.ini
            try
            {
                tool = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(tool), "evaoi.xml");
                if (System.IO.File.Exists(tool))
                {
                    Tuple<bool, string, SizeF> res = get_modeldir_and_ratio(tool);
                    if (res.Item1)
                    {
                        utility.IniFile ad = new utility.IniFile(System.IO.Path.Combine(dir, "aviadevice.ini"));
                        foreach (string model in System.IO.Directory.GetDirectories(res.Item2))
                        {
                            //Program.logIt($"model: {System.IO.Path.GetFileName(model)}");
                            tool = System.IO.Path.Combine(model, "work_station_1", "layout.xml");
                            Tuple<Rectangle, bool, string>[] areas = retrieve_area_by_filename(tool);
                            SizeF sz = SizeF.Empty;
                            foreach(Tuple<Rectangle, bool, string> i in areas)
                            {
                                SizeF s = new SizeF(res.Item3.Width * i.Item1.Width, res.Item3.Height * i.Item1.Height);
                                //Program.logIt($"size={sz}, {i.Item1}");
                                if (s.Width > sz.Width && s.Height > sz.Height)
                                    sz = s;
                            }
                            Program.logIt($"model: {System.IO.Path.GetFileName(model)}, size={sz}");
                            ad.WriteValue("models", System.IO.Path.GetFileName(model), $"{sz.Width},{sz.Height}");
                        }
                    }
                }
            }
            catch (Exception) { }
            return ret;
        }

        public static Tuple<bool, string,SizeF> get_modeldir_and_ratio(string xmlfilename)
        {
            bool ret = false;
            string model_dir = string.Empty;
            SizeF rets = SizeF.Empty;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlfilename);
                if (doc.DocumentElement != null)
                {
                    model_dir = doc?.DocumentElement?["system"]?["ModelDir"]?.InnerText;
                    if (System.IO.Directory.Exists(model_dir))
                    {
                        ret = true;
                    }
                    XmlNode n = doc.DocumentElement.SelectSingleNode("work_station/item[name='BACK']/system");
                    if (n != null)
                    {
                        string s = n["PixelSize"]?.InnerText;
                        float f;
                        if(float.TryParse(s,out f))
                        {
                            rets = new SizeF(f, f);
                            ret = true;
                        }
                    }
                }
            }
            catch (Exception) { }
            return new Tuple<bool, string, SizeF>(ret, model_dir, rets);
        }
        static Tuple<Rectangle, bool, string>[] retrieve_area_by_filename(string filename)
        {
            List<Tuple<Rectangle, bool, string>> area = new List<Tuple<Rectangle, bool, string>>();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filename);
                XmlNodeList reg_list = doc.DocumentElement.SelectNodes("//region");
                foreach (XmlNode n in reg_list)
                {
                    Rectangle r = Rectangle.Empty;
                    Point p = Point.Empty;
                    bool is_mask = true;
                    string path = get_path_by_node(n);
                    if (n["is_mask"] != null)
                    {
                        if (Int32.Parse(n["is_mask"].InnerText) == 0)
                            is_mask = false;
                    }
                    //if (is_mask == 1)
                    {
                        if (n["center"] != null)
                        {
                            string s = n["center"].InnerText;
                            string[] ss = s.Split(',');
                            float x = float.Parse(ss[0]);
                            float y = float.Parse(ss[1]);
                            PointF pf = new PointF(x, y);
                            p = Point.Round(pf);
                        }
                        if (n["radius"] != null)
                        {
                            float f = float.Parse(n["radius"].InnerText);
                            int x = p.X - (int)f;
                            int y = p.Y - (int)f;
                            int w = (int)(2 * f);
                            int h = (int)(2 * f);
                            r = new Rectangle(x, y, w, h);
                        }
                        if (n["width"] != null && n["height"] != null)
                        {
                            int w = int.Parse(n["width"].InnerText);
                            int h = int.Parse(n["height"].InnerText);
                            int x = p.X - w / 2;
                            int y = p.Y - h / 2;
                            r = new Rectangle(x, y, w, h);
                        }
                        if (!r.IsEmpty)
                        {
                            //Program.logIt($"center={p}, rect={r}");
                            Tuple<Rectangle, bool, string> i = new Tuple<Rectangle, bool, string>(r, is_mask, path);
                            area.Add(i);
                        }
                    }
                }
            }
            catch (Exception) { }
            return area.ToArray();
        }
        static string get_path_by_node(XmlNode node)
        {
            Stack<string> sk = new Stack<string>();
            while (node != null)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    sk.Push(node.Name);
                    node = node.ParentNode;
                }
                if (node.NodeType == XmlNodeType.Document)
                    break;
            }
            StringBuilder sb = new StringBuilder();
            foreach (string s in sk)
            {
                sb.Append(s);
                sb.Append('/');
            }
            return sb.ToString();
        }

    }
}
