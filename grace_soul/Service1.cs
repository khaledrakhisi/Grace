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
        schedule sch_to_run = null;
        private static ulong elapsedMinutes = 0;        

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
                    //_timer_trigger = new System.Timers.Timer();
                    //_timer_trigger.Interval = 2 * 1000; //every 2 second
                    //_timer_trigger.Elapsed += new System.Timers.ElapsedEventHandler(_timer_trigger_Elapsed);
                    //_timer_trigger.Start();

                    //cls_Utility.Log("** Triggerer Start................. OK");

                    // Trigger: 'OnStartup' event
                    #region OnStartup
                    RunTrigger("{@onstartup@}", "");
                    #endregion
                }
            }
            catch (Exception ex)
            {
                cls_Utility.Log("! Error on starting _timer_trigger." + ex.Message);
            }            
        }

        protected override void OnStop()
        {
            // Trigger: 'OnShutdown' Checking
            #region OnShutdown
            RunTrigger("{@onshutdown@}", "");
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

            #region Connecting to and running GET from API "grace.khaledr.ir/api/commands"
            string JSON_data = cls_Network.http_GET("api/commands");

            if (JSON_data != null)
            {
                cls_Utility.Log("\r\n" + "Http GET successfully.");

                Commands commands = new JavaScriptSerializer().Deserialize<Commands>(JSON_data);

                cls_Utility.Log("\r\n" + "JSON deserialized successfully.");

                if (commands != null)
                {
                    foreach (Command command in commands.commands)
                    {
                        #region Deleting the just fetched commands
                        string JSON_response = cls_Network.http_DELETE("api/commands/" + command.id);
                        if (JSON_response != null)
                            cls_Utility.Log("\r\n" + "Fetched command [" + command.id + "] deleted successfully.");
                        #endregion

                        if (cls_Network.ValidateIPv4(command.forWhom))
                        {
                            string localhostname = 
                        }

                        #region Running fetched command(s)
                        //object result = cls_Interpreter.RunACommand(command.text, null);
                        //if (result != null)
                        //    cls_Utility.Log("\r\n" + result);
                        #endregion

                        #region Adding deleted command(s) to History
                        cls_Utility.Log("\r\n" + "Deleted command [" + command.id + "] added to history successfully.");
                        #endregion
                    }

                }
            }
            else
            {
                //cls_Utility.Log("\r\n" + "TIK: no Https command found.");
            }
            #endregion

            elapsedMinutes++;

            try
            {
                _timer_runTotal = new System.Timers.Timer();                
                _timer_runTotal.Elapsed += new System.Timers.ElapsedEventHandler(_timer_runTotal_Elapsed);
                _timer_runTotal.Stop();
            }
            catch(Exception ex)
            {
                cls_Utility.Log("! Error - Service1. init run total schedule failed. " + ex.Message);
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

            #region Checking schedules that matches
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
    }
}
