using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Automation;

namespace WindowResizer
{
    public partial class Form1 : Form
    {

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);


        public Dictionary<IntPtr, string> processPairs = new Dictionary<IntPtr, string>();

        public Form1()
        {
            InitializeComponent();

            GetWindows();
        }

        private void GetWindows()
        {
            processPairs.Clear();
            comboBox1.Items.Clear();

            Process[] processlist = Process.GetProcesses();

            foreach (Process process in processlist)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    comboBox1.Items.Add(process.MainWindowTitle);
                    processPairs.Add(process.MainWindowHandle, process.MainWindowTitle);
                }

                Console.WriteLine("Process: {0} ID: {1}", process.ProcessName, process.Id);
            }
        }

        private void OnlyNumber(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(comboBox1.SelectedIndex < 0)
            {
                MessageBox.Show("You didn't select a process to resize.", "No Process Selected", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            else
            {
                try
                {
                    string windowName = comboBox1.Items[comboBox1.SelectedIndex].ToString();

                    int xSize = Int32.Parse(textBox1.Text);
                    int ySize = Int32.Parse(textBox2.Text);

                    IntPtr ApplicationHandle = processPairs.FirstOrDefault(x => x.Value == windowName).Key;

                    MoveWindow(ApplicationHandle, 5, 5, xSize, ySize, true);

                    AutomationElement element = AutomationElement.FromHandle(ApplicationHandle);
                    if (element != null)
                    {
                        element.SetFocus();
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n" + ex.HelpLink + "\n" + ex.StackTrace, "An Exception Occured", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GetWindows();
        }
    }
}
