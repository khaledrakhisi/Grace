using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.ServiceProcess;
using System.Windows.Forms;


namespace grace
{
    public partial class frm_Terminal : Form
    {
        string localAddress = string.Empty;               

        private string sConsoleText = string.Empty;        
       
        public frm_Terminal()
        {
            InitializeComponent();

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

            frm_terminal = this;
        }

        // Static form. Null if no form created yet.
        private static frm_Terminal frm_terminal = null;        
        private delegate void UpdateDelegate(string sValue);
        // Static method, call the non-static version if the form exist.
        public static void UpdateStaticConsole(string sValue)
        {
            if (frm_terminal != null)
                frm_terminal.UpdateConsole(sValue);
        }
        private void UpdateConsole(string sValue)
        {
            // If this returns true, it means it was called from an external thread.
            if (InvokeRequired)
            {
                // Create a delegate of this method and let the form run it.
                this.Invoke(new UpdateDelegate(UpdateConsole), new object[] { sValue });
                return; // Important
            }

            // Update textBox            
            string result = sValue;
            bool b = AnalyzeResult(ref result);
            result = result + (b ? "\r\n\r\n" : "");
            txt_console.AppendText(result);

        }
        //private void UpdateConsole(string sValue)
        //{

        //    string r = sValue;
        //    bool b = AnalyzeResult(ref r);
        //    r = r + (b ? "\r\n\r\n" : "");
        //    txt_console.AppendText(r);

        //}


        private void RunTrigger(string sTriggerName, string sTriggeParameter)
        {
            try
            {
                List<schedule> schedulesMatch = cls_Scheduler.FindATrigger(sTriggerName, sTriggeParameter);
                if (schedulesMatch != null && schedulesMatch.Count > 0)
                {
                    cls_Utility.Log("** Triggers which match the event \'" + sTriggerName + "\' with parameter \'" + sTriggeParameter + "\' are :" + schedulesMatch.Count);
                    foreach (schedule sch in schedulesMatch)
                    {
                        cls_Utility.Log("** Trigger service command: \r\n--" + sch.schedule_command);
                        object result = cls_Interpreter.RunACommand(sch.schedule_command, null);
                        if (result != null)
                            cls_Utility.Log("\r\n" + result);
                    }
                }
            }
            catch (Exception ex)
            {
                cls_Utility.Log("! Error on checking trigger \'onstartup\'." + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RunACommand(cmb_command.Text);

            //cls_Scheduler.SchedulerLoad();

            // Trigger: 'OnStartup' event
            //#region OnStartup
            //RunTrigger("{@onstartup@}", "");
            //#endregion

            // Trigger: 'OnWindowActivate' event
            //#region OnWindowActivate
            //string sTitle = cls_System.GetActiveWindowTitle();
            //RunTrigger("{@onwindowactivate@}", sTitle);
            //#endregion

            //cls_Scheduler.SchedulerLoad();            
            //List<schedule> schedulesMatch = cls_Scheduler.FindASchedule(DateTime.Now.ToString(), 3);
            //if (schedulesMatch != null && schedulesMatch.Count > 0)
            //{                
            //    foreach (schedule sch in schedulesMatch)
            //    {
            //        cls_Utility.Log("** Scheduler service command: \r\n" + sch.schedule_command);
            //        object result = cls_Interpreter.RunACommand(sch.schedule_command);
            //        if (result != null)
            //            cls_Utility.Log("** Scheduler service output: \r\n" + result);
            //    }
            //}
        }

        private void RunACommand(string sCommand)
        {            
            object result = cls_Interpreter.RunACommand(sCommand, cls_Network.cls_IPTools.GetLocalActiveIP(cls_Network.pingableIPAddress, cls_Network.PORT));

            if (result != null)
            {
                // including command statement along with result
                if (cls_Interpreter.isResultWillIncludeCommandStatement)
                {
                    result = "-- " + sCommand + "\r\n" + (string)result;
                }
                UpdateConsole((string)result);
            }

            if (cmb_command.Items.IndexOf(cmb_command.Text) == -1)
                cmb_command.Items.Insert(0, cmb_command.Text);

            cmb_command.Text = string.Empty;
        }

        private bool AnalyzeResult(ref string sResult)
        {
            bool newLineIsNeeded = true;
            string sElement = string.Empty;

            if (sResult == null) return false;

            try
            {
                if (sResult != null && sResult.Contains("{@contentsave@}"))
                {
                    sConsoleText = txt_console.Text;
                    sResult = cls_Utility.RemoveElement("contentsave", sResult);
                    txt_console.ReadOnly = false;
                    txt_console.Focus();

                    newLineIsNeeded = false;
                }

                if (sResult != null && sResult.Contains("{@clear@}"))
                {
                    txt_console.Text = string.Empty;
                    sResult = cls_Utility.RemoveElement("clear", sResult);

                    newLineIsNeeded = false;
                }

                if (sResult != null && sResult.Contains("{@fontsize:"))
                {
                    this.txt_console.Font = new Font(txt_console.Font.FontFamily.Name, float.Parse(cls_Utility.GetElementValue("fontsize", sResult)));
                    sResult = cls_Utility.RemoveElement("fontsize", sResult);

                    newLineIsNeeded = false;
                }
                if (sResult != null && sResult.Contains("{@textcolor:"))
                {
                    this.txt_console.ForeColor = Color.FromArgb(int.Parse(cls_Utility.GetElementValue("textcolor", sResult)));
                    sResult = cls_Utility.RemoveElement("textcolor", sResult);

                    newLineIsNeeded = false;
                }
                if (sResult != null && sResult.Contains("{@backcolor:"))
                {
                    this.txt_console.BackColor = Color.FromArgb(int.Parse(cls_Utility.GetElementValue("backcolor", sResult)));
                    sResult = cls_Utility.RemoveElement("backcolor", sResult);

                    newLineIsNeeded = false;
                }

                if (sResult != null && sResult.Contains("{@address:"))
                {
                    localAddress = cls_Utility.GetElementValue("address", sResult);
                    sResult = cls_Utility.RemoveElement("address", sResult);

                    newLineIsNeeded = false;
                }

                if (sResult != null && sResult.Contains("{@title:"))
                {
                    string sFilePath = cls_Utility.GetElementValue("title", sResult);
                    this.Text += " - (" + localAddress + ")" + sFilePath;
                    sResult = cls_Utility.RemoveElement("title", sResult);

                    IPAddress ip = IPAddress.Parse(localAddress);
                    cls_File.openedFileInfo = new cls_File.OpenedFileInfo(sFilePath, ip);

                    newLineIsNeeded = false;
                }

                if (sResult != null && sResult.Contains("{@notitle@}"))
                {
                    this.Text = "Terminal";
                    sResult = cls_Utility.RemoveElement("notitle", sResult);

                    newLineIsNeeded = false;
                }

                if (sResult != null && sResult.Contains("{@contentload@}"))
                {
                    txt_console.Text = sConsoleText;
                    sResult = cls_Utility.RemoveElement("contentload", sResult);
                    txt_console.ReadOnly = true;
                    cmb_command.Focus();

                    newLineIsNeeded = false;
                }
            }
            catch (Exception ex)
            {
                frm_terminal.UpdateConsole("\r\n! Error : " + ex.Message);
            }

            return newLineIsNeeded;
        }

        

        private void cmb_command_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = btn_run;
        }

        private void cmb_command_Leave(object sender, EventArgs e)
        {
            this.AcceptButton = null;
        }

        private void txt_console_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S && cls_File.openedFileInfo != null)
            {
                //RunACommand("file save," + txt_console.Text);
                RunACommand("friends unicastcommand," + localAddress + ",file save," + txt_console.Text);
            }
        }

        

        public static bool StopApplicationService()
        {
            try
            {
                frm_terminal.UpdateConsole("Attempting to stop " + cls_Utility.serviceName + "...");
            }
            catch { }
            cls_Utility.Log("Attempting to stop " + cls_Utility.serviceName + "...");
            if (cls_System.StopAService(cls_Utility.serviceName) == ServiceControllerStatus.Stopped)
            {
                try
                {
                    frm_terminal.UpdateConsole(cls_Utility.serviceName + " stopped.");
                }
                catch { }
                cls_Utility.Log(cls_Utility.serviceName + " stopped.");

                return true;
            }
            else
            {
                try
                {
                    frm_terminal.UpdateConsole("! Error : Cannot Stop \"" + cls_Utility.serviceName + "\" and because of that the UDP server cannot started.");
                }
                catch { }
                cls_Utility.Log("! Error : Cannot Stop \"" + cls_Utility.serviceName + "\" and because of that the UDP server cannot started.");
            }
            return false;
        }

        private void frm_terminal_Load(object sender, EventArgs e)
        {
            if(StopApplicationService())
                cls_Network.SetupUDPServer();

            cls_Network.PipeServerSetup();
        }

        private void cmb_command_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length != 0)
            {
                cmb_command.Text += files[0];
            }
        }

        private void frm_Terminal_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void frm_Terminal_FormClosing(object sender, FormClosingEventArgs e)
        {
            cls_Network.EndUDPServer();

            try
            {
                frm_terminal.UpdateConsole("Attempting to start " + cls_Utility.serviceName + ". if it has't start please start the service manually.");
            }
            catch { }
            cls_Utility.Log("Attempting to start " + cls_Utility.serviceName + ". if it has't start please start the service manually.");

            int attemp = 1;

            ServiceControllerStatus serviceStatus = ServiceControllerStatus.Stopped;
            while (serviceStatus != ServiceControllerStatus.Running)
            {
                try
                {
                    frm_terminal.UpdateConsole("Attempt #" + attemp);
                }
                catch { }
                cls_Utility.Log("Attempt #" + attemp);

                serviceStatus = cls_System.StartAService(cls_Utility.serviceName);
                if (serviceStatus == ServiceControllerStatus.Running)
                {
                    try
                    {
                        frm_terminal.UpdateConsole("Application service started successfully.");
                    }
                    catch { }
                    cls_Utility.Log("Application service started successfully.");
                }

                Application.DoEvents();

                attemp++;
                if (attemp > 3)
                {
                    try
                    {
                        frm_terminal.UpdateConsole("! Error : Cannot start " + cls_Utility.serviceName + " please start it manually.");
                    }
                    catch { }
                    cls_Utility.Log("! Error : Cannot start " + cls_Utility.serviceName + " please start it manually.");
                    e.Cancel = false;
                    break;
                }
            }
        }
    }
}
