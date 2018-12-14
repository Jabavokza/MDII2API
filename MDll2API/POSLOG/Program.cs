using POSLOG.From;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace POSLOG
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static wMain oC_Main;
        public static System.Windows.Forms.NotifyIcon oC_MPosLogNotifyIco;
        public static System.Windows.Forms.ContextMenu oC_NotiIcoMenu;
        public static System.Windows.Forms.MenuItem oC_MenuExit;
        public static System.Windows.Forms.MenuItem oC_MenuOpen;
        public static System.Windows.Forms.MenuItem oC_MenuHide;
        public static bool bPcClose = false;
        private static readonly log4net.ILog oC_Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [STAThread]
        static void Main()
        {
            Mutex mutex = new System.Threading.Mutex(false, "TheMallPosLog");
            try
            {
                if (mutex.WaitOne(0, false))
                {
                    // Run the application
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
                    oC_Main = new wMain();
                    C_INITxNotifyIcon();
                    oC_Main.Activate();
                    oC_Main.Show();
                    Application.Run();
                }
                else
                {
                    MessageBox.Show("An instance of the application is already running.");
                }
            }
            catch (Exception ex)
            {
                oC_Log.Error(ex.Message);
                MessageBox.Show("Program : Main//Unexpected error occur:" + ex);
            }
            finally
            {
                if (mutex != null)
                {
                    mutex.Close();
                    mutex = null;
                }
            }
        }
        static void OnProcessExit(object sender, EventArgs e)
        {
            oC_MPosLogNotifyIco.Dispose();
        }
        public static void C_INITxNotifyIcon()
        {
            try
            {
                oC_MPosLogNotifyIco = new NotifyIcon();
                oC_NotiIcoMenu = new ContextMenu();
                oC_MenuExit = new MenuItem("Close PosLog", new System.EventHandler(C_MenuExit_Click));
                oC_MenuOpen = new MenuItem("Open PosLog", new System.EventHandler(C_MenuOpen_Click));
                oC_MenuHide = new MenuItem("Hide PosLog", new System.EventHandler(C_MenuHide_Click));
                oC_NotiIcoMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { oC_MenuOpen, oC_MenuHide, oC_MenuExit });
                oC_MPosLogNotifyIco.Text = "POSSLOG";
                oC_MPosLogNotifyIco.Icon = POSLOG.Properties.Resources.ontMain_Icon;
                oC_MPosLogNotifyIco.ContextMenu = oC_NotiIcoMenu;
                oC_MPosLogNotifyIco.BalloonTipIcon = ToolTipIcon.Info;
                oC_MPosLogNotifyIco.BalloonTipTitle = "PosLog Manager";
                oC_MPosLogNotifyIco.BalloonTipText = "Left- click to open PosLog Form." + Environment.NewLine +
                                            "Right-click on the icon for more options.";
                oC_MPosLogNotifyIco.ShowBalloonTip(2000);
                oC_MPosLogNotifyIco.Visible = true;
                oC_MPosLogNotifyIco.Click += new System.EventHandler(C_NotiIco_Click);
            }
            catch (Exception ex)
            {
                oC_Log.Error(ex.Message, ex);
                MessageBox.Show("Program initialize failed. See detail in AppPath~\\LogErr\\ErrLog.log", "PosLog Application can not start.");
                Application.Exit();
            }
        }

        private static void C_MenuOpen_Click(object Sender, EventArgs e)
        {
            // Close the form, which closes the application.
            if (oC_Main.IsDisposed)
            {
                oC_Main = new wMain();
            }
            oC_Main.Show();
            oC_Main.WindowState = FormWindowState.Normal;
        }

        private static void C_MenuHide_Click(object Sender, EventArgs e)
        {
            // Close the form, which closes the application.
            if (oC_Main.IsDisposed)
            {
                oC_Main = new wMain();
            }
            oC_Main.Hide();
        }

        private static void C_MenuExit_Click(object Sender, EventArgs e)
        {
            // Close the form, which closes the application.
            oC_MPosLogNotifyIco.Dispose();
            bPcClose = true;
            oC_Main.Close();
            Application.Exit();
        }

        private static void C_NotiIco_Click(object Sender, EventArgs e)
        {
            // Show the form when the user double clicks on the notify icon.

            // Set the WindowState to normal if the form is minimized.
            if (oC_Main.IsDisposed)
            {
                oC_Main = new wMain();
            }
            // Activate the form.
            oC_Main.Show();

            oC_Main.WindowState = FormWindowState.Normal;
        }
    }
}
