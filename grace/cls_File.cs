using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace grace
{
    class cls_File
    {
        public class OpenedFileInfo
        {
            public string fileFullName;
            public IPAddress ipAddress;
            public OpenedFileInfo(string name, IPAddress ip)
            {
                fileFullName = name;
                ipAddress = ip;
            }
        }
        public static OpenedFileInfo openedFileInfo = null;

        [Flags]
        internal enum MoveFileFlags
        {
            None = 0,
            ReplaceExisting = 1,
            CopyAllowed = 2,
            DelayUntilReboot = 4,
            WriteThrough = 8,
            CreateHardlink = 16,
            FailIfNotTrackable = 32,
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool MoveFileEx(
                string lpExistingFileName,
                string lpNewFileName,
                MoveFileFlags dwFlags);

        public static List<DriveInfo> GetDriveList()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            return drives.ToList();
        }

        public static List<FileInfo> GetFileList(string sPath, string sPattern)
        {
            DirectoryInfo d = new DirectoryInfo(sPath);
            FileInfo[] Files = d.GetFiles(sPattern);
            return Files.ToList();
        }

        public static List<DirectoryInfo> GetFolderList(string sPath, string sPattern)
        {
            DirectoryInfo d = new DirectoryInfo(sPath);
            DirectoryInfo[] Folders = d.GetDirectories(sPattern);
            return Folders.ToList();
        }

        //public static string GetFileFolderList(string sPath, string sPattern = "*.*")
        //{
        //    DirectoryInfo d = new DirectoryInfo(sPath);
        //    FileInfo[] Files = d.GetFiles(sPattern);            
        //    string str = "** Files and folders list :\r\n";
        //    int index = 0;
        //    foreach (DirectoryInfo di in d.GetDirectories())
        //    {
        //        str = str + "\r\n" + index++.ToString() + "   "  + "[FOL]\t" + di.Name;
        //    }
        //    foreach (FileInfo file in Files)
        //    {
        //        str = str + "\r\n" + index++.ToString() + "   \t" + file.Name;
        //    }

        //    return str;
        //}

        public static string PopulatePath(string sPath)
        {            
            if (sPath.StartsWith(@".\"))
            {
                sPath = sPath.Replace(@".\", Path.GetDirectoryName(new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath)+"\\");
                //cls_Utility.Log("Path \'.\\' populated as  " + sPath);
            }

            return sPath;
        }

        public static void WriteTextToFile(string sFileFullPath, string sText)
        {
            try
            {
                using (StreamWriter writetext = new StreamWriter(sFileFullPath))
                {
                    writetext.WriteLine(sText);
                }

            }
            catch(Exception ex) 
            {
                throw ex;
            }
        }

        public static string ReadLineFromFile(string sFileFullPath)
        {
            try
            {
                using (StreamReader readtext = new StreamReader(sFileFullPath))
                {
                    return readtext.ReadLine();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string ReadTextFromFile(string sFileFullPath)
        {
            string sLine = "";
            string sText = "";
            try
            {
                using (StreamReader readtext = new StreamReader(sFileFullPath))
                {
                    while ((sLine = readtext.ReadLine()) != null)
                    {
                        if (sText != "") sText += "\r";
                        sText += sLine;
                    }
                    return sText;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
