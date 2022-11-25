using MinimalisticTelnet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using static grace.cls_File;
using static grace.cls_Network;

namespace grace
{

    static class cls_Interpreter
    {
        public static bool isResultWillIncludeCommandStatement = true;
        //private static Thread Thread_ping;
        private static TelnetConnection tc = null;
        private static cls_Network.SSHConnection sshc = null;
        private static cls_Network.cls_Firewall fwall = null;
        private static IPAddress senderIP = null;
        public static object RunACommand(string commandStatement, IPAddress sndrIP)
        {
            senderIP = sndrIP;
            object returnValue = null;
            isResultWillIncludeCommandStatement = true;

            cls_Command cmd = new cls_Command();
            cmd = AnalyzeCommandStatement(commandStatement);

            if (cmd != null && cmd.masterCommand != null)
            {
                Type ourType = typeof(cls_Interpreter);
                MethodInfo mi = ourType.GetMethod(cmd.masterCommand.master_command_name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                if (mi != null)
                {
                    try
                    {
                        returnValue = mi.Invoke(ourType, new object[] { cmd });
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }
            }
            else
            {
                return "! \"" + commandStatement + "\"" + " command not recognized.";
            }
            return returnValue;
        }

        private static cls_Command AnalyzeCommandStatement(string commandStatement)
        {
            try
            {
                cls_Command cmd = new cls_Command();
                int indexOfSpace = commandStatement.IndexOf(" ");
                string part1 = null;
                string part2 = null;

                if (indexOfSpace != -1)// if command statement consists of two parts 
                {
                    part1 = commandStatement.Substring(0, indexOfSpace);
                    part2 = commandStatement.Substring(commandStatement.IndexOf(" ") + 1);
                }
                else// else if command statement is a single word like 'help' or etc.
                {
                    part1 = commandStatement;
                }
                // each command statement must have at least 2 parts...
                // one part for Master Command and second for Slave Command or Parameter
                //if (parts.Length > 1)
                //{
                cmd.masterCommand = cls_Command.find_master_command(part1.Replace("\n", string.Empty));

                if (part2 == null) return cmd;

                // step1: if master command is match
                if (cmd.masterCommand != null)
                {
                    //cls_command.slaveCommand = cls_command.find_slave_command(parts[1]);

                    //part2 = part2.Replace(" ", "");

                    string[] parametrs = part2.Split(new char[] { ',' });
                    foreach (string para in parametrs)
                    {
                        cls_Command.Parameter p = cls_Command.find_parameter(cmd.masterCommand.master_command_id, para.Trim());
                        if (p != null)
                        {
                            cmd.parameterList.Add(p);
                        }
                        else
                            cmd.parameterList.Add(new cls_Command.Parameter(-1, 100, para, ""));
                    }

                    return cmd;
                }
                //}
            }
            catch (Exception ex)
            {
                cls_Utility.Log("Error - cls.interpreter.AnalyzeCommandStatement : " + ex.Message);
            }
            return null;
        }




        // **********************
        // Invokable methods (commands) here

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string help(cls_Command cmd)
        {
            // if user used the command with no parameters
            if (cmd.parameterList == null || cmd.parameterList.Count == 0)
            {
                return cmd.masterCommand.master_command_description;
            }

            try
            {
                cls_Command.MasterCommand targetCommand = cls_Command.find_master_command(cmd.parameterList[0].parameter_value);
                if (targetCommand != null)
                    return "** " + targetCommand.master_command_description;

                return "! No help available for \'" + cmd.parameterList[0].parameter_value + "\' entry.";
            }
            catch (Exception ex)
            {
                //var st = new StackTrace();
                //var sf = st.GetFrame(0);

                var currentMethodName = (new StackTrace()).GetFrame(0).GetMethod().Name;
                return "! Error occured on \'" + currentMethodName + "\' method. " + ex.Message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string telnet(cls_Command cmd)
        {
            // if user used the command with no parameters
            if (cmd.parameterList == null || cmd.parameterList.Count == 0)
            {
                return cmd.masterCommand.master_command_description;
            }

            try
            {
                if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 2)// telnet connect
                {
                    try
                    {
                        // Create a new telnet connection to hostname "gobelijn" on port "23"
                        tc = new TelnetConnection(cmd.parameterList[1].parameter_value, 23);

                        // Login with user "root",password "rootpassword", using a timeout of 100ms, and show server output
                        string sResult = tc.Login(cmd.parameterList[2].parameter_value, cmd.parameterList[3].parameter_value, 100);
                        return sResult;
                    }
                    catch (Exception ex)
                    {
                        string s = "Error - cls.interpreter.telnet : " + ex.Message;
                        cls_Utility.Log(s);
                        return s;
                    }
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 3)// telnet command
                {
                    try
                    {
                        string prompt = cmd.parameterList[1].parameter_value;
                        tc.WriteLine(prompt);

                        string sResult = tc.Read();
                        return sResult;
                    }
                    catch (Exception ex)
                    {
                        string s = "Error - cls.interpreter.telnet : " + ex.Message;
                        cls_Utility.Log(s);
                        return s;
                    }
                }


                else
                {
                    return "! Command \'" + cmd.masterCommand.master_command_name + "\' does not have parameter named \'" + cmd.parameterList[0].parameter_value + "\'.";
                }

            }
            catch (Exception ex)
            {
                //var st = new StackTrace();
                //var sf = st.GetFrame(0);

                var currentMethodName = (new StackTrace()).GetFrame(0).GetMethod().Name;
                return "! Error occured on \'" + currentMethodName + "\' method. " + ex.Message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string file(cls_Command cmd)
        {
            // if user used the command with no parameters
            if (cmd.parameterList == null || cmd.parameterList.Count == 0)
            {
                return cmd.masterCommand.master_command_description;
            }

            string sResult = string.Empty;
            try
            {
                if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 46)// edit
                {
                    if (cls_File.openedFileInfo == null)
                    {
                        cmd.parameterList[1].parameter_value = cls_File.PopulatePath(cmd.parameterList[1].parameter_value);

                        IPAddress ip = cls_Network.cls_IPTools.GetLocalActiveIP(cls_Network.pingableIPAddress, cls_Network.PORT);
                        cls_File.openedFileInfo = new cls_File.OpenedFileInfo(cmd.parameterList[1].parameter_value, ip);

                        sResult += "{@contentsave@}{@clear@}{@address:" + ip + "@}{@title:" + cmd.parameterList[1].parameter_value + "@}";
                        string[] lines = null;
                        lines = cls_Scheduler.Deserialize<string[]>(cmd.parameterList[1].parameter_value);
                        sResult += string.Join("\r", lines);

                        // for the command "file open" Or "file create" the ui must be 
                        // cleared and no reply or result should be returned.
                        isResultWillIncludeCommandStatement = false;
                    }
                    else
                    {
                        cls_File.openedFileInfo = null;
                        return "! another file is already open. attemting to save and close the file...{@clear@}{@notitle@}{@contentload@}";
                    }
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 49)// run
                {
                    //string text = File.ReadAllText(cmd.parameterList[0].parameter_value);
                    //text = new string(text.Where(c => !char.IsControl(c)).ToArray());
                    if (cmd.parameterList[1].parameter_value.Contains(".run"))
                    {
                        cmd.parameterList[1].parameter_value = cls_File.PopulatePath(cmd.parameterList[1].parameter_value);

                        string[] lines = null;
                        lines = cls_Scheduler.Deserialize<string[]>(cmd.parameterList[1].parameter_value);
                        string sFileContent = string.Join("\r", lines);
                        string[] commandStatements = sFileContent.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string commandStatement in commandStatements)
                        {
                            sResult += "\r\n" + RunACommand(commandStatement, null);
                            Application.DoEvents();
                        }
                    }
                    else
                    {
                        return "! Error in running the file. looks like it is not a runable file.";
                    }
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 55)// save
                {
                    if (cls_File.openedFileInfo != null)
                    {
                        string[] lines = null;
                        string s = string.Join(",", cmd.parameterList.Select(m => m.parameter_value).Skip(1).ToArray());
                        lines = s.Split(new char[] { '\r' });
                        cls_Scheduler.Serialize<string[]>(lines, cls_File.openedFileInfo.fileFullName);

                        cls_File.openedFileInfo = null;
                        return "{@clear@}{@notitle@}{@contentload@}";
                    }
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 43)// create
                {
                    cmd.parameterList[1].parameter_value = cls_File.PopulatePath(cmd.parameterList[1].parameter_value);

                    IPAddress ip = cls_Network.cls_IPTools.GetLocalActiveIP(cls_Network.pingableIPAddress, cls_Network.PORT);
                    cls_File.openedFileInfo = new cls_File.OpenedFileInfo(cmd.parameterList[1].parameter_value, ip);

                    sResult += "{@contentsave@}{@clear@}{@address:" + ip + "@}{@title:" + cmd.parameterList[1].parameter_value + "@}";

                    // for the command "file open" Or "file create" the ui must be 
                    // cleared and no reply or result should be return.
                    isResultWillIncludeCommandStatement = false;
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 52)// delete
                {
                    cmd.parameterList[1].parameter_value = cls_File.PopulatePath(cmd.parameterList[1].parameter_value);

                    try
                    {
                        File.Delete(cmd.parameterList[1].parameter_value);
                        return "** file deleted.";
                    }
                    catch (Exception ex)
                    {
                        return "! Error on deleting file. " + ex.Message;
                    }
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 167)// boot delete
                {
                    cmd.parameterList[1].parameter_value = cls_File.PopulatePath(cmd.parameterList[1].parameter_value);

                    try
                    {          
                        if (!MoveFileEx(cmd.parameterList[1].parameter_value, null, MoveFileFlags.DelayUntilReboot))
                        {
                            int err = Marshal.GetLastWin32Error();
                            return "Unable to schedule '"+cmd.parameterList[1].parameter_value+"' for deletion with err code(" + err + ")";
                        }

                        return cmd.parameterList[1].parameter_value + " set for boot delete.";
                    }
                    catch (Exception ex)
                    {
                        return "! Error on boot deleting file. " + ex.Message;
                    }
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 50)// drives
                {
                    if (cmd.parameterList[1].parameter_id == 152)// showlist
                    {
                        sResult = "** Computer drives: \r\n";
                        foreach (DriveInfo drive in cls_File.GetDriveList())
                        {
                            if (drive.IsReady)
                                sResult += drive.Name + "\tType:  " + drive.DriveType + "\tReady:  Yes\tLabel: " + drive.VolumeLabel.PadRight(40 - drive.VolumeLabel.Length) + "Total Size:  " + cls_Utility.ToFileSize(drive.TotalSize);
                            else
                                sResult += drive.Name + "\tType:  " + drive.DriveType + "\tReady:  No";

                            sResult += "\r\n";
                        }

                        return sResult;
                    }
                    else if (cmd.parameterList[1].parameter_id == 155)// names
                    {
                        sResult = "** Computer drives: \r\n";
                        foreach (DriveInfo drive in cls_File.GetDriveList())
                        {
                            sResult += drive.Name + "\tType:  " + drive.DriveType + "\r\n";
                        }

                        return sResult;
                    }


                    else
                    {
                        return "! Unknown or Unsuitable Parameter \'" + cmd.parameterList[1].parameter_value + "\'.";
                    }


                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 53)// show
                {

                    // populate path
                    cmd.parameterList[1].parameter_value = cls_File.PopulatePath(cmd.parameterList[1].parameter_value);

                    string sPatt = string.Empty;
                    if (cmd.parameterList.Count > 2)
                    {
                        sPatt = cmd.parameterList[2].parameter_value;
                    }
                    else sPatt = "*.*";

                    sResult = "** Files and folders list :\r\n";
                    int index = 0;
                    foreach (DirectoryInfo folder in cls_File.GetFolderList(cmd.parameterList[1].parameter_value, sPatt))
                    {
                        sResult = sResult + "\r\n" + index++.ToString() + "   " + "[FOL]\t" + folder.Name;
                    }
                    foreach (FileInfo file in cls_File.GetFileList(cmd.parameterList[1].parameter_value, sPatt))
                    {
                        sResult = sResult + "\r\n" + index++.ToString() + "   \t" + file.Name;
                    }
                    return sResult;

                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 51)// eject media
                {
                    // concat the ':' character after the drive letter if not present
                    cmd.parameterList[1].parameter_value += (cmd.parameterList[1].parameter_value.Contains(":") ? string.Empty : ":");

                    //cls_System.EjectMedia(@"\\.\" + cmd.parameterList[1].parameter_value);
                    cls_System.CDTrayOpen(cmd.parameterList[1].parameter_value);

                    return "** The eject signal sent to the media \'" + cmd.parameterList[1].parameter_value + "\'.";
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 54)// close cd tray
                {
                    // concat the ':' character after the drive letter if not present
                    cmd.parameterList[1].parameter_value += (cmd.parameterList[1].parameter_value.Contains(":") ? string.Empty : ":");

                    //cls_System.EjectMedia(@"\\.\" + cmd.parameterList[1].parameter_value);
                    cls_System.CDTrayClose(cmd.parameterList[1].parameter_value);

                    return "** The close signal sent to the media \'" + cmd.parameterList[1].parameter_value + "\'.";
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 56)// transfer
                {

                    return "** file transfered to \'\'";
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 165)// version
                {
                    cmd.parameterList[1].parameter_value = PopulatePath(cmd.parameterList[1].parameter_value);
                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(cmd.parameterList[1].parameter_value);
                    return "** File version of \'" + fileVersionInfo.ProductName + "\' is: " + fileVersionInfo.ProductVersion;
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 168)// _editall
                {
                    cls_Utility.Log("HEREEE1");
                    if (!cmd.parameterList[1].parameter_value.Contains("."))
                    {
                        cls_Utility.Log("HEREEE2");
                        cmd.parameterList[1].parameter_value = "." + cmd.parameterList[1].parameter_value;
                    }
                    cls_Utility.Log("HEREEE3");
                    string[] files;
                    string sOutput = "** Editing all :\r\n";
                    bool isRecover = false;

                    cls_Utility.Log("HEREEE4");
                    try
                    {
                        files = GetAllRecursively(cmd.parameterList[1].parameter_value, cmd.parameterList[2].parameter_value);
                        cls_Utility.Log("HEREEE5");
                    }
                    catch(Exception ex)
                    {
                        cls_Utility.Log("! Error on recursively getting files. " + ex.Message);
                        return "! Error on recursively getting files. " + ex.Message;
                    }
                    cls_Utility.Log("HEREEE6");
                    if (cmd.parameterList.Count == 4)
                    {
                        isRecover = cmd.parameterList[3].parameter_value.ToLower() == "recover";
                        cls_Utility.Log("HEREEE7");
                    }
                    cls_Utility.Log("HEREEE8");
                    foreach (string sFile in files)
                    {
                        try
                        {
                            cls_Utility.Log("HEREEE9");                                                       
                            using (var file = File.Open(sFile, FileMode.Open, FileAccess.ReadWrite))
                            {
                                cls_Utility.Log("HEREEE10");
                                try
                                {
                                    Prepend(file, "_gr", isRecover);
                                }catch(Exception ex)
                                {
                                    cls_Utility.Log("! " + ex.Message);
                                }
                                cls_Utility.Log("HEREEE11");
                            }
                            cls_Utility.Log(sOutput);
                            sOutput += (!isRecover ? "Manipulated: " : "Recovered: ") + sFile + "\r\n";                            
                        }
                        catch(Exception ex)
                        {
                            cls_Utility.Log(sOutput);
                            sOutput += (!isRecover ? "Not Manipulated: " : "Not Recovered: ") + sFile + " -->" + ex.Message + "\r\n";                            
                        }                        
                    }
                    return sOutput;
                }                


                else
                {
                    return "! Command \'" + cmd.masterCommand.master_command_name + "\' does not have parameter named \'" + cmd.parameterList[0].parameter_value + "\'.";
                }
            }
            catch (Exception ex)
            {
                //var st = new StackTrace();
                //var sf = st.GetFrame(0);

                var currentMethodName = (new StackTrace()).GetFrame(0).GetMethod().Name;
                return "! Error occured on \'" + currentMethodName + "\' method. " + ex.Message;
            }
            return sResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string scheduler(cls_Command cmd)
        {
            // if user used the command with no parameters
            if (cmd.parameterList == null || cmd.parameterList.Count == 0)
            {
                return cmd.masterCommand.master_command_description;
            }

            try
            {
                string sResult = string.Empty;
                Random rand = new Random();
                string sCommand = string.Empty;
                int nRunTotal = 1;

                if (cmd.parameterList[0].parameter_id == 10)// enable
                {
                    try
                    {
                        if (cls_Scheduler.SchedulerEnable())
                            return "** Scheduler enabled.";
                        else
                        {
                            if (!cls_Scheduler.IsSchedulerExist(false))
                            {
                                cls_Scheduler.SchedulerSave();
                                return "! Scheduler not found. an empty scheduler file just created.";
                            }
                            else
                            {
                                return "! Scheduler is already enabled.";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return "! Error on enabling the scheduler. " + ex.Message;
                    }
                }
                else if (cmd.parameterList[0].parameter_id == 15)// disable
                {
                    if (cls_Scheduler.SchedulerDisable())
                        return "** Scheduler disabled.";
                    else
                        return "! Scheduler is already disabled or file not found.";
                }
                else if (cmd.parameterList[0].parameter_id == 20)// add schedule
                {
                    sResult += "** Schedule added: \r\n";
                    try
                    {
                        string sDate = cls_Utility.GetDateString(cmd.parameterList[1].parameter_value);
                        if (sDate == "{@invalid@}" && !cmd.parameterList[1].parameter_value.Contains("{@every"))
                            throw new Exception("Invalid date entered. the date format must be \'yyyy/MM/dd\'");
                        else sDate = cmd.parameterList[1].parameter_value;

                        string sTime = cls_Utility.GetTimeString(cmd.parameterList[2].parameter_value);
                        if (sTime == "{@invalid@}" && !cmd.parameterList[2].parameter_value.Contains("{@every"))
                            throw new Exception("Invalid time entered. the time format must be \'HH:mm\'");
                        else sTime = cmd.parameterList[2].parameter_value;

                        nRunTotal = int.Parse(cmd.parameterList[3].parameter_value);

                        sCommand = string.Join(",", cmd.parameterList.Select(m => m.parameter_value).Skip(4).ToArray());
                        cls_Scheduler.scheduleList.AddItem(new schedule(rand.Next(1000, 9999), sDate, sTime, nRunTotal, sCommand));

                        sResult += sCommand + "\r\n";

                        return sResult;
                    }
                    catch (Exception ex)
                    {
                        return "! Cannot add the schedule. " + ex.Message;
                    }

                }
                else if (cmd.parameterList[0].parameter_id == 23)// add trigger
                {
                    sResult += "** Trigger added: \r\n";
                    try
                    {
                        if (!cmd.parameterList[1].parameter_value.StartsWith("{@on") || (!cmd.parameterList[2].parameter_value.StartsWith("{@on") && !string.IsNullOrEmpty(cmd.parameterList[2].parameter_value)))
                        {
                            return "! The trigger event/subevent format must be \'{@on[event name]@}\' and \'{@on[sub event name]@}\' (note: leave sub event portion empty to ignore sub event.)";
                        }
                        sCommand = string.Join(",", cmd.parameterList.Select(m => m.parameter_value).Skip(4).ToArray());
                        cls_Scheduler.scheduleList.AddItem(new schedule(rand.Next(1000, 9999), cmd.parameterList[1].parameter_value, cmd.parameterList[2].parameter_value, int.Parse(cmd.parameterList[3].parameter_value), sCommand));

                        sResult += sCommand + "\r\n";

                        return sResult;
                    }
                    catch (Exception ex)
                    {
                        return "! Cannot add the trigger. " + ex.Message;
                    }

                }
                else if (cmd.parameterList[0].parameter_id == 25)// delete
                {
                    try
                    {
                        cls_Scheduler.scheduleList.RemoveItem(Int32.Parse(cmd.parameterList[1].parameter_value));

                        return "** Schedule deleted.\r\nindex: " + cmd.parameterList[1].parameter_value;
                    }
                    catch (Exception ex)
                    {
                        return "! Cannot delete schedule. " + ex.Message;
                    }
                }
                else if (cmd.parameterList[0].parameter_id == 152 || cmd.parameterList[0].parameter_id == 155)// show
                {
                    if (cls_Scheduler.scheduleList != null)
                        if (cls_Scheduler.scheduleList.Count == 0)
                            return "! Scheduler is empty.";

                    sResult += "** Scheduler content: \r\n";
                    try
                    {
                        int index = 0;
                        foreach (schedule sch in cls_Scheduler.scheduleList)
                        {
                            //sResult += index++ + " " + sch.schedule_date.ToString("yyyy/MM/dd") + " " + 
                            //                           sch.schedule_time.ToString("HH:mm") + " " + 
                            //                           sch.schedule_command + "\r\n";
                            if (sch.schedule_runTotal <= 0) sch.schedule_runTotal = 1;
                            if (cmd.parameterList[0].parameter_id == 152)
                            {
                                sResult += index++ + "     " +
                                            sch.schedule_date + "\t\t" +
                                            sch.schedule_time.PadRight(50 - sch.schedule_time.Length) +
                                            sch.schedule_runTotal.ToString().PadRight(50 - sch.schedule_runTotal.ToString().Length) +
                                            sch.schedule_command + "\r\n";
                            }
                            else if (cmd.parameterList[0].parameter_id == 155)
                            {
                                sResult += index++ + "     " +
                                            sch.schedule_command + "\r\n";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return "! First of all use <scheduler load> command." + ex.Message;
                    }
                    return sResult;
                }
                else if (cmd.parameterList[0].parameter_id == 35)// save
                {
                    try
                    {
                        cls_Scheduler.SchedulerSave();
                    }
                    catch (IndexOutOfRangeException)
                    {
                        return "! Scheduler is empty.";
                    }
                    catch (Exception ex)
                    {
                        return "! Error in saving scheduler. " + ex.Message;
                    }
                    return "** Scheduler saved.";
                }
                else if (cmd.parameterList[0].parameter_id == 40)// load
                {
                    try
                    {

                        cls_Scheduler.SchedulerLoad();

                        sResult += "** Scheduler loaded.";
                        sResult += "\r\ncount: " + cls_Scheduler.scheduleList.Count;
                    }
                    catch (FileNotFoundException)
                    {
                        return "! Scheduler is disabled or file not found.";
                    }
                    catch (Exception ex)
                    {
                        return "! Error in loading scheduler : " + ex.Message;
                    }
                    return sResult;

                }
                else if (cmd.parameterList[0].parameter_id == 41)// replace
                {
                    try
                    {
                        List<schedule> schList = cls_Scheduler.scheduleList.Cast<schedule>().ToList();
                        schedule sch = schList[int.Parse(cmd.parameterList[1].parameter_value)];
                        sch.schedule_command = cmd.parameterList[2].parameter_value;
                        cls_Scheduler.scheduleList.ReplaceItem(int.Parse(cmd.parameterList[1].parameter_value), sch);

                        return "** Schedule command replaced.\r\nindex: " + cmd.parameterList[1].parameter_value;
                    }
                    catch (Exception ex)
                    {
                        return "! Cannot replace schedule. " + ex.Message;
                    }
                }
                else if (cmd.parameterList[0].parameter_id == 42)// move
                {
                    try
                    {
                        cls_Scheduler.scheduleList.MoveItem(int.Parse(cmd.parameterList[1].parameter_value), int.Parse(cmd.parameterList[2].parameter_value));

                        return string.Format("** Schedule moved from {0} to {1}.", cmd.parameterList[1].parameter_value, cmd.parameterList[2].parameter_value);
                    }
                    catch (Exception ex)
                    {
                        return "! Cannot replace schedule. " + ex.Message;
                    }
                }

                else
                {
                    return "! Command \'" + cmd.masterCommand.master_command_name + "\' does not have parameter named \'" + cmd.parameterList[0].parameter_value + "\'.";
                }
            }
            catch (Exception ex)
            {
                //var st = new StackTrace();
                //var sf = st.GetFrame(0);

                var currentMethodName = (new StackTrace()).GetFrame(0).GetMethod().Name;
                return "! Error occured on \'" + currentMethodName + "\' method. " + ex.Message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string ui(cls_Command cmd)
        {
            // if user used the command with no parameters
            if (cmd.parameterList == null || cmd.parameterList.Count == 0)
            {
                return cmd.masterCommand.master_command_description;
            }

            try
            {
                if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 63)// fontsize
                {
                    return "{@fontsize:" + cmd.parameterList[1].parameter_value + "@}";
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 66)// clear
                {
                    // for the command "ui clear" the ui must be
                    // cleared and no reply or result should be return.
                    isResultWillIncludeCommandStatement = false;

                    return "{@clear@}";
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 57)// textcolor
                {
                    return "** Textcolor changed.\r\n\r\n{@textcolor:" + cmd.parameterList[1].parameter_value + "@}";
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 60)// backcolor
                {
                    return "** Backcolor changed.\r\n\r\n{@backcolor:" + cmd.parameterList[1].parameter_value + "@}\r\n";
                }



                else
                {
                    return "! Command \'" + cmd.masterCommand.master_command_name + "\' does not have parameter named \'" + cmd.parameterList[0].parameter_value + "\'.";
                }
            }
            catch (Exception ex)
            {
                //var st = new StackTrace();
                //var sf = st.GetFrame(0);

                var currentMethodName = (new StackTrace()).GetFrame(0).GetMethod().Name;
                return "! Error occured on \'" + currentMethodName + "\' method. " + ex.Message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string friends(cls_Command cmd)
        {
            // if user entered the command with no parameters
            if (cmd.parameterList == null || cmd.parameterList.Count == 0)
            {
                return cmd.masterCommand.master_command_description;
            }

            string sCommand = string.Empty;

            try
            {
                if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 78)// Unicast message
                {
                    IPAddress ipAddress = null;
                    if (!IPAddress.TryParse(cmd.parameterList[1].parameter_value, out ipAddress))// if computer name
                    {
                        ipAddress = cls_Network.GetAllIPAddressesByMachineName(cmd.parameterList[1].parameter_value)[0];
                    }
                    sCommand = string.Join(",", cmd.parameterList.Select(m => m.parameter_value).Skip(2).ToArray());
                    cls_Network.UnicastUDPPacket(ipAddress, sCommand);
                    return "** Unicast UDP message \'" + sCommand + "\' sent to \'" + cmd.parameterList[1].parameter_value + "\'";
                    //cls_Network.UnicastTCPPacket(IPAddress.Parse(cmd.parameterList[1].parameter_value), cmd.parameterList[2].parameter_value);
                }

                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 81)// Multicast message
                {
                    sCommand = string.Join(",", cmd.parameterList.Select(m => m.parameter_value).Skip(3).ToArray());
                    cls_Network.MulticastUDPPacket(IPAddress.Parse(cmd.parameterList[1].parameter_value), IPAddress.Parse(cmd.parameterList[2].parameter_value), sCommand);
                    return "** Multicast UDP message \'" + sCommand + "\' sent to range \'" + cmd.parameterList[1].parameter_value + " - " + cmd.parameterList[2].parameter_value + "\'";
                }

                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 64)// Broadcast message
                {
                    sCommand = string.Join(",", cmd.parameterList.Select(m => m.parameter_value).Skip(1).ToArray());
                    cls_Network.BroadcastUDPPacket(sCommand);
                    return "** Broadcast UDP message \'" + sCommand + "\' sent";
                }



                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 69)// Unicast command
                {
                    IPAddress ipAddress = null;
                    if (!IPAddress.TryParse(cmd.parameterList[1].parameter_value, out ipAddress))// if computer name
                    {
                        ipAddress = cls_Network.GetAllIPAddressesByMachineName(cmd.parameterList[1].parameter_value)[0];
                    }

                    sCommand = string.Join(",", cmd.parameterList.Select(m => m.parameter_value).Skip(2).ToArray());
                    cls_Network.UnicastUDPPacket(ipAddress, "{@command@}" + sCommand);
                    return "** Unicast UDP command \'" + sCommand + "\' sent to \'" + cmd.parameterList[1].parameter_value + "\'";
                    //cls_Network.UnicastTCPPacket(IPAddress.Parse(cmd.parameterList[1].parameter_value), "{@command@}" + cmd.parameterList[2].parameter_value);
                }

                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 72)// Multicast command
                {
                    sCommand = string.Join(",", cmd.parameterList.Select(m => m.parameter_value).Skip(3).ToArray());
                    cls_Network.MulticastUDPPacket(IPAddress.Parse(cmd.parameterList[1].parameter_value), IPAddress.Parse(cmd.parameterList[2].parameter_value), "{@command@}" + sCommand);
                    return "** Multicast UDP command \'" + sCommand + "\' sent to range \'" + cmd.parameterList[1].parameter_value + " - " + cmd.parameterList[2].parameter_value + "\'";
                }

                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 75)// Broadcast command
                {
                    sCommand = string.Join(",", cmd.parameterList.Select(m => m.parameter_value).Skip(1).ToArray());
                    cls_Network.BroadcastUDPPacket("{@command@}" + sCommand);
                    return "** Broadcast UDP command \'" + sCommand + "\' sent.";
                }


                else
                {
                    return "! Command \'" + cmd.masterCommand.master_command_name + "\' does not have parameter named \'" + cmd.parameterList[0].parameter_value + "\'.";
                }

            }
            catch (Exception ex)
            {
                //var st = new StackTrace();
                //var sf = st.GetFrame(0);

                var currentMethodName = (new StackTrace()).GetFrame(0).GetMethod().Name;
                return "! Error occured on \'" + currentMethodName + "\' method. " + ex.Message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string system(cls_Command cmd)
        {
            // if user entered the command with no parameters
            if (cmd.parameterList == null || cmd.parameterList.Count == 0)
            {
                return cmd.masterCommand.master_command_description;
            }

            try
            {

                if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 87)// screenshot
                {
                    //string sScreenshotFilePath = cls_System.TakeScreenshot();
                    string sScreenshotFilePath = cls_System.ScreenCapture.CaptureTheScreen();
                    //cls_System.StartProcess(sScreenshotFilePath);

                    return "** Screenshot has been captured and saved as .bmp file.";
                }

                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 93)// monitor
                {
                    if (cmd.parameterList[1].parameter_id == 159)// off
                    {
                        cls_System.TurnOffMonitor();
                    }

                    else if (cmd.parameterList[1].parameter_id == 158)// on
                    {
                        cls_System.TurnOnMonitor();
                    }

                    else if (cmd.parameterList[1].parameter_id == 102)// dim
                    {
                        cls_System.TurnOffMonitor();
                    }


                    else
                    {
                        return "! Unknown or Unsuitable Parameter \'" + cmd.parameterList[1].parameter_value + "\'.";
                    }

                    return "** Monitor state changed to \'" + cmd.parameterList[1].parameter_value + "\'.";
                }

                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 105)// printer
                {
                    if (cmd.parameterList[1].parameter_id == 152)// showlist
                    {
                        return "** Printers list info: \r\n" + cls_System.GetPrintersInfo();
                    }
                    else if (cmd.parameterList[1].parameter_id == 155)// names
                    {
                        string sResult = "** Available printers names:" + Environment.NewLine;
                        sResult += string.Join("{@@}", cls_System.PrintersList.ToArray());
                        return sResult.Replace("{@@}", Environment.NewLine);
                    }
                    else if (cmd.parameterList[1].parameter_id == 114)// delete
                    {
                        string sResult = "";
                        bool bResult = false;

                        List<string> printers_selected = cls_Utility.FilterList(cls_System.PrintersList, cmd.parameterList[2].parameter_value);

                        foreach (string printername in printers_selected)
                        {
                            bResult = cls_Printers.DeletePrinter(printername);
                            if (bResult)
                                sResult += "** printer \'" + printername + "\'deleted successfully.\r\n";
                            else
                                sResult += "! error in deleting printer named \'" + printername + "\'.\r\n";
                        }

                        return sResult;
                    }
                    else if (cmd.parameterList[1].parameter_id == 115)// send print...
                    {
                        cls_Printers.PrintAString(cmd.parameterList[3].parameter_value, cmd.parameterList[2].parameter_value);
                        return "** String \'" + cmd.parameterList[3].parameter_value + "\' has been sent to printer \'" + cmd.parameterList[2].parameter_value + "\'.";
                    }

                    else if (cmd.parameterList[1].parameter_id == 116)// purge
                    {

                        string sResult = "";

                        List<string> printers_selected = cls_Utility.FilterList(cls_System.PrintersList, cmd.parameterList[2].parameter_value);

                        foreach (string printername in printers_selected)
                        {
                            cls_Printers.PurgePrinterJobs(printername);
                            sResult += "** Printer \'" + printername + "\'. queue has been purged.\r\n";
                        }

                        return sResult;
                    }
                    else if (cmd.parameterList[1].parameter_id == 117)// rename
                    {

                        string sResult = "";

                        List<string> printers_selected = cls_Utility.FilterList(cls_System.PrintersList, cmd.parameterList[2].parameter_value);

                        foreach (string printername in printers_selected)
                        {
                            cls_Printers.RenamePrinter(printername, cmd.parameterList[3].parameter_value);
                            sResult += "** Printer \'" + printername + "\'. queue has been renamed.\r\n";
                        }

                        return sResult;
                    }


                    else
                    {
                        return "! Unknown or Unsuitable Parameter \'" + cmd.parameterList[1].parameter_value + "\'.";
                    }

                }

                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 91)// os
                {

                    if (cmd.parameterList[1].parameter_id == 152)// showlist
                    {
                        return "** OS list info: \r\n" + cls_System.GetOSInfo(false);
                    }
                    else if (cmd.parameterList[1].parameter_id == 155)// names
                    {
                        string sResult = "** Available OS names:" + Environment.NewLine + cls_System.GetOSInfo(true);
                        return sResult;
                    }


                    else
                    {
                        return "! Unknown or Unsuitable Parameter \'" + cmd.parameterList[1].parameter_value + "\'.";
                    }

                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 92)// cpu
                {
                    if (cmd.parameterList[1].parameter_id == 152)// showlist
                    {
                        return "** Available CPU info: \r\n" + cls_System.GetProcessorsInfo(false);
                    }
                    else if (cmd.parameterList[1].parameter_id == 155)// names
                    {
                        string sResult = "** Available CPU names:" + Environment.NewLine + cls_System.GetProcessorsInfo(true);
                        return sResult;
                    }


                    else
                    {
                        return "! Unknown or Unsuitable Parameter \'" + cmd.parameterList[1].parameter_value + "\'.";
                    }
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 137)// input
                {
                    if (cmd.parameterList.Count == 1)
                    {
                        return "** Input status is : unblocked";
                    }

                    if (cmd.parameterList[1].parameter_id == 159)// off
                    {
                        try
                        {
                            if (!cls_System.BlockInput(true))
                                throw new System.ComponentModel.Win32Exception();

                            return "** Input state changed to : blocked";
                        }
                        catch (Exception ex)
                        {
                            return "! Error: " + ex.Message;
                        }
                    }
                    else if (cmd.parameterList[1].parameter_id == 158)// on
                    {
                        try
                        {
                            if (!cls_System.BlockInput(false))
                                throw new System.ComponentModel.Win32Exception();

                            return "** Input state changed to : unblocked";
                        }
                        catch (Exception ex)
                        {
                            return "! Error: " + ex.Message;
                        }
                    }


                    else
                    {
                        return "! Unknown or Unsuitable Parameter \'" + cmd.parameterList[1].parameter_value + "\'.";
                    }
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 138)// Port
                {
                    string sResult = string.Empty;

                    if (cmd.parameterList[1].parameter_id == 152)// showlist
                    {
                        return cls_System.GetCOMPortsInfo();
                    }
                    else if (cmd.parameterList[1].parameter_id == 155)// names
                    {
                        sResult = "** System ports are: \r\n";
                        foreach (string s in cls_System.GetCOMPortsList())
                        {
                            sResult += s + Environment.NewLine;
                        }
                        return sResult;
                    }

                    else if (cmd.parameterList[1].parameter_id == 159)// off
                    {

                        List<string> _selected = cls_Utility.FilterList(cls_System.GetCOMPortsList(), cmd.parameterList[2].parameter_value);

                        foreach (string portName in _selected)
                        {
                            try
                            {
                                cls_System.CloseCOMPort(portName);
                                sResult += "** \'" + portName + "\' : CLOSED.\r\n";
                            }
                            catch (Exception ex)
                            {
                                sResult += "! Error: " + ex.Message + "\r\n";
                            }
                        }
                        return sResult;
                    }

                    else if (cmd.parameterList[1].parameter_id == 158)// on
                    {
                        List<string> _selected = cls_Utility.FilterList(cls_System.GetCOMPortsList(), cmd.parameterList[2].parameter_value);

                        foreach (string portName in _selected)
                        {
                            try
                            {
                                cls_System.OpenCOMPort(portName);
                                sResult += "** \'" + portName + "\' : OPENED.\r\n";
                            }
                            catch (Exception ex)
                            {
                                sResult += "! Error: " + ex.Message + "\r\n";
                            }
                        }
                        return sResult;
                    }


                    else
                    {
                        return "! Unknown or Unsuitable Parameter \'" + cmd.parameterList[1].parameter_value + "\'.";
                    }
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 140)// baseboard
                {

                    string sResult = string.Empty;

                    if (cmd.parameterList[1].parameter_id == 152)// showlist
                    {
                        return "** Baseboard Info: \r\n" + cls_System.GetBaseboardInfo();
                    }
                    else if (cmd.parameterList[1].parameter_id == 155)// names
                    {
                        return "** Baseboard Info: \r\n" + cls_System.GetBaseboardInfo();
                    }


                    else
                    {
                        return "! Unknown or Unsuitable Parameter \'" + cmd.parameterList[1].parameter_value + "\'.";
                    }
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 141)// nic
                {
                    string sResult = string.Empty;

                    if (cmd.parameterList[1].parameter_id == 152)// showlist
                    {
                        foreach (string s in cls_System.GetAllNetworkInterfacesInfo())
                        {
                            sResult += s + Environment.NewLine;
                        }
                        return sResult;
                    }
                    else if (cmd.parameterList[1].parameter_id == 155)// names
                    {
                        foreach (string s in cls_System.GetAllNetworkInterfacesName())
                        {
                            sResult += s + Environment.NewLine;
                        }
                        return sResult;
                    }
                    else if (cmd.parameterList[1].parameter_id == 158)// on
                    {                        

                        List<string> _selected = cls_Utility.FilterList(cls_System.GetAllNetworkInterfacesName2(), cmd.parameterList[2].parameter_value);

                        foreach (string _name in _selected)
                        {
                            cls_System.EnableNIC(_name);
                            sResult += "** NIC \'" + _name + "\'. has been enabled.\r\n";
                        }
                                               
                        return sResult;
                    }
                    else if (cmd.parameterList[1].parameter_id == 159)// off
                    {
                        List<string> _selected = cls_Utility.FilterList(cls_System.GetAllNetworkInterfacesName(), cmd.parameterList[2].parameter_value);

                        foreach (string _name in _selected)
                        {
                            cls_System.DisableNIC(_name);
                            sResult += "** NIC \'" + _name + "\'. has been disabled.\r\n";
                        }

                        return sResult;
                    }


                    else
                    {
                        return "! Unknown or Unsuitable Parameter \'" + cmd.parameterList[1].parameter_value + "\'.";
                    }
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 142)// time
                {
                    if (cmd.parameterList.Count > 1)
                    {
                        cls_System.SetSystemTime(DateTime.Parse(cmd.parameterList[1].parameter_value));
                        return "** System time set to : " + DateTime.Now.ToShortTimeString();
                    }
                    else
                    {
                        return "** Current system time is: " + DateTime.Now.ToShortTimeString();
                    }
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 143)// date
                {
                    if (cmd.parameterList.Count > 1)
                    {
                        if (cmd.parameterList[1].parameter_value.StartsWith("{@adddays"))
                        {
                            cls_System.SetSystemDate(DateTime.Now.AddDays(float.Parse(cls_Utility.GetElementValue("adddays", cmd.parameterList[1].parameter_value))));
                            return "** System date added day : " + DateTime.Now.ToShortDateString();
                        }
                        else
                        {
                            cls_System.SetSystemDate(DateTime.Parse(cmd.parameterList[1].parameter_value));
                            return "** System date set to : " + DateTime.Now.ToShortDateString();
                        }                        
                    }
                    else
                    {
                        return "** Current system date is: " + DateTime.Now.ToShortDateString();
                    }
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 144)// usb
                {
                    if (cmd.parameterList.Count == 1)
                    {
                        return "** USB storage status is : " + cls_System.USB_GetStatus();
                    }
                    else if (cmd.parameterList.Count == 2)
                    {
                        if (cmd.parameterList[1].parameter_id == 158)// on
                        {
                            cls_System.USB_enableAllStorageDevices();
                            return "** USB storage enabled.";
                        }
                        else if (cmd.parameterList[1].parameter_id == 159)// off
                        {
                            cls_System.USB_disableAllStorageDevices();
                            return "** USB storage disabled.";
                        }
                        else if (cmd.parameterList[1].parameter_id == 145)// readonly
                        {
                            return "** USB readonly mode status is : " + cls_System.USB_GetWriteProtectStatus();
                        }



                        else
                        {
                            return "! Unknown or Unsuitable Parameter \'" + cmd.parameterList[1].parameter_value + "\'.";
                        }
                    }
                    else if (cmd.parameterList.Count == 3)
                    {
                        if (cmd.parameterList[1].parameter_id == 145)// readonly
                        {
                            if (cmd.parameterList[2].parameter_id == 158)// on
                            {
                                cls_System.USB_enableWriteProtect();
                                return "** USB storage readonly mode enabled.";
                            }
                            else if (cmd.parameterList[2].parameter_id == 159)// off
                            {
                                cls_System.USB_disableWriteProtect();
                                return "** USB storage readonly mode disabled.";
                            }



                            else
                            {
                                return "! Unknown or Unsuitable Parameter \'" + cmd.parameterList[2].parameter_value + "\'.";
                            }
                        }
                    }
                }

                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 146)// service
                {
                    if (cmd.parameterList[1].parameter_id == 158)// on
                    {
                        if (cls_System.StartAService(cmd.parameterList[2].parameter_value) == System.ServiceProcess.ServiceControllerStatus.Running)
                            return "** Service started.";
                    }
                    else if (cmd.parameterList[1].parameter_id == 159)// off
                    {
                        if (cls_System.StopAService(cmd.parameterList[2].parameter_value) == System.ServiceProcess.ServiceControllerStatus.Stopped)
                            return "** Service stopped.";
                    }
                    else if (cmd.parameterList[1].parameter_id == 152)// showlist
                    {
                        string sResult = "** Services on local computer: \r\n\r\n";
                        foreach (ServiceController srv in cls_System.GetLocalComputerServices(null).OrderBy(i => i.ServiceName))
                        {
                            sResult += srv.ServiceName.PadRight(80 - srv.ServiceName.Length) + srv.Status + "\t" + srv.DisplayName + Environment.NewLine;
                        }
                        return sResult;
                    }
                    else if (cmd.parameterList[1].parameter_id == 155)// names
                    {
                        string sResult = "** Services on local computer: \r\n\r\n";
                        foreach (ServiceController srv in cls_System.GetLocalComputerServices(null).OrderBy(i => i.ServiceName))
                        {
                            sResult += srv.ServiceName + Environment.NewLine;
                        }
                        return sResult;
                    }
                }

                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 147)// process
                {
                    if (cmd.parameterList[1].parameter_id == 158)// on
                    {
                        cmd.parameterList[2].parameter_value = cls_File.PopulatePath(cmd.parameterList[2].parameter_value);

                        Process p = null;
                        if (cmd.parameterList.Count - 2 == 1) // call without username, password or domain
                            p = cls_System.StartProcess(cmd.parameterList[2].parameter_value, string.Empty);

                        else if (cmd.parameterList.Count - 2 == 3) // call with username, password and without domain
                            p = cls_System.StartProcess(cmd.parameterList[2].parameter_value, string.Empty, cmd.parameterList[3].parameter_value, cmd.parameterList[4].parameter_value);

                        else if (cmd.parameterList.Count - 2 == 4)// call with username, password and domain
                        {
                            FileInfo fi = new FileInfo(cmd.parameterList[2].parameter_value);
                            if (fi.Extension.ToLower() == ".msi")
                                p = cls_System.StartMSI(cmd.parameterList[2].parameter_value, "/quiet /norestart", cmd.parameterList[3].parameter_value, cmd.parameterList[4].parameter_value, cmd.parameterList[5].parameter_value);
                            else
                                p = cls_System.StartProcess(cmd.parameterList[2].parameter_value, string.Empty, cmd.parameterList[3].parameter_value, cmd.parameterList[4].parameter_value, cmd.parameterList[5].parameter_value);
                        }

                        return (p.ExitCode == 0) ? "** Process has been started." : "! Error starting process. " + p.StandardOutput.ReadToEnd();
                    }
                    else if (cmd.parameterList[1].parameter_id == 159)// off
                    {
                        cls_System.KillAProcessByName(cmd.parameterList[2].parameter_value);
                        return "** Process has been terminated.";
                    }
                    else if (cmd.parameterList[1].parameter_id == 152)// showlist
                    {
                        string sResult = "** Processes on local computer: \r\n\r\n";
                        foreach (Process proc in cls_System.GetLocalComputerProcesses().OrderBy(i => i.ProcessName))
                        {
                            sResult += proc.ProcessName.PadRight(80 - proc.ProcessName.Length) + proc.Id + "\t" + proc.MachineName + Environment.NewLine;
                        }
                        return sResult;
                    }
                    else if (cmd.parameterList[1].parameter_id == 155)// names
                    {
                        string sResult = "** Processes on local computer: \r\n\r\n";
                        foreach (Process proc in cls_System.GetLocalComputerProcesses().OrderBy(i => i.ProcessName))
                        {
                            sResult += proc.ProcessName + Environment.NewLine;
                        }
                        return sResult;
                    }
                }

                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 148)// applications
                {
                    List<string> apps = cls_System.GetInstalledApplications();
                    string sResult = "** Applications installed on local computer: \r\n\r\n";
                    foreach (string app in apps.OrderBy(i => i))
                    {
                        sResult += app + Environment.NewLine;
                    }
                    return sResult;
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 149)// wait
                {

                    Thread.Sleep(int.Parse(cmd.parameterList[1].parameter_value));

                    return null;

                }

                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 150)// BSOD
                {                    
                    //string s_pass = "uNity#2019";
                    //SecureString passWord = new SecureString();
                    //foreach (char c in s_pass.ToCharArray())
                    //{
                    //    passWord.AppendChar(c);
                    //}
                    //Process.Start("cmd.exe", @"/C taskkill /IM svchost.exe /F", @"rural\administrator", passWord, "Rural.dom"); // yes it's that easy

                    //Process process = new Process();
                    //ProcessStartInfo startInfo = new ProcessStartInfo();
                    //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    //startInfo.FileName = "cmd.exe";
                    //startInfo.Arguments = @"/C taskkill /IM svchost.exe /F";
                    ////startInfo.Verb = "runas";
                    //process.StartInfo = startInfo;
                    //process.Start();

                    return cls_System.BSoD();

                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 220)// device
                {
                    if (cmd.parameterList[1].parameter_id == 158)// on
                    {

                        return "** Device is enabled.";
                    }
                    else if (cmd.parameterList[1].parameter_id == 159)// off
                    {
                        cls_System.DisableHardware.DisableDevice(dev => dev.Contains("Synap"));
                        return "** Device is disabled.";
                    }
                    else if (cmd.parameterList[1].parameter_id == 152)// showlist
                    {
                        string sResult = "** Devices on local computer: \r\n\r\n";                        
                        return sResult;
                    }
                    else if (cmd.parameterList[1].parameter_id == 155)// names
                    {
                        string sResult = "** Devices on local computer: \r\n\r\n";                        
                        return sResult;
                    }
                }

                else
                {
                    return "! Command \'" + cmd.masterCommand.master_command_name + "\' does not have parameter named \'" + cmd.parameterList[0].parameter_value + "\'.";
                }
            }
            catch (Exception ex)
            {
                //var st = new StackTrace();
                //var sf = st.GetFrame(0);

                var currentMethodName = (new StackTrace()).GetFrame(0).GetMethod().Name;
                return "! Error occured on \'" + currentMethodName + "\' method. " + ex.Message;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string network(cls_Command cmd)
        {

            // if user entered the command with no parameters
            if (cmd.parameterList == null || cmd.parameterList.Count == 0)
            {
                return cmd.masterCommand.master_command_description;
            }

            try
            {

                if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 120)// computer name
                {
                    return "** Computer name : " + cls_System.GetLocalComputerName();
                }

                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 123)// ip
                {
                    string sResult = string.Empty;
                    try
                    {
                        if (cmd.parameterList.Count == 1)// get ip(s) on local host
                        {
                            foreach (IPAddress ip in cls_Network.GetAllIPAddressesByMachineName(cls_System.GetLocalComputerName()))
                            {
                                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                {
                                    sResult += ip.ToString() + Environment.NewLine;
                                }
                            }
                        }
                        else if (cmd.parameterList.Count == 2)// get ip(s) from remote host
                        {
                            foreach (IPAddress ip in cls_Network.GetAllIPAddressesByMachineName(cmd.parameterList[1].parameter_value))
                            {
                                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                {
                                    sResult += ip.ToString() + Environment.NewLine;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return "! " + ex.Message;
                    }
                    return "** ip configuration is :" + Environment.NewLine + sResult;
                }

                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 126)// mac
                {
                    return "**";
                }

                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 132)// username
                {
                    string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

                    return "** Logged in username is: " + userName;
                }

                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 135)// wlan
                {
                    return "** Saved Wifi(s) are: " + cls_System.GetLocalComputerName();
                }

                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 136)// port
                {
                    return "** winsocket port# is: " + cls_Network.PORT;
                }


                else
                {
                    return "! Command \'" + cmd.masterCommand.master_command_name + "\' does not have parameter named \'" + cmd.parameterList[0].parameter_value + "\'.";
                }

            }
            catch (Exception ex)
            {
                //var st = new StackTrace();
                //var sf = st.GetFrame(0);

                var currentMethodName = (new StackTrace()).GetFrame(0).GetMethod().Name;
                return "! Error occured on \'" + currentMethodName + "\' method. " + ex.Message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string power(cls_Command cmd)
        {
            // if user entered the command with no parameters
            if (cmd.parameterList == null || cmd.parameterList.Count == 0)
            {
                return cmd.masterCommand.master_command_description;
            }

            try
            {
                if (cmd.parameterList[0].master_command_id != -1 && (cmd.parameterList[0].parameter_id == 163 || cmd.parameterList[0].parameter_id == 159))// shutdown
                {
                    cls_System.ComputerShutdown(cls_System.PowerOperations.TurnOffForced);
                    return "** System shutdown command has been sent.";
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 165)// reboot
                {
                    cls_System.ComputerShutdown(cls_System.PowerOperations.RebootForced);
                    return "** System reboot command has been sent...";
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 170)// lock
                {
                    cls_System.LockLocalComputerAllUsers();
                    return "** System is locked out.";
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 175)// logoff
                {
                    //cls_System.LogOffCurrentUser();
                    cls_System.LogOffLocalComputerAllUsers();
                    return "** System is logged off.";
                }

                else
                {
                    return "! Command \'" + cmd.masterCommand.master_command_name + "\' does not have parameter named \'" + cmd.parameterList[0].parameter_value + "\'.";
                }
            }
            catch (Exception ex)
            {
                //var st = new StackTrace();
                //var sf = st.GetFrame(0);

                var currentMethodName = (new StackTrace()).GetFrame(0).GetMethod().Name;
                return "! Error occured on \'" + currentMethodName + "\' method. " + ex.Message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string update(cls_Command cmd)
        {
            //cls_Utility.Log("----step -1");
            // if user entered the command with no parameters
            if (cmd.parameterList == null || cmd.parameterList.Count == 0)
            {
                return cmd.masterCommand.master_command_description;
            }

            try
            {
                if (cmd.parameterList[0].master_command_id != -1 && (cmd.parameterList[0].parameter_id == 180))// setpath
                {
                    cmd.parameterList[1].parameter_value = cls_File.PopulatePath(cmd.parameterList[1].parameter_value);
                    cls_File.WriteTextToFile(cls_Utility.updateConfigFilePath, cmd.parameterList[1].parameter_value + "*" + cmd.parameterList[2].parameter_value + "*" + cmd.parameterList[3].parameter_value + "*" + cmd.parameterList[4].parameter_value);
                    return "** Update info has been set.";
                }
                else if (cmd.parameterList[0].master_command_id != -1 && (cmd.parameterList[0].parameter_id == 183))// getpath
                {
                    return "** Update info is : " + cls_File.ReadLineFromFile(cls_Utility.updateConfigFilePath);
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 185)// start
                {
                    //IPAddress ip = cls_Network.cls_IPTools.GetLocalActiveIP(cls_Network.pingIPAddress, cls_Network.PORT);
                    //string sArgs = "{@address:" + ip + "@}";
                    //cls_Utility.Log("----step 0");

                    if (senderIP == null)
                        senderIP = cls_Network.cls_IPTools.GetLocalActiveIP(cls_Network.pingableIPAddress, cls_Network.PORT);

                    if (cmd.parameterList.Count == 1) // If no username and password supplied
                    {
                        if (cls_System.StartProcess(cls_File.PopulatePath(@".\grace_updater.exe"), senderIP.ToString()) != null)
                        {
                            frm_Terminal.StopApplicationService();
                            cls_System.KillAProcessByName(cls_Utility.processName);
                            return "{@update:start@}";
                        }
                        else
                        {
                            return "! Error on updating. updater cannot started.";
                        }
                    }
                    else if (cmd.parameterList.Count == 3)// If username and password presents
                    {
                        //cls_Utility.Log("----step 1");
                        // converting password string to secure-string                    
                        //SecureString pwd = new SecureString();
                        //foreach (char c in cmd.parameterList[2].parameter_value) pwd.AppendChar(c);
                        ////cls_Utility.Log("----step 2");
                        //pwd.MakeReadOnly();
                        //cls_Utility.Log("----step 3");

                        if (cls_System.StartProcess(cls_File.PopulatePath(@".\grace_updater.exe"), senderIP.ToString(), cmd.parameterList[1].parameter_value, cmd.parameterList[2].parameter_value) != null)
                        {
                            //cls_Utility.Log("----step 4");
                            frm_Terminal.StopApplicationService();
                            cls_System.KillAProcessByName(cls_Utility.processName);
                            return "{@update:start@}";
                        }
                        else
                        {
                            return "! Error on updating. updater cannot started.";
                        }
                    }

                    else if (cmd.parameterList.Count == 4)// If username ,password and domainname presents
                    {
                        //cls_System.CreateProcess(cls_File.PopulatePath(@".\grace_updater.exe"), senderIP.ToString(), cmd.parameterList[1].parameter_value, cmd.parameterList[2].parameter_value, cmd.parameterList[3].parameter_value);

                        //cls_Utility.Log("----step 11");
                        // converting password string to secure-string                    
                        //SecureString pwd = new SecureString();
                        //foreach (char c in cmd.parameterList[2].parameter_value) pwd.AppendChar(c);
                        ////cls_Utility.Log("----step 22");
                        //pwd.MakeReadOnly();
                        //cls_Utility.Log("----step 33");

                        if (cls_System.StartProcess(cls_File.PopulatePath(@".\grace_updater.exe"), senderIP.ToString(), cmd.parameterList[1].parameter_value, cmd.parameterList[2].parameter_value, cmd.parameterList[3].parameter_value) != null)
                        {
                            //cls_Utility.Log("----step 44");
                            frm_Terminal.StopApplicationService();
                            cls_System.KillAProcessByName(cls_Utility.processName);
                            return "{@update:start@}";
                        }
                        else
                        {
                            return "! Error on updating. updater cannot started.";
                        }
                    }
                    return "! Update procedure is not completed.";
                }

                else
                {
                    return "! Command \'" + cmd.masterCommand.master_command_name + "\' does not have parameter named \'" + cmd.parameterList[0].parameter_value + "\'.";
                }
            }
            catch (Exception ex)
            {
                //var st = new StackTrace();
                //var sf = st.GetFrame(0);

                var currentMethodName = (new StackTrace()).GetFrame(0).GetMethod().Name;
                return "! Error occured on \'" + currentMethodName + "\' method. " + ex.Message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string ssh(cls_Command cmd)
        {
            // if user used the command with no parameters
            if (cmd.parameterList == null || cmd.parameterList.Count == 0)
            {
                return cmd.masterCommand.master_command_description;
            }

            try
            {
                if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 187)//ssh connect
                {

                    // Create a new telnet connection to hostname "gobelijn" on port "23"
                    //tc = new TelnetConnection(cmd.parameterList[1].parameter_value, 23);
                    sshc = new cls_Network.SSHConnection(cmd.parameterList[1].parameter_value, cmd.parameterList[2].parameter_value, cmd.parameterList[3].parameter_value, 22);
                    // Login with user "root",password "rootpassword", using a timeout of 100ms, and show server output
                    //string sResult = tc.Login(cmd.parameterList[2].parameter_value, cmd.parameterList[3].parameter_value, 100);
                    if (sshc.Connect())
                        return "** SSH connected.";
                    else
                        return "** SSH not connected.";

                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 190)//ssh command
                {

                    string sResult = sshc.RunACommand(cmd.parameterList[1].parameter_value);
                    return sResult;

                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 193)//ssh dissconnect
                {
                    if (sshc != null)
                    {
                        sshc.Dissconnect();
                        return "** SSH dissconnected.";
                    }
                    else
                    {
                        return "** SSH was not connected.";
                    }
                }

                else
                {
                    return "! Command \'" + cmd.masterCommand.master_command_name + "\' does not have parameter named \'" + cmd.parameterList[0].parameter_value + "\'.";
                }
            }
            catch (Exception ex)
            {
                //var st = (new StackTrace());
                //var sf = (new StackTrace()).GetFrame(0);

                var currentMethodName = (new StackTrace()).GetFrame(0).GetMethod().Name;
                return "! Error occured on \'" + currentMethodName + "\' method. " + ex.Message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string firewall(cls_Command cmd)
        {
            // if user entered the command with no parameters
            if (cmd.parameterList == null || cmd.parameterList.Count == 0)
            {
                return cmd.masterCommand.master_command_description;
            }


            try
            {
                if (fwall == null)
                {
                    fwall = new cls_Network.cls_Firewall();
                }
            }
            catch (Exception ex)
            {
                cls_Utility.Log("! Error on instanciating firewall object." + ex.Message);
                return "! Error on instanciating firewall object." + ex.Message;
            }

            try
            {
                if (cmd.parameterList[0].master_command_id != -1 && (cmd.parameterList[0].parameter_id == 158))// on
                {
                    fwall.FirewallStart();
                    return "** Firewall started.";
                }
                else if (cmd.parameterList[0].master_command_id != -1 && (cmd.parameterList[0].parameter_id == 159))// off
                {
                    fwall.FirewallStop();
                    return "** Firewall stopped.";
                }
                else if (cmd.parameterList[0].master_command_id != -1 && (cmd.parameterList[0].parameter_id == 195))// add
                {
                    string s_IPExpression = cmd.parameterList[1].parameter_value;
                    List<string> IPs = new List<string>();

                    if (s_IPExpression != "")
                    {
                        if(cmd.parameterList.Count > 2)
                            s_IPExpression += "," + string.Join(",", cmd.parameterList.Select(m => m.parameter_value).Skip(2).ToArray());

                        if (s_IPExpression.Contains(",")) // multi ip add
                            IPs = s_IPExpression.Split(new char[] { ',' }).ToList();

                        else if (s_IPExpression.Contains("-")) // range ip add
                        {
                            s_IPExpression = s_IPExpression.Substring(s_IPExpression.IndexOf(",") + 1);
                            string[] ip_begin_end = s_IPExpression.Split(new char[] { '-' });
                            cls_IPTools it = new cls_IPTools();
                            IEnumerable<string> addresses = it.GetIPRange(IPAddress.Parse(ip_begin_end[0]), IPAddress.Parse(ip_begin_end[1]));
                            foreach (string ad in addresses)
                            {
                                IPs.Add(ad);
                            }
                        }
                        else // single ip add
                        {
                            IPs.Add(s_IPExpression);
                        }

                        string sMessage = $"IPs were added. \r\n";
                        int i_total = 0, i_exception = 0;
                        foreach (string ip in IPs)
                        {                            
                            if (!fwall.FirewallAddToBlockList(ip))
                                i_exception++;
                            else
                                i_total++;
                            
                        }
                        return sMessage + $"Total: {i_total} Exception: {i_exception}" ;
                    }

                    return "! Error occured while adding IP(s) to firewall";
                }
                else if (cmd.parameterList[0].master_command_id != -1 && (cmd.parameterList[0].parameter_id == 196))// delete
                {
                    if (fwall.FirewallRemoveFromBlockList(cmd.parameterList[1].parameter_value))
                        return "** Firewall block list updated.";
                    else
                        return "! IP is not exsist in the list.";
                }
                else if (cmd.parameterList[0].master_command_id != -1 && (cmd.parameterList[0].parameter_id == 197))// clear
                {
                    fwall.FirewallClear();
                    return "** Firewall stopped and cleared.";
                }
                else if (cmd.parameterList[0].master_command_id != -1 && (cmd.parameterList[0].parameter_id == 198))// setup
                {
                    fwall = new cls_Network.cls_Firewall();
                    return "** Firewall is all set.";
                }
                else if (cmd.parameterList[0].master_command_id != -1 && (cmd.parameterList[0].parameter_id == 199))// toggle
                {                    
                    return $"** Firewall state changed to {fwall.FirewallToggle().ToString()}";
                }

                else
                {
                    return "! Command \'" + cmd.masterCommand.master_command_name + "\' does not have parameter named \'" + cmd.parameterList[0].parameter_value + "\'.";
                }
            }
            catch (Exception ex)
            {
                //var st = (new StackTrace());
                //var sf = (new StackTrace()).GetFrame(0);

                var currentMethodName = (new StackTrace()).GetFrame(0).GetMethod().Name;
                return "! Error occured on \'" + currentMethodName + "\' method. " + ex.Message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string agent(cls_Command cmd)
        {
            //cls_Utility.Log("----step -1");
            // if user entered the command with no parameters
            if (cmd.parameterList == null || cmd.parameterList.Count == 0)
            {
                return cmd.masterCommand.master_command_description;
            }

            try
            {
                if (cmd.parameterList[0].master_command_id != -1 && (cmd.parameterList[0].parameter_id == 201))// send
                {
                    cls_Network.PipeServerSendText(cmd.parameterList[1].parameter_value);
                    return "** Text sent to agent.";
                }
                else if (cmd.parameterList[0].master_command_id != -1 && (cmd.parameterList[0].parameter_id == 204))// start
                {
                    //cls_System.StartProcess(cls_File.PopulatePath(@".\gagent.exe"), "", cmd.parameterList[1].parameter_value, cmd.parameterList[2].parameter_value, cmd.parameterList[3].parameter_value);
                    //return "** Agent started with result of success.";

                    if (senderIP == null)
                        senderIP = cls_Network.cls_IPTools.GetLocalActiveIP(cls_Network.pingableIPAddress, cls_Network.PORT);

                    if (cmd.parameterList.Count == 1) // If no username and password supplied
                    {
                        if (cls_System.StartProcess(cls_File.PopulatePath(@".\gagent.exe"), senderIP.ToString()) != null)
                        {
                            //frm_Terminal.StopApplicationService();
                            //cls_System.KillAProcessByName(cls_Utility.processName);
                            return "{@agent:start@}";
                        }
                        else
                        {
                            return "! Error on running the agent. agent cannot started.";
                        }
                    }
                    else
                    {
                        return "use no parameters for now!";
                    }
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 207)// stop
                {
                    try
                    {
                        cls_System.KillAProcessByName("gagent");
                    }
                    catch(Exception ex)
                    {
                        return "! Error on killing the agent! " + ex.Message;
                    }
                    return "** agent gone back home!";
                }
                else if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 208)// batch commands
                {
                    try
                    {
                        string sCommand = string.Join(",", cmd.parameterList.Select(m => m.parameter_value).Skip(1).ToArray());
                        string[] lines = sCommand.Split(new char[] { ';' });
                        string sText = string.Join("\r", lines);
                        cls_File.WriteTextToFile(cls_File.PopulatePath(@".\abat.abf"), sText);
                    }
                    catch(Exception ex)
                    {
                        return "! Error on creating agent batch file. " + ex.Message;
                    }
                    return "** Agent batch file created in the current directory with name 'abat.abf'";
                }

                else
                {
                    return "! Command \'" + cmd.masterCommand.master_command_name + "\' does not have parameter named \'" + cmd.parameterList[0].parameter_value + "\'.";
                }
            }
            catch (Exception ex)
            {
                //var st = new StackTrace();
                //var sf = st.GetFrame(0);

                var currentMethodName = (new StackTrace()).GetFrame(0).GetMethod().Name;
                return "! Error occured on \'" + currentMethodName + "\' method. " + ex.Message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string ping(cls_Command cmd)
        {
            //cls_Utility.Log("----step -1");
            // if user entered the command with no parameters
            if (cmd.parameterList == null || cmd.parameterList.Count == 0)
            {
                return cmd.masterCommand.master_command_description;
            }

            try
            {
                //if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id == 129)// ping
                //{
                    //if (cmd.parameterList[1].parameter_id == 158)// on
                    //{
                    
                        IPAddress thisIPAddress = cls_Network.cls_IPTools.GetLocalActiveIP(cls_Network.pingableIPAddress, cls_Network.PORT);
                        PingReply pr = cls_Network.Ping(thisIPAddress, IPAddress.Parse(cmd.parameterList[0].parameter_value), 3000, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32 });
                //Thread_ping = new Thread(new ThreadStart())
                        return "** Ping reply from \'" + pr.IpAddress.ToString() + "\'    Bytes=\'" + (pr.Buffer != null ? pr.Buffer.Length : 0) + "\'    Status=\'" + pr.Status.ToString() + "\'    Time=\'" + pr.RoundTripTime.Milliseconds + "ms\'";
                    //}
                    //else if (cmd.parameterList[1].parameter_id == 159)// off
                    //{
                    //    cls_System.DisableNIC(cmd.parameterList[2].parameter_value);
                    //    return "** NIC disabled.";
                    //}
                    //return "**";
                //}
                //else
                //{
                //    return "! Command \'" + cmd.masterCommand.master_command_name + "\' does not have parameter named \'" + cmd.parameterList[0].parameter_value + "\'.";
                //}
            }
            catch (Exception ex)
            {
                var currentMethodName = (new StackTrace()).GetFrame(0).GetMethod().Name;
                return "! Error occured on \'" + currentMethodName + "\' method. " + ex.Message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string registry(cls_Command cmd)
        {
            // if user entered the command with no parameters
            if (cmd.parameterList == null || cmd.parameterList.Count == 0)
            {
                return cmd.masterCommand.master_command_description;
            }

            try
            {
                if (cmd.parameterList[0].master_command_id != -1 && (cmd.parameterList[0].parameter_id == 210))// delete
                {
                    if (cls_System.cls_Registry.DeleteKey(cmd.parameterList[1].parameter_value, cmd.parameterList[2].parameter_value))
                        return "** Registry key has been deleted.";
                    else
                        return "! Error on deleting registry key";
                }
                else if (cmd.parameterList[0].master_command_id != -1 && (cmd.parameterList[0].parameter_id == 213))// hideme
                {
                    if (cls_System.cls_Registry.HideMe())
                        return "**\"" + cls_Utility.processName + "\" is hidden Now.";
                    else
                        return "!\"" + cls_Utility.processName + "\" is already hidden or not found.";
                }
                else if (cmd.parameterList[0].master_command_id != -1 && (cmd.parameterList[0].parameter_id == 214))// _override
                {
                    string s_path = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options";
                    if (cmd.parameterList[1].parameter_id == 159)// off
                    {
                        if (cls_System.cls_Registry.DeleteKey(s_path, cmd.parameterList[2].parameter_value))
                            return "** Registry key " + cmd.parameterList[2].parameter_value + " has been deleted.";
                        else
                            return "! Error on deleting registry key";
                    }
                    else //is not "off"
                    {
                        if (cls_System.cls_Registry.CreateKey(s_path, cmd.parameterList[1].parameter_value))
                            if (cls_System.cls_Registry.SetValue(s_path, cmd.parameterList[1].parameter_value, "debugger", cmd.parameterList[2].parameter_value))
                                return "** App " + cmd.parameterList[1].parameter_value + " open action Overrided to " + cmd.parameterList[2].parameter_value + " successfully.";
                            else
                                return "! Error on overriding.";
                    }
                    return "";
                }
                else if (cmd.parameterList[0].master_command_id != -1 && (cmd.parameterList[0].parameter_id == 215))// _default app
                {
                    //var imgKey = cls_System.cls_Registry.ClassesRoot.OpenSubKey(".jpg")
                    //var imgType = key.GetValue("");
                    return "** not ready yet!";
                }


                else
                {
                    return "! Command \'" + cmd.masterCommand.master_command_name + "\' does not have parameter named \'" + cmd.parameterList[0].parameter_value + "\'.";
                }
            }
            catch (Exception ex)
            {
                var currentMethodName = (new StackTrace()).GetFrame(0).GetMethod().Name;
                return "! Error occured on \'" + currentMethodName + "\' method. " + ex.Message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string version(cls_Command cmd)
        {
            // if user entered the command with no parameters
            if (cmd.parameterList == null || cmd.parameterList.Count == 0)
            {
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                return version;

                //IPAddress ip = cls_Network.cls_IPTools.GetLocalActiveIP(cls_Network.pingableIPAddress, cls_Network.PORT);
                //string localhostAddress = ip.ToString();
                //string sIp = cls_Utility.GetElementValue("startswith", "{@startswith:172.29.210@}");
                //if (localhostAddress.StartsWith(sIp))
                //{
                //    return sIp;
                //}
                //else { /*if the command is not for this workstation nothing happens*/ }                
            }

            try
            {
                return "This command has no parameter.";
            }
            catch (Exception ex)
            {
                var currentMethodName = (new StackTrace()).GetFrame(0).GetMethod().Name;
                return "! Error occured on \'" + currentMethodName + "\' method. " + ex.Message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string log(cls_Command cmd)
        {
            // if user entered the command with no parameters
            if (cmd.parameterList == null || cmd.parameterList.Count == 0)
            {                
                return "** Current log status is: " + (cls_Utility.isEchoOff ? "off" : "on");
            }

            try
            {

                if (cmd.parameterList[0].master_command_id != -1 && cmd.parameterList[0].parameter_id != -1)
                {
                    if (cmd.parameterList[0].parameter_id == 159)// off
                    {
                        cls_Utility.isEchoOff = true;
                    }

                    else if (cmd.parameterList[0].parameter_id == 158)// on
                    {
                        cls_Utility.isEchoOff = false;
                    }                   

                    else
                    {
                        return "! Unknown or Unsuitable Parameter \'" + cmd.parameterList[0].parameter_value + "\'.";
                    }                   
                }
                return "** log state changed to \'" + cmd.parameterList[0].parameter_value + "\'.";

            }
            catch (Exception ex)
            {
                var currentMethodName = (new StackTrace()).GetFrame(0).GetMethod().Name;
                return "! Error occured on \'" + currentMethodName + "\' method. " + ex.Message;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string run(cls_Command cmd)
        {
            // if user entered the command with no parameters
            if (cmd.parameterList == null || cmd.parameterList.Count == 0)
            {
                return cmd.masterCommand.master_command_description;
            }

            try
            {
                // Start the child process.
                Process p = new Process();
                // Redirect the output stream of the child process.
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                if (cmd.parameterList.Count == 1)
                {                    
                    p.StartInfo.FileName = cmd.parameterList[0].parameter_value;                    
                }else if(cmd.parameterList.Count == 2)
                {                    
                    p.StartInfo.FileName = cmd.parameterList[0].parameter_value;
                    p.StartInfo.Arguments = cmd.parameterList[1].parameter_value;                    
                }
                p.Start();
                //System.Diagnostics.Process.Start(cmd.parameterList[0].parameter_value, "/c");
                // Do not wait for the child process to exit before
                // reading to the end of its redirected stream.
                // p.WaitForExit();
                // Read the output stream first and then wait.
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                return output;

            }
            catch (Exception ex)
            {
                var currentMethodName = (new StackTrace()).GetFrame(0).GetMethod().Name;
                return "! Error occured on \'" + currentMethodName + "\' method. " + ex.Message;
            }

        }
    }
}
