using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;
using System.ServiceProcess;
using System.Runtime.InteropServices;

namespace ScreenGrabberTI
{
    class AppContext : ApplicationContext
    {
        //Component declarations
        private NotifyIcon TrayIcon;
        private ContextMenuStrip TrayIconContextMenu;
        private ToolStripMenuItem CloseMenuItem;
        private ToolStripMenuItem AttachMenuItem;
        private ToolStripMenuItem ConfigurationMenuItem;
        private ToolStripMenuItem LogMenuItem;
        private BackgroundWorker bgWorker;
        private List<string> attList = new List<string>();
        private List<IntPtr> lastPtr = new List<IntPtr>();

        public AppContext()
        {
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            InitializeComponent();
            TrayIcon.Visible = true;
        }

        private void InitializeComponent()
        {            
            //First Launch Setup
            if(Properties.Settings.Default.firstLaunch)
            {
                //Create Log Directory
                string appdata = Environment.ExpandEnvironmentVariables("%APPDATA%");
                if (!Directory.Exists(appdata + @"\ScreenGrabber\"))
                {
                    Directory.CreateDirectory(appdata + @"\ScreenGrabber\");
                }
                LogHelper.Log(LogTarget.File, "Beginning application initialization...");
                //Initialize our list of lists.
                List<List<string>> bigList = new List<List<string>>();
                Properties.Settings.Default.windowList = bigList;
                Properties.Settings.Default.Save();
                LogHelper.Log(LogTarget.File, "Initialization complete.");
                LogHelper.Log(LogTarget.File, "Launching configuration window.");
                //Launch the configuration Form
                Form2 form2 = new Form2();
                if(form2.ShowDialog() == DialogResult.Cancel) {
                    MessageBox.Show("The application will not function correctly if you do not configure it.",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LogHelper.Log(LogTarget.File, "Configuration aborted before completion, First launch failed. Application will not function correctly until configuration has been completed.");
                }
                else
                {
                    LogHelper.Log(LogTarget.File, "First launch completed successfully without errors.");
                }                
                //First Launch is over
                Properties.Settings.Default.firstLaunch = false;
                Properties.Settings.Default.Save();
            }
            LogHelper.Log(LogTarget.File, "Application launched, beginning initialization.");
            //create our bgworker thread
            bgWorker = new BackgroundWorker();
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
            //set up the tray icon
            TrayIcon = new NotifyIcon();
            string title = "Screen Grabber - Unattached";
            string info = "Unattached";
            TrayIcon.BalloonTipIcon = ToolTipIcon.Info;
            TrayIcon.BalloonTipText = info;
            TrayIcon.BalloonTipTitle = title;
            TrayIcon.Text = title;


            //The icon is added to the project resources.
            //Here I assume that the name of the file is 'TrayIcon.ico'
            TrayIcon.Icon = Properties.Resources.TrayIcon;

            //Optional - handle doubleclicks on the icon:
            TrayIcon.DoubleClick += TrayIcon_DoubleClick;

            //Optional - Add a context menu to the TrayIcon:
            TrayIconContextMenu = new ContextMenuStrip();
            CloseMenuItem = new ToolStripMenuItem();
            AttachMenuItem = new ToolStripMenuItem();
            ConfigurationMenuItem = new ToolStripMenuItem();
            LogMenuItem = new ToolStripMenuItem();
            TrayIconContextMenu.SuspendLayout();

            // 
            // TrayIconContextMenu
            // 
            this.TrayIconContextMenu.Items.AddRange(new ToolStripItem[] {
            this.AttachMenuItem, this.LogMenuItem, this.ConfigurationMenuItem, this.CloseMenuItem});
            this.TrayIconContextMenu.Name = "TrayIconContextMenu";
            this.TrayIconContextMenu.Size = new Size(153, 70);
            // 
            // Attach/Unattach
            // 
            this.AttachMenuItem.Name = "AttachItem";
            this.AttachMenuItem.Size = new Size(152, 22);
            this.AttachMenuItem.Text = "Attach";
            this.AttachMenuItem.Click += new EventHandler(this.AttachItem_Click);
            // 
            // Log
            // 
            this.LogMenuItem.Name = "LogItem";
            this.LogMenuItem.Size = new Size(152, 22);
            this.LogMenuItem.Text = "Open Log";
            this.LogMenuItem.Click += new EventHandler(this.LogItem_Click);
            // 
            // Configuration
            // 
            this.ConfigurationMenuItem.Name = "ConfigurationItem";
            this.ConfigurationMenuItem.Size = new Size(152, 22);
            this.ConfigurationMenuItem.Text = "Configuration";
            this.ConfigurationMenuItem.Click += new EventHandler(this.Configuration_Click);
            // 
            // CloseMenuItem
            // 
            this.CloseMenuItem.Name = "CloseMenuItem";
            this.CloseMenuItem.Size = new Size(152, 22);
            this.CloseMenuItem.Text = "Exit";
            this.CloseMenuItem.Click += new EventHandler(this.CloseMenuItem_Click);

            TrayIconContextMenu.ResumeLayout(false);
            TrayIcon.ContextMenuStrip = TrayIconContextMenu;
            //make sure the IIS service is running, and start it if it isn't.
            ServiceController sc = new ServiceController("World Wide Web Publishing Service");
            if ((sc.Status.Equals(ServiceControllerStatus.Stopped) || sc.Status.Equals(ServiceControllerStatus.StopPending)))
            {
                LogHelper.Log(LogTarget.File, "IIS Service is stopped, starting service...");
                sc.Start();
            }
            else
            {
                LogHelper.Log(LogTarget.File, "IIS Service is running.");
            }
            LogHelper.Log(LogTarget.File, "Initialization complete.");
        }

        private void ChangeText(bool attach)
        {
            if (attach)
            {
                LogHelper.Log(LogTarget.File, "Attaching to applications...");
                this.TrayIcon.Text = "Screen Grabber - Attached";
                this.AttachMenuItem.Text = "Unattach";
                StringBuilder sb = new StringBuilder();
                foreach (string item in GetAttached()) { sb.Append(item + "\n"); }
                this.TrayIcon.BalloonTipText = sb.ToString();
                this.TrayIcon.BalloonTipTitle = "Screen Grabber - Attached";
            }
            else
            {
                this.TrayIcon.Text = "Screen Grabber - Unattached";
                this.AttachMenuItem.Text = "Attach";
                StringBuilder sb = new StringBuilder();
                this.TrayIcon.BalloonTipText = "Unattached";
                this.TrayIcon.BalloonTipTitle = "Screen Grabber - Unattached";
            }
        }

        private List<string> GetAttached()
        {            
            return attList;
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            //Cleanup so that the icon will be removed when the application is closed
            TrayIcon.Visible = false;
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            //Here you can do stuff if the tray icon is doubleclicked
            TrayIcon.ShowBalloonTip(10000);
        }

        private void AttachItem_Click(object sender, EventArgs e)
        {
            if (this.AttachMenuItem.Text == "Attach")
            {
                List<List<string>> windowList = Properties.Settings.Default.windowList;
                if(windowList.Count <= 0)
                {
                    LogHelper.Log(LogTarget.File, "Attempted an attach before configuration was completed.");
                    MessageBox.Show("You have not completed configuration, please configure the application before using.","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
                else
                {

                    FileInfo fi = new FileInfo(Environment.ExpandEnvironmentVariables("%APPDATA%" + @"\ScreenGrabber\log.txt"));
                    if(fi.Length >= (long)15000000)
                    {
                        fi.Delete();
                        LogHelper.Log(LogTarget.File, "Log file exceeded maximum size, deleting it and recreating.");
                    }
                    LogHelper.Log(LogTarget.File, "Attaching to applications...");
                    foreach(List<string> list in windowList)
                    {
                        attList.Add(list[0]);
                    }
                    ChangeText(true);
                    TrayIcon.ShowBalloonTip(5000);
                    if (!bgWorker.IsBusy) { bgWorker.RunWorkerAsync(windowList); }
                    else { MessageBox.Show("Already running.. try an unattach. If you see this, you may need to restart the application.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }                    
                }
            }
            else
            {
                LogHelper.Log(LogTarget.File, "Ending the background thread and unattaching... this may take a moment.");
                TrayIcon.BalloonTipText = "Unattaching, this may take a moment...";
                TrayIcon.ShowBalloonTip(5000);
                bgWorker.CancelAsync();
            }

        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LogHelper.Log(LogTarget.File, "Successfully unattached.");
            attList.Clear();
            ChangeText(false);
            TrayIcon.ShowBalloonTip(5000);
            lastPtr.Clear();
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            List<List<String>> windowList = e.Argument as List<List<string>>;
            string filename;
            string filePath = Properties.Settings.Default.filePath;
            int timer = Properties.Settings.Default.timer * 1000;
            IntPtr hWnd;
            Rectangle rect = new Rectangle();
            Bitmap image;
            Bitmap cropped;
            int x = 0;
            bool loop = true;
            while (loop)
            {                
                //stop the loop if user has triggered a cancel
                if (bgWorker.CancellationPending)
                {                    
                    return;
                }
                x = 0;
                foreach (List<string> AppList in windowList)
                {
                    try
                    {
                        //get our rectangle from the saved values
                        rect.X = Convert.ToInt32(AppList[1]);
                        rect.Y = Convert.ToInt32(AppList[2]);
                        rect.Width = Convert.ToInt32(AppList[3]);
                        rect.Height = Convert.ToInt32(AppList[4]);
                        if (lastPtr.Count() == windowList.Count())
                        {                            
                            hWnd = lastPtr[x];
                        }
                        else
                        {
                            LogHelper.Log(LogTarget.File, "Searching for window handles...");
                            hWnd = Capture.WildcardWindow(null, AppList[0]);
                            if (hWnd != IntPtr.Zero)
                            {
                                lastPtr.Add(hWnd);
                                LogHelper.Log(LogTarget.File, "Located window handle " + hWnd.ToString() + " attaching now...");
                            }
                        }                        
                        filename = filePath + "image" + x.ToString() + ".png";
                        //capture the screenshot using the windows display hooks
                        image = Capture.CaptureWindow(hWnd);
                        //delete the existing image
                        if (File.Exists(filename)) { File.Delete(filename); }
                        //crop the image to the desired size.
                        cropped = Capture.cropAtRect(image, rect);
                        cropped.Save(filename);
                        x++;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log(LogTarget.File, "Failed to capture the screenshot.\n" + ex);
                        LogHelper.Log(LogTarget.File, "Window handles cleared, attempting a re-attach.");
                        lastPtr.Clear();
                    }
            }
                //Extra garbage collection
                GC.Collect();
                //Sleep the loop based on interval
                System.Threading.Thread.Sleep(timer);
            }
        }

        private void LogItem_Click(object sender, EventArgs e)
        {
            Form1 form = new Form1();
            form.Show();
        }

        private void Configuration_Click(object sender, EventArgs e)
        {
            Form2 form = new Form2();
            form.Show();
        }

        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit?",
                    "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AppContext());
        }
    }
}
