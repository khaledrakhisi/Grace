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
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

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
            try
            {
                if (DateTime.Now.ToString("HH:mm") == "00:00" || DateTime.Now.ToString("HH:mm") == "09:00" || DateTime.Now.ToString("HH:mm") == "12:00")
                {
                    cls_Utility.ClearLogFile();
                }
            }
            catch
            {

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
                cls_Utility.Log("! Error - Service1. init run total schedule failed. " + ex.Message);
            }
            //cls_Utility.Log("** Scheduler 1 Minute Tik(elapsed:"+elapsedMinutes.ToString()+")................. OK");
            //if (elapsedMinutes % 4/*randomEveryNSecond*/ < .3f)
            //{
            //    cls_Utility.Log("** every 4 minutes@@@@@@@@@@@");
            //}

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
                cls_Utility.Log("! Error - Service1. loading schedules failed. " + ex.Message);
            }

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
