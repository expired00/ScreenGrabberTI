using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenGrabberTI
{
    public partial class Form2 : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public IntPtr winPtr;
        public Rectangle rectVal;

        private bool editMode = false;
        private int editIndex;

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            //Populate listbox
            List<List<string>> windowList = Properties.Settings.Default.windowList;            
            foreach(List<string> list in windowList)
            {
                listBox1.Items.Add(list[0]);
            }
            //Populate filepath
            fptextBox.Text = Properties.Settings.Default.filePath;
            timertextBox.Text = Properties.Settings.Default.timer.ToString();
        }

        //Add Button
        private void button1_Click(object sender, EventArgs e)
        {
            if ((nametextBox.Text == "") || (xtextBox.Text == "") || (ytextBox.Text == "") || (widtextBox.Text == "") || (heitextBox.Text == ""))
            {
                MessageBox.Show("All values are required.",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                List<List<string>> windowList = Properties.Settings.Default.windowList;
                List<string> tempList = new List<string>();
                tempList.Add(nametextBox.Text);
                tempList.Add(xtextBox.Text);
                tempList.Add(ytextBox.Text);
                tempList.Add(widtextBox.Text);
                tempList.Add(heitextBox.Text);
                if (editMode)
                {
                    windowList[editIndex] = tempList;
                    button1.Text = "Add";
                    editMode = false;
                    nametextBox.ReadOnly = false;
                    buttonEdit.Enabled = true;
                    button4.Enabled = true;
                    listBox1.Enabled = true;
                }
                else
                {
                    windowList.Add(tempList);
                }                
                Properties.Settings.Default.windowList = windowList;
                Properties.Settings.Default.Save();
                listBox1.Items.Clear();
                int i = 0;
                foreach (List<string> list in windowList)
                {
                    listBox1.Items.Add(list[0] + " - image" + i.ToString() + ".png");
                    i++;
                }
                nametextBox.Text = "";
                xtextBox.Text = "";
                ytextBox.Text = "";
                widtextBox.Text = "";
                heitextBox.Text = "";
            }
        }
        //Edit Button
        private void buttonEdit_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >=0)
            {
                List<List<string>> windowList = Properties.Settings.Default.windowList;
                List<string> tempList = new List<string>();
                tempList = windowList[listBox1.SelectedIndex];
                nametextBox.Text = tempList[0];
                xtextBox.Text = tempList[1];
                ytextBox.Text = tempList[2];
                widtextBox.Text = tempList[3];
                heitextBox.Text = tempList[4];
                nametextBox.ReadOnly = true;
                button1.Text = "Save";
                buttonEdit.Enabled = false;
                button4.Enabled = false;
                listBox1.Enabled = false;
                editMode = true;
                editIndex = listBox1.SelectedIndex;
                Properties.Settings.Default.Save();
            }
            else
            {
                MessageBox.Show("Please select an item from the box.");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            List<List<string>> windowList = Properties.Settings.Default.windowList;
            List<string> tempList = new List<string>();
            tempList = windowList[listBox1.SelectedIndex];
            windowList.RemoveAt(listBox1.SelectedIndex);
            listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            Properties.Settings.Default.Save();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.filePath = fptextBox.Text;
            Properties.Settings.Default.timer = Convert.ToInt32(timertextBox.Text);
            Properties.Settings.Default.Save();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();            
        }

        public static void bringToFront(string title)
        {
            // Get a handle to the application.
            IntPtr handle = FindWindow(null, title);

            // Verify that app is a running process.
            if (handle == IntPtr.Zero)
            {
                return;
            }

            // Make app the foreground application
            SetForegroundWindow(handle);
        }

        private void xtextBox_DoubleClick(object sender, EventArgs e)
        {
            winPtr = ScreenGrabberTI.Capture.WildcardWindow(null, nametextBox.Text);
            if (winPtr == IntPtr.Zero)
            {
                MessageBox.Show("Unable to find a window by that name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            this.Hide();
            Form3 form3 = new Form3();
            form3.winPtr = winPtr;
            form3.ShowDialog();
            if (form3.DialogResult == DialogResult.OK)
            {
                rectVal = form3.rectVal;
                xtextBox.Text = rectVal.X.ToString();
                ytextBox.Text = rectVal.Y.ToString();
                widtextBox.Text = rectVal.Width.ToString();
                heitextBox.Text = rectVal.Height.ToString();
            }
            this.Show();
            button1.Select();
            
        }
    }
}
