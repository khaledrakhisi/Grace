using grace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace grace_updater
{
    public partial class frm_Updater : Form
    {
        private const int PORT = 2019;
        private string[] sArgs;
        public frm_Updater(string[] args)
        {
            sArgs = args;
            InitializeComponent();
        }

        [DllImport("advapi32.DLL", SetLastError = true)]
        public static extern int LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        public static void UnicastUDPPacket(IPAddress ip, string sMessage)
        {
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint ipep = new IPEndPoint(ip, PORT);
                byte[] buffer = Encoding.Unicode.GetBytes(sMessage);
                socket.SendTo(buffer, ipep);
                //socket.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateFiles()
        {
            
            //try
            //{
            //    cls_System.KillAProcessByName(cls_Utility.processName);
            Thread.Sleep(5000);
            cls_Utility.Log("** Getting ready for updateing...");
            //}
            //catch (Exception ex)
            //{
            //    cls_Utility.Log("! Cannot kill \'" + cls_Utility.processName + "\' process. " + ex.Message);
            //}
            //try
            //{
            //    frm_Terminal.StopApplicationService();
            //}
            //catch { }
            //string[] sArgs = Environment.GetCommandLineArgs();

            try
            {
                cls_Utility.Log("** Fetching uppath file...");
                string []sUpdateInfo = null;
                //string sUpdatePath = string.Empty, sUsername = string.Empty, sPassword = string.Empty, sdomainname = string.Empty;
                if (File.Exists(cls_Utility.updateConfigFilePath))
                {
                    sUpdateInfo = cls_File.ReadLineFromFile(cls_Utility.updateConfigFilePath).Split('*');
                }
                else
                {
                    UnicastUDPPacket(IPAddress.Parse(sArgs[0]), "! Update configuration file not found. Operation aborted.");
                    cls_Utility.Log("! Update configuration file not found. Operation aborted.");
                    Application.Exit();
                }

                cls_Utility.Log("** Collecting files list...");


                List<FileInfo> files = null;

                // Login to File Server
                IntPtr admin_token = default(IntPtr);
                //Added these 3 lines
                WindowsIdentity wid_current = WindowsIdentity.GetCurrent();
                WindowsIdentity wid_admin = null;
                WindowsImpersonationContext wic = null;                
                if (LogonUser(sUpdateInfo[1], sUpdateInfo[3], sUpdateInfo[2], 9, 0, ref admin_token) != 0)
                {
                    //Newly added lines
                    wid_admin = new WindowsIdentity(admin_token);
                    wic = wid_admin.Impersonate();

                    files = cls_File.GetFileList(sUpdateInfo[0], "*");
                    progressBar1.Maximum = files.Count;
                }
                
                cls_Utility.Log("** Copying...");
                int nCopiedFilesCount = 0;
                foreach (FileInfo file in files)
                {
                    try
                    {
                        File.Copy(file.FullName, Path.Combine(cls_File.PopulatePath(@".\"), file.Name), true);                        
                        nCopiedFilesCount++;
                        cls_Utility.Log("** File \'" + file.Name +"\' copied successfully.");

                    }catch(Exception ex)
                    {
                        //UnicastUDPPacket(IPAddress.Parse(sArgs[0]), "! Error on updating. " + ex.Message);
                        cls_Utility.Log("! Error on copying. " + ex.Message);
                    }
                    progressBar1.PerformStep();
                    Application.DoEvents();
                }

                UnicastUDPPacket(IPAddress.Parse(sArgs[0]), "** \'" + nCopiedFilesCount.ToString() + "\' files copied successfully.");
                cls_Utility.Log("** \'" + nCopiedFilesCount.ToString() + "\' files copied successfully.");
                // Starting the service

                cls_Utility.Log("Attempting to start " + cls_Utility.serviceName + ". if it has't start please start the service manually.");

                int attemp = 1;

                ServiceControllerStatus serviceStatus = ServiceControllerStatus.Stopped;
                while (serviceStatus != ServiceControllerStatus.Running)
                {
                    cls_Utility.Log("Attempt #" + attemp);

                    serviceStatus = cls_System.StartAService(cls_Utility.serviceName);
                    if (serviceStatus == ServiceControllerStatus.Running)
                    {
                        cls_Utility.Log("Application service started successfully.");
                        break;
                    }

                    Application.DoEvents();

                    attemp++;
                    if (attemp > 3)
                    {
                        cls_Utility.Log("! Error : Cannot start " + cls_Utility.serviceName + " please start it manually.");
                        break;
                    }
                }
                
            }
            catch (Exception ex)
            {
                cls_Utility.Log("! Error on grace_updater. " + ex.Message);
            }

            Application.Exit();
            cls_System.StartAService(cls_Utility.serviceName);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);

            //WindowsIdentity idnt = new WindowsIdentity(username, password);

            //WindowsImpersonationContext context = idnt.Impersonate();

            //File.Copy(@"\\172.27.20.4\shabakeh\IT\sport.jpg", @"E:\", true);

            //context.Undo();
            timer1.Enabled = false;
            UpdateFiles(); 
        }

        private void frm_updater_Load(object sender, EventArgs e)
        {
            cls_Utility.Log("** grace_updater Executed successfully.");
        }
    }
}
