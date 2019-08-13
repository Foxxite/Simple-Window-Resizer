using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Management;
using System.IO;
using Newtonsoft.Json;

namespace WindowResizer
{
    public partial class Form1 : Form
    {

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        public static Dictionary<IntPtr, string> processPairs = new Dictionary<IntPtr, string>();

        public static Dictionary<SavedProgram, string> savedProgramsList = new Dictionary<SavedProgram, string>();

        public static Dictionary<string, bool> programResized = new Dictionary<string, bool>();

        public Form1()
        {
            InitializeComponent();

            GetWindows();

            LoadSavedPrograms();

            WaitForProcess();
        }

        #region WindowResize

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

        #endregion

        #region Misc

        private void button2_Click(object sender, EventArgs e)
        {
            GetWindows();
        }

        private void OnlyNumber(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void LoadSavedPrograms()
        {
            if (Directory.Exists(Application.StartupPath + "/saved") && File.Exists(Application.StartupPath + "/saved/savedPrograms.foxsave"))
            {
                string[] lines = File.ReadAllLines(Application.StartupPath + "/saved/savedPrograms.foxsave");

                foreach(string line in lines)
                {
                    if(!String.IsNullOrEmpty(line))
                    {
                        SavedProgram SavedProgram = JsonConvert.DeserializeObject<SavedProgram>(line);

                        if (savedProgramsList.FirstOrDefault(x => x.Value == SavedProgram.ApplicationMainWindowTitle).Value == null)
                        {
                            savedProgramsList.Add(SavedProgram, SavedProgram.ApplicationMainWindowTitle);
                        }
                    }
                }

            }
        }

        #endregion

        #region AutoResize

        /*
         * 
         *  Auto Resize Saved Programs
         * 
        */

        private void WaitForProcess()
        {
            ManagementEventWatcher startWatch = new ManagementEventWatcher(
            new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            startWatch.EventArrived += new EventArrivedEventHandler(startWatch_EventArrived);
            startWatch.Start();
        }

        static void startWatch_EventArrived(object sender, EventArrivedEventArgs e)
        {
            Process[] processlist = Process.GetProcesses();

            foreach (Process process in processlist)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    if (savedProgramsList.FirstOrDefault(x => x.Value == process.MainWindowTitle).Value != null && (programResized.FirstOrDefault(x => x.Key == process.MainWindowTitle).Value == false))
                    {
                        try
                        {
                            SavedProgram program = savedProgramsList.FirstOrDefault(x => x.Value == process.MainWindowTitle).Key;

                            int xSize = program.x;
                            int ySize = program.y;

                            IntPtr ApplicationHandle = Process.GetProcesses().FirstOrDefault(x => x.MainWindowTitle == program.ApplicationMainWindowTitle).MainWindowHandle;

                            MoveWindow(ApplicationHandle, 5, 5, xSize, ySize, true);

                            AutomationElement element = AutomationElement.FromHandle(ApplicationHandle);
                            if (element != null)
                            {
                                element.SetFocus();
                            }

                            programResized.Add(program.ApplicationMainWindowTitle, true);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message + "\n" + ex.HelpLink + "\n" + ex.StackTrace, "An Exception Occured", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        }
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            LoadSavedPrograms();
            
            if (comboBox1.SelectedIndex < 0)
            {
                MessageBox.Show("You didn't select a process to resize.", "No Process Selected", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            else
            {
                try
                {
                    string processName = comboBox1.Items[comboBox1.SelectedIndex].ToString();

                    int xSize = Int32.Parse(textBox1.Text);
                    int ySize = Int32.Parse(textBox2.Text);

                    SavedProgram thisProgam = new SavedProgram(processName, xSize, ySize);

                    if (savedProgramsList.FirstOrDefault(x => x.Value == thisProgam.ApplicationMainWindowTitle).Value != null)
                    {
                        MessageBox.Show("This program is already saved, edit it's settings in the .foxsave file", "Program already saved", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }

                    var json = JsonConvert.SerializeObject(thisProgam);

                    if(!Directory.Exists(Application.StartupPath + "/saved"))
                    {
                        Directory.CreateDirectory(Application.StartupPath + "/saved");
                    }

                    File.AppendAllText(Application.StartupPath + "/saved/savedPrograms.foxsave", "\n"+json);

                    MessageBox.Show("The program " + thisProgam.ApplicationMainWindowTitle + " has been saved with the settings: X: " + thisProgam.x + ", Y: " + thisProgam.y + ". And will automatically be resized to these settings if this app is running.", "Program Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);


                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n" + ex.HelpLink + "\n" + ex.StackTrace, "An Exception Occured", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }

        }
    }

#endregion

}
