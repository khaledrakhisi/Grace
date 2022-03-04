using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace grace
{
    
    public class cls_Scheduler
    {
        public static schedules scheduleList = null;
        private static string sFileFullName = Path.Combine(Path.GetDirectoryName(new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath), @"sch");
        //private static string sFileFullNameDisabled = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"sch-d");
        private static string sFileFullNameDisabledMode = Path.Combine(Path.GetDirectoryName(new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath), @"sch-d");

        //Serializing the List
        public static void Serialize<T>(T emps, String filename)
        {
            //Create the stream to add object into it.
            System.IO.Stream ms = File.OpenWrite(filename);
            //Format the object as Binary
            BinaryFormatter formatter = new BinaryFormatter();
            //It serialize the employee object
            formatter.Serialize(ms, emps);
            ms.Flush();
            ms.Close();
            ms.Dispose();
        }
        //Deserializing the List
        public static T Deserialize<T>(String filename)
        {
            FileStream fs = null;
            object obj = null;
            T emps = default(T);
            try
            {
                //Format the object as Binary
                BinaryFormatter formatter = new BinaryFormatter();
                //Reading the file from the server
                fs = File.Open(filename, FileMode.Open);
                //It deserializes the file as object.
                obj = formatter.Deserialize(fs);
                //Generic way of setting typecasting object.
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                emps = (T)obj;
                fs.Flush();
                fs.Close();
                fs.Dispose();
            }
            return emps;
        }
        //Deserializing the List and displaying it.
        public static void SchedulerLoad()
        {
            try
            {
                if (!File.Exists(sFileFullName))
                    throw new FileNotFoundException();

                // Deserializing the collection
                scheduleList = Deserialize<schedules>(sFileFullName);
            }
            catch (Exception ex)
            {
                //cls_scheduler.scheduleList = new schedules();
                throw ex;
            }            
        }
        public static void SchedulerSave()
        {
            if (scheduleList == null)
            {
                throw new IndexOutOfRangeException();
            }
            //Serializing the collection
            Serialize(scheduleList, sFileFullName);
        }
        public static List<schedule> FindASchedule(string date, ulong timerElapsedMinutes)
        {
            List<schedule> schList = new List<schedule>();

            foreach (schedule sch in scheduleList)
            {
                //string s = cls_Utility.GetDateString(date);
                //string t = cls_Utility.GetTimeString(date);
                //if (sch.schedule_date == "{@everyday}")
                //{
                //    string sEveryDay = cls_Utility.GetElementValue("everyday", sch.schedule_date);
                //}
                //cls_Utility.Log("####Now date:" +cls_Utility.GetDateString(date)+" time:"+cls_Utility.GetTimeString(date)+"######Single schedule date:" + cls_Utility.GetDateString(sch.schedule_date) + "   Time:" + cls_Utility.GetTimeString(sch.schedule_time));

                if ((cls_Utility.GetDateString(sch.schedule_date) == cls_Utility.GetDateString(date) || sch.schedule_date == "{@everyday@}") && (sch.schedule_time.StartsWith("{@everyminutes:")))
                {                    
                    string s_numOrNumRange = cls_Utility.GetElementValue("everyminutes", sch.schedule_time);                    
                    ulong t = 1;
                    try
                    {
                        if (s_numOrNumRange.Contains("-")) // if random range given
                        {
                            string[] numbers = s_numOrNumRange.Split(new Char[] { '-' });
                            t = (ulong)new Random().Next(int.Parse(numbers[0]), int.Parse(numbers[1])+1);
                        }
                        else // if single number given
                        {
                            t = ulong.Parse(s_numOrNumRange);
                        }
                        cls_Utility.Log("Num or Numrange: " + t.ToString());
                    }
                    catch (Exception ex)
                    {
                        cls_Utility.Log("! Error on getting and converting minutes range: " + ex.Message);
                    }
                    
                    //cls_Utility.Log("Checking timerElapsedMinutes(" + timerElapsedMinutes.ToString() + ")==schedulerRunTime(" + t);
                    if(timerElapsedMinutes % t/*randomEveryNSecond*/ < .3f)
                        schList.Add(sch);
                }

                else if ((cls_Utility.GetDateString(sch.schedule_date) == cls_Utility.GetDateString(date) || sch.schedule_date == "{@everyday@}")
                   && (cls_Utility.GetTimeString(sch.schedule_time) == cls_Utility.GetTimeString(date)))
                {
                    schList.Add(sch);
                }

            }
            return schList;
        }
        public static List<schedule> FindATrigger(string sTriggerName, string sTriggerParameter)
        {
            List<schedule> schList = new List<schedule>();

            foreach (schedule sch in scheduleList)
            {                
                if (sch.schedule_date == sTriggerName && sch.schedule_time == sTriggerParameter)
                {                    
                    schList.Add(sch);
                }               
            }
            return schList;
        }
        public static bool IsThereAnyTrigger()
        {
            List<schedule> schList = new List<schedule>();

            foreach (schedule sch in scheduleList)
            {
                if (sch.schedule_date.StartsWith("{@on"))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool SchedulerEnable()
        {
            try
            {
                scheduleList = new schedules();

                if (IsSchedulerExist(true))
                {
                    File.Move(sFileFullNameDisabledMode, sFileFullName);
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return false;
        }        
        public static bool SchedulerDisable()
        {
            if (IsSchedulerExist(false))
            {
                cls_Scheduler.scheduleList = null;
                File.Move(sFileFullName, sFileFullNameDisabledMode);
                return true;
            }

            return false;
        }
        public static bool IsSchedulerExist(bool bSearchAsDisabledModePath)
        {
            return File.Exists(bSearchAsDisabledModePath ? sFileFullNameDisabledMode : sFileFullName);
        }
    }

    //Classes
    [Serializable]
    public class schedule
    {
        public schedule(int sch_id, string sch_date, string sch_time, int sch_runTotal, string sch_command)
        {
            schedule_id = sch_id;
            schedule_date = sch_date;
            schedule_time = sch_time;
            schedule_runTotal = sch_runTotal;
            schedule_command = sch_command;            
        }

        public int schedule_id { get; set; } = 0;
        public string schedule_date { get; set; }
        public string schedule_time { get; set; }
        public string schedule_command { get; set; }
        public int schedule_runTotal { get; set; }
    }

    [Serializable]
    public class schedules : CollectionBase
    {
        //Constructor
        public schedules()
        {

        }
        //Add function
        public void AddItem(schedule objT)
        {
            this.List.Add(objT);
        }
        //Add function
        public void RemoveItem(int index)
        {
            this.List.RemoveAt(index);
        }
        public void ReplaceItem(int index, schedule objT)
        {
            this.List[index] = objT;
        }
        public void MoveItem(int oldIndex, int newIndex)
        {
            object item = this.List[oldIndex];
            RemoveItem(oldIndex);
            this.List.Insert(newIndex, item);
        }
        public bool Contains(object value)
        {
            bool inList = false;
            for (int i = 0; i < Count; i++)
            {
                if (this.List[i] == value)
                {
                    inList = true;
                    break;
                }
            }
            return inList;
        }

        //Indexer
        public schedules this[int i]
        {
            get
            {
                return (schedules)this.List[i];
            }
            set
            {
                this.List.Add(value);
            }
        }
    }
}
