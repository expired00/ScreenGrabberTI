using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Linq;

namespace ScreenGrabberTI
{
   static class Capture
    {
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

        public static Bitmap cropAtRect(this Bitmap b, Rectangle r)
        {
            Bitmap nb = new Bitmap(r.Width, r.Height);
            Graphics g = Graphics.FromImage(nb);
            g.DrawImage(b, -r.X, -r.Y);
            return nb;
        }

        public static Bitmap CaptureWindow(IntPtr hWnd)
        {
            System.Drawing.Rectangle rctForm = System.Drawing.Rectangle.Empty;
            using (System.Drawing.Graphics grfx = System.Drawing.Graphics.FromHdc(GetWindowDC(hWnd)))
            {
                rctForm = System.Drawing.Rectangle.Round(grfx.VisibleClipBounds);
            }
            System.Drawing.Bitmap pImage = new System.Drawing.Bitmap(rctForm.Width, rctForm.Height);
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(pImage);
            IntPtr hDC = graphics.GetHdc();
            //paint control onto graphics using provided options        
            try
            {
                PrintWindow(hWnd, hDC, (int)0);
            }
            finally
            {
                graphics.ReleaseHdc(hDC);
            }
            return pImage;
        }

        public static IntPtr FindWindowBrute(string lpClassName, string lpWindowName)
        {
            IntPtr winPtr = IntPtr.Zero;
            int i = 0;
            string temp;
            if(lpWindowName.Contains("#"))
            {
                while (i <= 99)
                {
                    temp = lpWindowName.Replace("#", i.ToString());
                    winPtr = FindWindow(null, temp);
                    if(winPtr != IntPtr.Zero)
                    {
                        return winPtr;
                    }
                    i++;
                }
            }            
            return winPtr;
        }

        public static IntPtr WildcardWindow(string lpClassName, string lpWindowName)
        {
            bool blah = lpWindowName.Contains("*");
            if (lpWindowName.Contains("*") == false)
            {
                if (lpWindowName.Contains("?") == false)
                {                    
                    if (lpWindowName.Contains("#"))
                    {
                        return FindWindowBrute(lpClassName, lpWindowName);
                    }
                    return FindWindow(lpClassName, lpWindowName);
                }
                return FindWindow(lpClassName, lpWindowName);
            }
            IntPtr winPtr = IntPtr.Zero;
            string temp;
            Process[] procs = Process.GetProcesses();
            foreach(Process proc in procs)
            {
                temp = proc.MainWindowTitle;
                if (Regex.IsMatch(proc.MainWindowTitle, WildCardToRegular(lpWindowName)))
                {
                    winPtr = FindWindow(lpClassName, proc.MainWindowTitle);
                }                
            }
            return winPtr;
        }
        private static String WildCardToRegular(String value)
        {
            string temp = "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*").Replace("\\ ", " ") + "$";
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*").Replace("\\ ", " ") + "$";
        }
    }
}
