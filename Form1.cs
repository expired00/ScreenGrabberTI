using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace ScreenGrabberTI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        int start = 0;
        int indexOfSearchText = 0;

        private void Form1_Load(object sender, EventArgs e)
        {
            string appdata = Environment.ExpandEnvironmentVariables("%APPDATA%");
            richTextBox1.Text = File.ReadAllText(appdata + @"\ScreenGrabber\log.txt");
            richTextBox1.ScrollToCaret();
        }

        private void openLogFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Open folder location
            ProcessStartInfo proc = new ProcessStartInfo();
            proc.FileName = "explorer.exe";
            proc.Arguments = Environment.ExpandEnvironmentVariables("%APPDATA%") + @"\ScreenGrabber\";
            Process.Start(proc);
        }

        private void clearLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Clear the log
            if (File.Exists(Environment.ExpandEnvironmentVariables("%APPDATA%") + @"\ScreenGrabber\log.txt"))
            {
                File.Delete(Environment.ExpandEnvironmentVariables("%APPDATA%") + @"\ScreenGrabber\log.txt");
                richTextBox1.Clear();
                LogHelper.Log(LogTarget.File, "Log file cleared.");
                richTextBox1.Text = File.ReadAllText(Environment.ExpandEnvironmentVariables("%APPDATA%") + @"\ScreenGrabber\log.txt");
                richTextBox1.ScrollToCaret();
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int startindex = 0;
            if (toolStripTextBox1.Text.Length > 0)
                startindex = FindMyText(toolStripTextBox1.Text.Trim(), start, richTextBox1.Text.Length);

            // If string was found in the RichTextBox, highlight it
            if (startindex >= 0)
            {
                // Set the highlight color as red
                richTextBox1.SelectionColor = Color.Red;
                // Find the end index. End Index = number of characters in textbox
                int endindex = toolStripTextBox1.Text.Length;
                // Highlight the search string
                richTextBox1.Select(startindex, endindex);
                // mark the start position after the position of
                // last search string
                start = startindex + endindex;
            }
        }

        public int FindMyText(string txtToSearch, int searchStart, int searchEnd)
        {
            // Unselect the previously searched string
            if (searchStart > 0 && searchEnd > 0 && indexOfSearchText >= 0)
            {
                richTextBox1.Undo();
            }

            // Set the return value to -1 by default.
            int retVal = -1;

            // A valid starting index should be specified.
            // if indexOfSearchText = -1, the end of search
            if (searchStart >= 0 && indexOfSearchText >= 0)
            {
                // A valid ending index
                if (searchEnd > searchStart || searchEnd == -1)
                {
                    // Find the position of search string in RichTextBox
                    indexOfSearchText = richTextBox1.Find(txtToSearch, searchStart, searchEnd, RichTextBoxFinds.None);
                    // Determine whether the text was found in richTextBox1.
                    if (indexOfSearchText != -1)
                    {
                        // Return the index to the specified search text.
                        retVal = indexOfSearchText;
                    }
                }
            }
            return retVal;
        }
    }
}
