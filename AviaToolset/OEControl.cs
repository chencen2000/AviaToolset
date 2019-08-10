using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AviaToolset
{
    class OEControl : ApplicationContext
    {
        const string EVENT_NAME = "OEAPPRUNNING";
        #region win32 APIs
        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        const int WM_COMMAND = 0x111;

        public enum GWL
        {
            GWL_WNDPROC = (-4),
            GWL_HINSTANCE = (-6),
            GWL_HWNDPARENT = (-8),
            GWL_STYLE = (-16),
            GWL_EXSTYLE = (-20),
            GWL_USERDATA = (-21),
            GWL_ID = (-12)
        }
        private enum GetWindowType : uint
        {
            /// <summary>
            /// The retrieved handle identifies the window of the same type that is highest in the Z order.
            /// <para/>
            /// If the specified window is a topmost window, the handle identifies a topmost window.
            /// If the specified window is a top-level window, the handle identifies a top-level window.
            /// If the specified window is a child window, the handle identifies a sibling window.
            /// </summary>
            GW_HWNDFIRST = 0,
            /// <summary>
            /// The retrieved handle identifies the window of the same type that is lowest in the Z order.
            /// <para />
            /// If the specified window is a topmost window, the handle identifies a topmost window.
            /// If the specified window is a top-level window, the handle identifies a top-level window.
            /// If the specified window is a child window, the handle identifies a sibling window.
            /// </summary>
            GW_HWNDLAST = 1,
            /// <summary>
            /// The retrieved handle identifies the window below the specified window in the Z order.
            /// <para />
            /// If the specified window is a topmost window, the handle identifies a topmost window.
            /// If the specified window is a top-level window, the handle identifies a top-level window.
            /// If the specified window is a child window, the handle identifies a sibling window.
            /// </summary>
            GW_HWNDNEXT = 2,
            /// <summary>
            /// The retrieved handle identifies the window above the specified window in the Z order.
            /// <para />
            /// If the specified window is a topmost window, the handle identifies a topmost window.
            /// If the specified window is a top-level window, the handle identifies a top-level window.
            /// If the specified window is a child window, the handle identifies a sibling window.
            /// </summary>
            GW_HWNDPREV = 3,
            /// <summary>
            /// The retrieved handle identifies the specified window's owner window, if any.
            /// </summary>
            GW_OWNER = 4,
            /// <summary>
            /// The retrieved handle identifies the child window at the top of the Z order,
            /// if the specified window is a parent window; otherwise, the retrieved handle is NULL.
            /// The function examines only child windows of the specified window. It does not examine descendant windows.
            /// </summary>
            GW_CHILD = 5,
            /// <summary>
            /// The retrieved handle identifies the enabled popup window owned by the specified window (the
            /// search uses the first such window found using GW_HWNDNEXT); otherwise, if there are no enabled
            /// popup windows, the retrieved handle is that of the specified window.
            /// </summary>
            GW_ENABLEDPOPUP = 6
        }
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, UInt32 wParam, UInt32 lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, UInt32 wParam, UInt32 lParam);
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, GetWindowType uCmd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindow(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);
        #endregion

        static Process start_app()
        {
            Process p = null;
            try
            {
                utility.IniFile config = new utility.IniFile(System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FDHOME"), "Avia", "config.ini"));
                string app = System.IO.Path.GetFullPath(config.GetString("ui", "app", ""));
                if (System.IO.File.Exists(app))
                {
                    // check if the app running.
                    var pp = Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(app));
                    if (pp.Length == 0)
                    {
                        p = new Process();
                        p.StartInfo.FileName = app;
                        p.StartInfo.Arguments = "-ControlMode";
                        p.StartInfo.UseShellExecute = true;
                        p.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(app);
                        p.Start();
                        p.WaitForInputIdle();
                    }
                    else
                    {
                        p = pp[0];
                        //SetForegroundWindow(p.MainWindowHandle);
                    }
                }
            }
            catch (Exception)
            {

            }
            return p;
        }
        static IntPtr[] find_app_wnd(IntPtr desktop, Process p)
        {
            List<IntPtr> wnds = new List<IntPtr>();
            IntPtr wnd = GetWindow(desktop, GetWindowType.GW_CHILD);
            uint pid;
            do
            {
                wnd = GetWindow(wnd, GetWindowType.GW_HWNDNEXT);
                if (IsWindow(wnd))
                {
                    GetWindowThreadProcessId(wnd, out pid);
                    if (pid == p.Id)
                    {
                        wnds.Add(wnd);
                    }
                }
            } while (IsWindow(wnd));
            return wnds.ToArray();
        }
        static IntPtr[] get_children_wnd(IntPtr parentWnd)
        {
            List<IntPtr> ret = new List<IntPtr>();
            if (IsWindow(parentWnd))
            {
                IntPtr wnd = GetWindow(parentWnd, GetWindowType.GW_CHILD);
                while(IsWindow(wnd))
                {
                    ret.Add(wnd);
                    wnd = GetWindow(wnd, GetWindowType.GW_HWNDNEXT);
                }
            }
            return ret.ToArray();
        }
        public static void OE_App_3_0_2_0(System.Threading.EventWaitHandle evt)
        {
            Program.logIt("OE_App_3_0_2_0: ++");
            Process app = start_app();
            if (app != null)
            {
                Program.logIt($"OE_App_3_0_2_0: App started, {app.MainWindowTitle}");
                // to start the process post message WM_COMMAND, 0x3C(0x3D), 0
                System.Threading.Thread.Sleep(5000);
                app.Refresh();
                SetForegroundWindow(app.MainWindowHandle);
                IntPtr mainWnd = app.MainWindowHandle;
                Program.logIt($"OE_App_3_0_2_0: send message to {app.MainWindowTitle}({app.MainWindowHandle}), 0x111,0x3c,0");
                PostMessage(app.MainWindowHandle, WM_COMMAND, 0x3c, 0);
                IntPtr topWnd = GetDesktopWindow();
                Program.logIt($"OE_App_3_0_2_0: desktop Wnd: {topWnd}");
                evt.WaitOne();
                Program.logIt($"OE_App_3_0_2_0: recv terminate event.");
                app.Refresh();
                Program.logIt($"OE_App_3_0_2_0: send message to {app.MainWindowTitle}({app.MainWindowHandle}), 0x111,0x3d,0");
                PostMessage(app.MainWindowHandle, WM_COMMAND, 0x3d, 0);
                System.Threading.Thread.Sleep(5000);
                app.CloseMainWindow();
                app.WaitForExit(3000);
                if (!app.HasExited)
                    app.Kill();
            }
            Program.logIt("OE_App_3_0_2_0: --");
        }

        public static void OE_App_3_0_2_0_2()
        {
            Program.logIt("OE_App_3_0_2_0: ++");
            Process app = start_app();
            if (app != null)
            {
                Program.logIt($"OE_App_3_0_2_0: App started, {app.MainWindowTitle}");
                // to start the process post message WM_COMMAND, 0x3C(0x3D), 0
                System.Threading.Thread.Sleep(5000);
                app.Refresh();
                SetForegroundWindow(app.MainWindowHandle);
                IntPtr mainWnd = app.MainWindowHandle;
                Program.logIt($"OE_App_3_0_2_0: send message to {app.MainWindowTitle}({app.MainWindowHandle}), 0x111,0x3c,0");  
                PostMessage(app.MainWindowHandle, WM_COMMAND, 0x3c, 0);
                IntPtr topWnd = GetDesktopWindow();
                Program.logIt($"OE_App_3_0_2_0: desktop Wnd: {topWnd}");
                // wait for F1 popup                
                IntPtr f1Wnd = IntPtr.Zero;
                while (!IsWindow(f1Wnd))
                {
                    System.Threading.Thread.Sleep(10000);
                    IntPtr[] wnds = find_app_wnd(topWnd, app);
                    foreach (IntPtr p in wnds)
                    {
                        StringBuilder sb = new StringBuilder(512);
                        GetWindowText(p, sb, sb.Capacity);
                        Program.logIt($"Wnd: {p}, {sb.ToString()}");
                        if (string.Compare(sb.ToString(),"Message", true) == 0)
                        {
                            //IntPtr style = GetWindowLongPtr(p, (int) GWL.GWL_STYLE);
                            //Program.logIt($"Wnd: {p}, {sb.ToString()}, isvisible{IsWindowVisible(p)},");
                            if (IsWindowVisible(p))
                            {
                                foreach (IntPtr pp in get_children_wnd(p))
                                {
                                    StringBuilder sb1 = new StringBuilder(512);
                                    GetWindowText(pp, sb1, sb1.Capacity);
                                    //Program.logIt($"Wnd: {pp}, {sb1.ToString()}");
                                    if (string.Compare(sb1.ToString(), "if loading complete please press F1", true) == 0)
                                    {
                                        f1Wnd = p;
                                    }
                                }
                            }
                        }
                    }
                }
                if (IsWindow(f1Wnd))
                {
                    Program.logIt("F1 popup box ready.");
                    SetForegroundWindow(f1Wnd);
                    System.Threading.Thread.Sleep(3000);
                    foreach (IntPtr pp in get_children_wnd(f1Wnd))
                    {
                        StringBuilder sb1 = new StringBuilder(512);
                        GetWindowText(pp, sb1, sb1.Capacity);
                        if (string.Compare(sb1.ToString(), "Abort", true) == 0)
                        {
                            Program.logIt("Send F1.");
                            System.Threading.Thread.Sleep(1000);
                            PostMessage(pp, WM_KEYDOWN, 0x70, 0x003b0001);
                            //System.Threading.Thread.Sleep(20);
                            PostMessage(pp, WM_KEYUP, 0x70, 0xC03B0001);
                        }
                    }
                    PostMessage(app.MainWindowHandle, 0x463, 0xe, 0);
                    PostMessage(app.MainWindowHandle, 0x463, 0xd, 0x3d5e1c0);
                    //System.Threading.Thread.Sleep(5000);
                    //SetForegroundWindow(app.MainWindowHandle);
                    //SetForegroundWindow(f1Wnd);
                    //System.Threading.Thread.Sleep(1000);
                    //Program.logIt("Semd Key TAB");
                    //System.Windows.Forms.SendKeys.SendWait("{F1}");
                    //System.Threading.Thread.Sleep(1000);
                    //Program.logIt("Semd Key Enter");
                    //System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                }

            }
            Program.logIt("OE_App_3_0_2_0: --");
        }
        static void test()
        {

        }
        OEControl()
        {
            Task t = Task.Run(() => 
            {
                //OEControl.OE_App_3_0_2_0();                
            });
            t.ContinueWith(_ =>{
                Program.logIt("OE_App_3_0_2_0: will be terminate.");
                this.ExitThread();
            });
            //
        }
        public static void startup(System.Collections.Specialized.StringDictionary args)
        {
            Program.logIt("OEControl::startup: ++");
            if (args.ContainsKey("start"))
            {
                bool own;
                System.Threading.EventWaitHandle evt = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset, EVENT_NAME, out own);
                if (!own)
                {
                    // OE app already running.
                    Program.logIt("OEControl::startup: app already running");
                }
                else
                {
                    OEControl.OE_App_3_0_2_0(evt);
                }
            }
            else if (args.ContainsKey("stop"))
            {
                try
                {
                    System.Threading.EventWaitHandle evt = System.Threading.EventWaitHandle.OpenExisting(EVENT_NAME);
                    evt.Set();
                }
                catch (Exception) { }
            }
            Program.logIt("OEControl::startup: --");
        }
        [STAThread]
        public static void start(System.Collections.Specialized.StringDictionary args)
        {
            //Application.Run(new OEControl());
            start_app();
        }
    }
}
