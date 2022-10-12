using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace grace
{
    static class cls_Utility
    {
        public static string processName = "grace";
        public static string serviceName = "AugustGrace";
        public static string updateConfigFilePath = cls_File.PopulatePath(@".\uppath.dat");
        public static bool isEchoOff = true;
        public static bool isRemoteLog = false;

        #region Log Classes
        public class Logger
        {
            public string text { get; set; }
            public string from { get; set; }
            public string dateTime { get; set; }
        }
        #endregion

        public static void SaveLogToTheServer(string s_fromAddress, string s_log)
        {
            Logger httpLog = new Logger();
            httpLog.from = s_fromAddress;
            httpLog.text = s_log;

            string json = new JavaScriptSerializer().Serialize(httpLog);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            string JSON_response = cls_Network.Http_POST("api/logs/", httpContent).Result;
            if (JSON_response != null)
                cls_Utility.Log("\r\n" + "Http Log saved successfully.");
        }

        public static void Log(string sLog, bool append = true, string sFileFullName = "")
        {
            try
            {
                if (isRemoteLog)
                {
                    
                }
            }
            catch
            {

            }

            if (isEchoOff) return;
            try
            {
                if (sFileFullName == "")
                {
                    sFileFullName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"gracesvclog.txt");
                }
                using (StreamWriter sw = new StreamWriter(sFileFullName, append))
                {
                    sw.WriteLine(DateTime.Now.ToString("yyyy/MM/dd") + " " + DateTime.Now.ToString("HH:mm:ss") + " " + sLog + "\r\n");
                }
            }
            catch
            {

            }
        }

        public static void ClearLogFile(string sFileFullName = "")
        {
            if (sFileFullName == "")
            {
                sFileFullName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"gracesvclog.txt");
                File.Delete(sFileFullName);
            }
        }

        public static Thread StartThread(Action action)
        {
            Thread thread = new Thread(() => { action(); });
            thread.IsBackground = true;
            thread.Start();
            return thread;
        }

        public static string GetElementValue(string sElementName, string sText)
        {
            if (sText == "" || sText == string.Empty || sText == null) return null;

            string sElement = "{@" + sElementName;
            int pos2 = sText.IndexOf("@}", sText.IndexOf(sElement) + sElement.Length);
            int pos1 = sText.IndexOf(sElement) + sElement.Length + 1;
            sText = sText.Substring(pos1, (pos2 - pos1) < 0 ? 0 : (pos2 - pos1)).Trim();
            return sText;
        }
        public static string RemoveElement(string sElementName, string sText)
        {
            if (sText == "" || sText == string.Empty || sText == null) return null;

            string sElement = "{@" + sElementName;
            int pos2 = sText.IndexOf("@}", sText.IndexOf(sElement) + sElement.Length) + 2;
            int pos1 = sText.IndexOf(sElement);
            sText = sText.Remove(pos1, pos2 - pos1);
            return sText;
        }

        public static string GetDateString(string date)
        {
            DateTime theDate;
            //if (DateTime.TryParseExact(date, "MM/dd/yyyy",
            //        CultureInfo.InvariantCulture, DateTimeStyles.None, out theDate))
            //if (DateTime.ParseExact(date, "MM/dd/yyyy", null))
            try
            {
                theDate = Convert.ToDateTime(date);


                // the string was successfully parsed into theDate
                return theDate.ToString("MM/dd/yyyy");
            }
            catch
            {
                
                //theDate = DateTime.ParseExact(date, "MM/dd/yyyy", null);
                // the parsing failed, return some sensible default value
                return "{@invalid@}";
                //return theDate.ToString("MM/dd/yyyy");
            }
        }
        public static string GetTimeString(string time)
        {
            DateTime theTime;
            //if (DateTime.TryParseExact(time, "HH:mm",
            //        CultureInfo.InvariantCulture, DateTimeStyles.None, out theTime))
            if (DateTime.TryParse(time, out theTime))
            {
                // the string was successfully parsed into theDate
                return theTime.ToString("HH:mm");
            }
            else if(time.Contains("{@every"))
            {
                // the parsing failed, return some sensible default value
                return time;
            }
            else
            {
                return "{@invalid@}";
            }
        }

        public static string UniqFileName(string sAPartFixedName, string sExtension)
        {
            sExtension = sExtension.Contains(".") ? sExtension : "." + sExtension;
            Random rnd = new Random();
            return sAPartFixedName + rnd.Next(1000, 9999).ToString() + sExtension;
        }

        // Return a string describing the value as a file size.
        // For example, 1.23 MB.
        public static string ToFileSize(this double value)
        {
            string[] suffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};
            for (int i = 0; i < suffixes.Length; i++)
            {
                if (value <= (Math.Pow(1024, i + 1)))
                {
                    return ThreeNonZeroDigits(value /
                        Math.Pow(1024, i)) +
                        " " + suffixes[i];
                }
            }

            return ThreeNonZeroDigits(value /
                Math.Pow(1024, suffixes.Length - 1)) +
                " " + suffixes[suffixes.Length - 1];
        }

        // Return the value formatted to include at most three
        // non-zero digits and at most two digits after the
        // decimal point. Examples:
        //         1
        //       123
        //        12.3
        //         1.23
        //         0.12
        private static string ThreeNonZeroDigits(double value)
        {
            if (value >= 100)
            {
                // No digits after the decimal.
                return value.ToString("0,0");
            }
            else if (value >= 10)
            {
                // One digit after the decimal.
                return value.ToString("0.0");
            }
            else
            {
                // Two digits after the decimal.
                return value.ToString("0.00");
            }
        }

        public static List<T> FilterList<T>(List<T> t_list, string s_filter)
        {
            List<T> matchingvalues;

            // Operation excutes on all printers
            if (s_filter == "{@all@}")
            {
                return t_list;
            }
            else if (s_filter.Contains("{@contains:"))
            {
                //t_list.Find(x => x.Contains("seat"))
                matchingvalues = t_list.FindAll(stringToCheck => stringToCheck.ToString().ToLower().Contains(cls_Utility.GetElementValue("contains", s_filter).ToLower()));
                return matchingvalues;
            }

            //Else operation excutes on specified printer by name
            matchingvalues = t_list.FindAll(stringToCheck => stringToCheck.ToString().ToLower() == s_filter.ToLower());
            return matchingvalues;
        }
        
    }
}
