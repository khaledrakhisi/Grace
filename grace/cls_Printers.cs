using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Management;
using System.Printing;

namespace grace
{

    class cls_Printers
    {
        private static ManagementScope oManagementScope = null;
        //Adds the Printer
        public static bool AddPrinter(string sPrinterName)
        {
            try
            {
                oManagementScope = new ManagementScope(ManagementPath.DefaultPath);
                oManagementScope.Connect();

                ManagementClass oPrinterClass = new ManagementClass(new ManagementPath("Win32_Printer"), null);
                ManagementBaseObject oInputParameters = oPrinterClass.GetMethodParameters("AddPrinterConnection");

                oInputParameters.SetPropertyValue("Name", sPrinterName);

                oPrinterClass.InvokeMethod("AddPrinterConnection", oInputParameters, null);
                return true;
            }
            catch
            {
                return false;
            }
        }
        //Deletes the printer
        public static bool DeletePrinter(string sPrinterName)
        {
            oManagementScope = new ManagementScope(ManagementPath.DefaultPath);
            oManagementScope.Connect();

            SelectQuery oSelectQuery = new SelectQuery();
            oSelectQuery.QueryString = @"SELECT * FROM Win32_Printer WHERE Name = '" + sPrinterName.Replace("\\", "\\\\") + "'";

            ManagementObjectSearcher oObjectSearcher = new ManagementObjectSearcher(oManagementScope, oSelectQuery);
            ManagementObjectCollection oObjectCollection = oObjectSearcher.Get();

            if (oObjectCollection.Count != 0)
            {
                foreach (ManagementObject oItem in oObjectCollection)
                {
                    oItem.Delete();
                    return true;
                }
            }
            return false;
        }
        //Renames the printer
        public static void RenamePrinter(string sPrinterName, string newName)
        {
            oManagementScope = new ManagementScope(ManagementPath.DefaultPath);
            oManagementScope.Connect();

            SelectQuery oSelectQuery = new SelectQuery();
            oSelectQuery.QueryString = @"SELECT * FROM Win32_Printer WHERE Name = '" + sPrinterName.Replace("\\", "\\\\") + "'";

            ManagementObjectSearcher oObjectSearcher = new ManagementObjectSearcher(oManagementScope, oSelectQuery);
            ManagementObjectCollection oObjectCollection = oObjectSearcher.Get();

            if (oObjectCollection.Count != 0)
            {
                foreach (ManagementObject oItem in oObjectCollection)
                {
                    oItem.InvokeMethod("RenamePrinter", new object[] { newName });
                    return;
                }
            }

        }
        //Sets the printer as Default
        public static void SetDefaultPrinter(string sPrinterName)
        {
            oManagementScope = new ManagementScope(ManagementPath.DefaultPath);
            oManagementScope.Connect();

            SelectQuery oSelectQuery = new SelectQuery();
            oSelectQuery.QueryString = @"SELECT * FROM Win32_Printer WHERE Name = '" + sPrinterName.Replace("\\", "\\\\") + "'";

            ManagementObjectSearcher oObjectSearcher = new ManagementObjectSearcher(oManagementScope, oSelectQuery);
            ManagementObjectCollection oObjectCollection = oObjectSearcher.Get();

            if (oObjectCollection.Count != 0)
            {
                foreach (ManagementObject oItem in oObjectCollection)
                {
                    oItem.InvokeMethod("SetDefaultPrinter", new object[] { sPrinterName });
                    return;

                }
            }
        }
        //Gets the printer information
        public static void GetPrinterInfo(string sPrinterName)
        {
            oManagementScope = new ManagementScope(ManagementPath.DefaultPath);
            oManagementScope.Connect();

            SelectQuery oSelectQuery = new SelectQuery();
            oSelectQuery.QueryString = @"SELECT * FROM Win32_Printer WHERE Name = '" + sPrinterName.Replace("\\", "\\\\") + "'";

            ManagementObjectSearcher oObjectSearcher = new ManagementObjectSearcher(oManagementScope, @oSelectQuery);
            ManagementObjectCollection oObjectCollection = oObjectSearcher.Get();

            foreach (ManagementObject oItem in oObjectCollection)
            {
                Console.WriteLine("Name : " + oItem["Name"].ToString());
                Console.WriteLine("PortName : " + oItem["PortName"].ToString());
                Console.WriteLine("DriverName : " + oItem["DriverName"].ToString());
                Console.WriteLine("DeviceID : " + oItem["DeviceID"].ToString());
                Console.WriteLine("Shared : " + oItem["Shared"].ToString());
                Console.WriteLine("---------------------------------------------------------------");
            }
        }
        //Checks whether a printer is installed
        public bool IsPrinterInstalled(string sPrinterName)
        {
            oManagementScope = new ManagementScope(ManagementPath.DefaultPath);
            oManagementScope.Connect();

            SelectQuery oSelectQuery = new SelectQuery();
            oSelectQuery.QueryString = @"SELECT * FROM Win32_Printer WHERE Name = '" + sPrinterName.Replace("\\", "\\\\") + "'";

            ManagementObjectSearcher oObjectSearcher = new ManagementObjectSearcher(oManagementScope, oSelectQuery);
            ManagementObjectCollection oObjectCollection = oObjectSearcher.Get();

            return oObjectCollection.Count > 0;
        }

        public static string PrinterStatus(string s_printerFullName)
        {
            try
            {
                //string printerName = "PrinterName";
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Printer " + "WHERE Name = '" + s_printerFullName.Replace("\\", "\\\\") + "'");

                string s_st = "";
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    s_st = s_st + "\n" + "PrinterStatus: " + queryObj["PrinterStatus"].ToString();
                }

                return s_st;
            }
            catch (ManagementException e)
            {
                throw new Exception("! Exception Occured. ", e);
            }
        }

        public static string GetPrinterInformation(string s_printerFullName)
        {
            // Lookup arrays.
            string[] PrinterStatuses =
                {
                "Other", "Unknown", "Idle", "Printing", "WarmUp",
                "Stopped Printing", "Offline"
            };
            string[] PrinterStates =
                {
                "Paused", "Error", "Pending Deletion", "Paper Jam",
                "Paper Out", "Manual Feed", "Paper Problem",
                "Offline", "IO Active", "Busy", "Printing",
                "Output Bin Full", "Not Available", "Waiting",
                "Processing", "Initialization", "Warming Up",
                "Toner Low", "No Toner", "Page Punt",
                "User Intervention Required", "Out of Memory",
                "Door Open", "Server_Unknown", "Power Save"
            };

            // Get a ManagementObjectSearcher for the printer.
            string query = "SELECT * FROM Win32_Printer WHERE Name='" + s_printerFullName.Replace("\\", "\\\\") + "'";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

            string s_st = "";
            // Get the ManagementObjectCollection representing
            // the result of the WMI query. Loop through its
            // single item. Display some of that item's properties.
            foreach (ManagementObject service in searcher.Get())
            {
                s_st = service.Properties["Name"].Value.ToString();

                UInt32 state = (UInt32)service.Properties["PrinterState"].Value;
                s_st = s_st + "\n" + PrinterStates[state];

                UInt16 status = (UInt16)service.Properties["PrinterStatus"].Value;
                s_st = s_st + "\n" + PrinterStatuses[status];

                s_st = s_st + "\n" + GetPropertyValue(service.Properties["Description"]);
                s_st = s_st + "\n" + GetPropertyValue(service.Properties["Default"]);
                s_st = s_st + "\n" + GetPropertyValue(service.Properties["HorizontalResolution"]);
                s_st = s_st + "\n" + GetPropertyValue(service.Properties["VerticalResolution"]);
                s_st = s_st + "\n" + GetPropertyValue(service.Properties["PortName"]);

                //lstPaperSizes.Items.Clear();
                //string[] paper_sizes = (string[])service.Properties["PrinterPaperNames"].Value;
                //foreach (string paper_size in paper_sizes)
                //{
                //    lstPaperSizes.Items.Add(paper_size);
                //}                
                // List the available properties.
                //foreach (PropertyData data in service.Properties)
                //{
                //    string txt = data.Name;
                //    if (data.Value != null)
                //        txt += ": " + data.Value.ToString();
                //    //MessageBox.Show(txt);
                //}
            }
            return s_st;
        }
        // If the data is not null and has a value, return it.
        private static string GetPropertyValue(PropertyData data)
        {
            if ((data == null) || (data.Value == null)) return "";
            return data.Value.ToString();
        }

        public static void GetAPrinterStatus(string s_printerFullName)
        {
            string query = string.Format("SELECT * from Win32_Printer WHERE Name='" + s_printerFullName.Replace("\\", "\\\\") + "'");

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            using (ManagementObjectCollection coll = searcher.Get())
            {
                try
                {
                    string s_st = "";
                    foreach (ManagementObject printer in coll)
                    {
                        foreach (PropertyData property in printer.Properties)
                        {
                            if (property.Name.ToLower().Contains("stat"))
                            {
                                s_st = s_st + "\n" + string.Format("{0}: {1}", property.Name, property.Value);
                            }
                        }
                    }
                }
                catch (ManagementException ex)
                {
                    throw new Exception("! Exception Occured. ", ex);
                }
            }
        }

        public static void PrintAString(string s, string printerName)
        {

            PrintDocument p = new PrintDocument();
            p.PrintPage += delegate (object sender1, PrintPageEventArgs e1)
            {
                e1.Graphics.DrawString(s, new Font("Arial", 12), new SolidBrush(Color.Black), new RectangleF(0, 0, p.DefaultPageSettings.PrintableArea.Width, p.DefaultPageSettings.PrintableArea.Height));

            };

            try
            {
                if (!string.IsNullOrEmpty(printerName))
                    p.PrinterSettings.PrinterName = printerName;
                p.Print();
            }
            catch (Exception ex)
            {
                throw new Exception("! Exception Occured While Printing", ex);
            }
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
                sServerName = @"\\" + cls_System.GetLocalComputerName();
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
                sServerName = @"\\" + cls_System.GetLocalComputerName();
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
        public static void PurgePrinterJobs(string fullPrinterName)
        {
            //PrintServer hostingServer = GetPrinterServerPointer(fullPrinterName);
            PrintQueue hostingQueue = GetPrinterQueuePointer(fullPrinterName);



            try
            {
                // Create objects to represent the server, queue, and print job.

                //MessageBox.Show(hostingQueue.NumberOfJobs.ToString());
                hostingQueue.Purge();
            }
            catch (Exception ex)
            {
                throw new Exception("! Exception Occured.", ex);
            }

        }
    }

}