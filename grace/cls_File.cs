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

        public static bool IsFileExist(string fileFullPath)
        {
            return File.Exists(cls_File.PopulatePath(fileFullPath));
        }

        public static string [] GetAllRecursively(string sExtensions, string sDir)
        {
            List<string> extensions;
            string[] files;
            if (sExtensions == "*.*" || sExtensions == ".*")
            {                
                files = Directory.GetFiles(sDir, "*.*", SearchOption.AllDirectories).ToArray();
            }
            else
            {
                extensions = sExtensions.ToLower().Split(new char[] { ';' }).ToList();
                files = Directory.GetFiles(sDir, "*.*", SearchOption.AllDirectories)
                                .Where(f => extensions.IndexOf(Path.GetExtension(f).ToLower()) >= 0).ToArray();
            }

                
            return files;
        }

        private static void PrependString(string value, FileStream file, bool isRecover)
        {
            byte[] buffer;
            try
            {
                cls_Utility.Log("HEREEE13");
                buffer = new byte[file.Length];
                cls_Utility.Log("HEREEE14");
                while (file.Read(buffer, 0, buffer.Length) != 0)
                {
                }
                cls_Utility.Log("HEREEE15");
                if (!file.CanWrite)
                    throw new ArgumentException("The specified file cannot be written.", "file");
            }
            catch
            {
                throw new Exception("! Cannot read the file.");
            }

            byte[] data;
            byte[] firstChunk;
            byte[] secondChunk;
            string sTail;
            try
            {
                cls_Utility.Log("HEREEE16");
                file.Position = 0;
                data = Encoding.Unicode.GetBytes(value);
                firstChunk = buffer.Take(200).ToArray();
                secondChunk = buffer.Skip(200).ToArray();
                sTail = Encoding.Unicode.GetString(buffer.Skip(buffer.Length - data.Length).ToArray());
                cls_Utility.Log("HEREEE17");
            }
            catch
            {
                throw new Exception("! Failed to prepend the file.");
            }            
            if (!sTail.Contains("_gr") && !isRecover)
            {
                try
                {
                    cls_Utility.Log("HEREEE18");
                    file.SetLength(buffer.Length + data.Length);
                    cls_Utility.Log("HEREEE19");
                    //file.Write(data, 0, data.Length);
                    file.Write(firstChunk, 0, firstChunk.Length);
                    cls_Utility.Log("HEREEE30");
                    //file.Write(buffer.Reverse().ToArray(), 0, data.Length);
                    file.Write(secondChunk.Reverse().ToArray(), 0, secondChunk.Length);
                    cls_Utility.Log("HEREEE31");
                    file.Write(data, 0, data.Length);
                    cls_Utility.Log("HEREEE32");
                }
                catch
                {
                    throw new Exception("! Failed to encode the file.");
                }
            }
            if(sTail.Contains("_gr") && isRecover)
            {
                try
                {
                    cls_Utility.Log("HEREEE20 __" + firstChunk.Length.ToString() + "__" + secondChunk.Length.ToString() + "__" + data.Length.ToString());
                    file.SetLength(firstChunk.Length + secondChunk.Length - data.Length);
                    //file.Write(data, 0, data.Length);
                    //file.Write(buffer.Skip(data.Length).ToArray(), 0, buffer.Length - data.Length);
                    cls_Utility.Log("HEREEE21");
                    file.Write(firstChunk.ToArray(), 0, firstChunk.Length);
                    cls_Utility.Log("HEREEE22");
                    file.Write(secondChunk.Take(secondChunk.Length - data.Length).Reverse().ToArray(), 0, secondChunk.Length - data.Length);
                    cls_Utility.Log("HEREEE23");
                }
                catch
                {
                    cls_Utility.Log("! Failed to decode the file.");
                    throw new Exception("! Failed to decode the file.");
                }
            }
        }

        public static void Prepend(FileStream file, string value, bool isRecover)
        {
            cls_Utility.Log("HEREEE12");
            try
            {
                PrependString(value, file, isRecover);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
