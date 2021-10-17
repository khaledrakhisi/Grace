using grace;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Windows.Forms;

namespace grace_agent
{
    public partial class frm_agent : Form
    {
        public frm_agent()
        {
            InitializeComponent();

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

            frm_ag = this;
        }

        private static Thread thread;


        // Static form. Null if no form created yet.
        private static frm_agent frm_ag = null;
        private delegate void UpdateDelegate(string sValue);
        // Static method, call the non-static version if the form exist.
        public static void UpdateStaticConsole(string sValue)
        {
            if (frm_ag != null)
                frm_ag.UpdateConsole(sValue);
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
            
            textBox1.AppendText(result);

        }

        private static void PipeServerListening()
        {
            using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("grace_agent_pipe", PipeDirection.Out))
            {
                UpdateStaticConsole("NamedPipeServerStream object created.\r\n");

                // Wait for a client to connect                                
                UpdateStaticConsole("Waiting for client connection...\r\n");

                pipeServer.WaitForConnection();

                //Console.WriteLine("Client connected.");                
                UpdateStaticConsole("Client connected.");
                try
                {
                    // Read user input and send that to the client process.
                    using (StreamWriter sw = new StreamWriter(pipeServer))
                    {
                        sw.AutoFlush = true;                                               
                        UpdateStaticConsole("Enter text: ");
                        sw.WriteLine("from server");
                    }
                }
                // Catch the IOException that is raised if the pipe is broken
                // or disconnected.
                catch (IOException ex)
                {
                    //Console.WriteLine("ERROR: {0}", e.Message);
                    MessageBox.Show("ERROR: " + ex.Message);
                }
            }
        }        

        private void Frm_agent_Load(object sender, EventArgs e)
        {
            //thread = new Thread(PipeServerListening);
            //thread.IsBackground = true;
            //thread.Start();

            //cls_System.BSoD();

            string sResult = "";

            if (File.Exists(cls_File.PopulatePath(@".\abat.abf")))
            {
                string s_text = cls_File.ReadTextFromFile(cls_File.PopulatePath(@".\abat.abf"));
                string [] commandStatements = s_text.Split(new char[] { '\r' });
                foreach (string commandStatement in commandStatements)
                {
                    sResult += "\r" + cls_Interpreter.RunACommand(commandStatement, null);
                    cls_Utility.Log("\r" + sResult);

                    Application.DoEvents();
                }
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(cls_Network.PipeClientReceiveText("grace_pipe"));
        }
    }
}
