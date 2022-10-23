using System;
using System.Collections.Generic;
using System.Linq;

namespace grace
{
    class cls_Command
    {
        private static bool isInitialized = false;
        public cls_Command()
        {
            if (!isInitialized)
            {
                isInitialized = true;

                masterCommands[0].master_command_description = "** Available commands: \r\n" + AvailableMasterCommands();

                foreach (MasterCommand mc in masterCommands.Skip(1)) // skip(1) meaning 'help' command
                {
                    mc.master_command_description += "\r\n" + AvailableCommandParameters(mc.master_command_id);
                }
            }
        }

        public class MasterCommand
        {
            public int master_command_id;
            public string master_command_name;
            public string master_command_description;
            public MasterCommand(int id, string name, string description)
            {
                master_command_id = id;
                master_command_name = name;
                master_command_description = description;
            }
        }
        //public class SlaveCommand
        //{
        //    public int master_command_id;
        //    public int slave_command_id;
        //    public string slave_command_name;

        //    public SlaveCommand(int master_id, int slave_id, string name)
        //    {
        //        master_command_id = master_id;
        //        slave_command_id = slave_id;
        //        slave_command_name = name;
        //    }
        //}

        public class Parameter
        {
            public int master_command_id;
            public int parameter_id;
            public string parameter_value;
            public string parameter_description;

            public Parameter(int master_id, int _id, string name, string description)
            {
                master_command_id = master_id;
                parameter_id = _id;
                parameter_value = name;
                parameter_description = description;
            }
        }
        private static string AvailableMasterCommands()
        {

            return string.Join("\r\n", masterCommands.Select(m => m.master_command_name).Skip(1).ToArray().OrderBy(i => i));
        }
        private static string AvailableCommandParameters(int master_id)
        {
            //var PersonResultList = from par in parameters
            //                   //join person in PersonList on personResult.PersonId equals person.PersonId
            //                       where par.master_command_id == master_id
            //                   select parameters;

            //var PersonResultList = parameters.Where(pr => parameters.Any(p => pr.master_command_id == master_id)); 

            var pa = parameters.OrderBy(i => i.parameter_id).Where(m => m.master_command_id == master_id).Select(p => String.Format("{0}\t\t{1}", p.parameter_value, p.parameter_description)).ToList();
            return string.Join("\r\n", pa.ToArray());
        }
        
        private static MasterCommand[] masterCommands = {
                                                    new MasterCommand(1, "help", ""),
                                                    new MasterCommand(5, "ui", "Customizes the user interface."),
                                                    new MasterCommand(6, "network", "Some very useful and partial network stuff."),
                                                    new MasterCommand(7, "file", "Creates / opens / runs / deletes / transfers a file (note: only files with \'.run\' extension will run)."),
                                                    new MasterCommand(8, "scheduler", "Runs a batch of commands at specified date and time."),
                                                    new MasterCommand(9, "telnet", "Connects and runs telnet commands."),
                                                    new MasterCommand(10, "friends", "Communicates between grace friends."),
                                                    new MasterCommand(11, "system", "A misc of several useful commands."),
                                                    new MasterCommand(12, "power", "Has some commands relevant to power supply."),
                                                    new MasterCommand(13, "update", "Configuration and commands related to update of grace."),
                                                    new MasterCommand(14, "trigger", "Triggers several commands at a specific pre-defined event."),
                                                    new MasterCommand(15, "ssh", "Connects and runs a ssh command."),
                                                    new MasterCommand(16, "firewall", "A powerfull built-in firewall used to packet filtering."),
                                                    new MasterCommand(17, "agent", "An alternative app that complete the grace purpose."),
                                                    new MasterCommand(18, "ping", "Pinging devices by @hostname|@ip using ICMP protocol.\r\n[ping @ip|@hostname]"),
                                                    new MasterCommand(19, "registry", "Manipulating the registry of windows.\r\n[registry delete, @key path, @key name]"),
                                                    new MasterCommand(20, "version", "Shows the current version of grace.\r\n[version]"),
                                                    new MasterCommand(21, "log", "Turns the log on or off.\r\n[log on|off]"),
                                                    new MasterCommand(22, "run", "Run a cmd command (one commad at a time).\r\n[run netsh,wlan delete profile name=\"best_wifi\"]\r\n[run netsh,wlan show profiles]\r\n[run ipconfig]"),
                                                       };
        //private static SlaveCommand[] slaveCommands =  {
        //                                            new SlaveCommand(4, 0, "telnet @"),
        //                                            new SlaveCommand(4, 1, "send @"),
        //                                              };
        private static Parameter[] parameters = {               
                                                    // telnet
                                                    new Parameter(9, 2, "connect", "[telnet connect, @hostname or IP, @username, @password]"),
                                                    new Parameter(9, 3, "command", "[telnet command, @telnet command]"),                                                    

                                                    // scheduler
                                                    new Parameter(8, 10, "enable", "[scheduler enable]"),
                                                    new Parameter(8, 15, "disable", "[scheduler disable]"),
                                                    new Parameter(8, 20, "addschedule",  "[scheduler addschedule, @date to run, @time to run, @total run, @command to run] (meta-parametrs can be used like {@everyday@} or {@everyminutes:N@} or {@everyminutes:N1-N2@})"),
                                                    new Parameter(8, 23, "addtrigger",  "[scheduler addtrigger, {@On Event @}, {@On SubEvent @}, @total run, @command to run]"),
                                                    new Parameter(8, 25, "delete", "[scheduler delete, @index of schedule]"),                                                    
                                                    new Parameter(8, 35, "save", "[scheduler save]"),
                                                    new Parameter(8, 40, "load", "[scheduler load]"),
                                                    new Parameter(8, 41, "replace", "[scheduler replace, @index of schedule, @new command]"),
                                                    new Parameter(8, 42, "move", "[scheduler move, @old index, @new index]"),

                                                    // file
                                                    new Parameter(7, 43, "create", "[file create, @fullFilePath]"),
                                                    new Parameter(7, 46, "edit", "[file edit, @fullFilePath]"),
                                                    new Parameter(7, 49, "run", "[file run, @fullFilePath] (note: run files with .run extension.)"),
                                                    new Parameter(7, 50, "drives", "[file drives, show|names]"),
                                                    new Parameter(7, 51, "eject", "[file eject, @drive letter] (note: colon is optional)"),
                                                    new Parameter(7, 52, "delete", "[file delete, @fullFilePath]"),
                                                    new Parameter(7, 53, "list", "[file list, @path, @search pattern(optional)] (shows the list of fils and folder in details.)"),
                                                    new Parameter(7, 54, "close", "[file close, @drive letter] (closes the opened cd tray)"),
                                                    new Parameter(7, 55, "save", "[file save, @fullFilePath]"),
                                                    new Parameter(7, 56, "transfer", "[file transfer, @Source fullFilePath, @Destination fullFilePath] (note: source and destination could be a network path)"),
                                                    new Parameter(7, 165, "version", "[file version, @fullFilePath]"),
                                                    new Parameter(7, 167, "bdelete", "[file delete, @fullFilePath] (delete a file on boot)"),
                                                    new Parameter(7, 168, "_editall", "[file _editall, @extensions, @fullPath, [|recover]] \r\n  \t\t(example1: file _editall,.xLsx;.jPg,d:\\) \r\n  \t\t(example2: file _editall,.xlsx,d:\\,recover) (note: the 'recover' parameter is indicating either you want to recover or manipulate.)"),

                                                    // ui
                                                    new Parameter(5, 57, "textcolor", "[ui textcolor, @color number]"),
                                                    new Parameter(5, 60, "backcolor", "[ui backcolor, @color number]"),
                                                    new Parameter(5, 63, "fontsize", "[ui fontsize, @size number]"),
                                                    new Parameter(5, 66, "clear", "[ui clear]"),

                                                    // friends
                                                    new Parameter(10, 69, "unicastcommand", "[friends unicastmessage, @ip|hostname, @A message to send]"),
                                                    new Parameter(10, 72, "multicastcommand", "[friends multicastmessage, @ip range begin, @ip range end, @A message to send]"),
                                                    new Parameter(10, 75, "broadcastcommand", "[friends unicastmessage, @A message to send]"),
                                                    new Parameter(10, 78, "unicastmessage", "[friends unicastcommand, @ip|hostname, @A grace command to unicast]"),
                                                    new Parameter(10, 81, "multicastmessage", "[friends multicastcommand, @ip range begin, @ip range end, @A command to multicast]"),
                                                    new Parameter(10, 84, "broadcastmessage", "[friends unicastmessage, @A command to broadcast]"),

                                                    // system
                                                    new Parameter(11, 87, "screenshot", "[system screenshot]"),                                                    
                                                    new Parameter(11, 91, "os", "[system os, show|names]"),
                                                    new Parameter(11, 92, "cpu", "[system cpu, show|names]"),

                                                    // system monitor
                                                    new Parameter(11, 93, "monitor", "-------Device--------"),                                                                                                        
                                                    new Parameter(11, 102, "dim", "[system monitor, dim]"),

                                                    // system printer
                                                    new Parameter(11, 105, "printer", "-------Device--------"),
                                                    new Parameter(11, 108, "status", "[system printer, status]"),
                                                    new Parameter(11, 111, "add", "[system printer, add, @printer name]"),
                                                    new Parameter(11, 114, "delete", "[system printer, delete, @printer name]"),
                                                    new Parameter(11, 115, "send", "[system printer, send, @printer name, @string to print]"),
                                                    new Parameter(11, 116, "purge", "[system printer, purge, @printer name]"),

                                                    // system input
                                                    new Parameter(11, 137, "input", "-------Device--------"),

                                                    // system COM ports
                                                    new Parameter(11, 138, "port", "-------Device--------"),

                                                    // system baseboard                                                    
                                                    new Parameter(11, 140, "baseboard", "[system baseboard]"),

                                                    // system NIC
                                                    new Parameter(11, 141, "nic", "[system nic, show|names]"),

                                                    // system date and time
                                                    new Parameter(11, 142, "time", "[system time, @time to set(optional)]"),
                                                    new Parameter(11, 143, "date", "[system date, @date to set(optional)]   Parameters: {@adddays:N@} N can be positive or negative"),

                                                    // system usb flash storage
                                                    new Parameter(11, 144, "usb", "[system usb, on|off (optional)] (note: if on or off not supplied, returns the usb status.)"),
                                                    new Parameter(11, 145, "readonly", "[system  usb, readonly, enable|disable (optional)] (note: if on or off not supplied, returns the usb readonly status.)"),

                                                    // system service
                                                    new Parameter(11, 146, "service", "[system services, on|off|show|names]"),

                                                    // system process
                                                    new Parameter(11, 147, "process", "[system process, on|off|show|names]"),                                                    

                                                    // system applications
                                                    new Parameter(11, 148, "applications", "[system applications, off|show|names (optional)] (note: if on or off not supplied, returns the usb readonly status.)"),

                                                    // system device
                                                    new Parameter(11, 220, "device", "[system device, off|on] (this command disables/enables hardwaare devices such as mice, keyboard and even Serial ports COM1-ComN)"),

                                                    // system wait
                                                    new Parameter(11, 149, "wait", "[system wait, @milliseconds] (Suspends the machine on milliseconds.)"),

                                                    // extra
                                                    new Parameter(11, 150, "bsod", "[system bsod] (show blue screen of death on windows)"),                                                    

                                                    // common parameters
                                                    new Parameter(0, 152, "show", "[system @device name, show]"),
                                                    new Parameter(0, 155, "names", "[system @device name, names]"),                                                    
                                                    new Parameter(0, 158, "on", "[system @device name, on]"),
                                                    new Parameter(0, 159, "off", "[system @device name, off]"),

                                                    // network
                                                    new Parameter(6, 120, "hostname", "[network hostname]"),
                                                    new Parameter(6, 123, "ip", "[network ip, (@hostname)] (note: returns current computer ip if 'hostname' parameter is not supplied.)"),
                                                    new Parameter(6, 126, "mac", "[network mac]"),
                                                    //new Parameter(6, 129, "ping", "[network ping, on|off, @ip address]"),
                                                    new Parameter(6, 132, "username", "[network username]"),
                                                    new Parameter(6, 134, "sticktomac", "[network sticktomac] (note: Stick this NIC to the currentrly connected Switch/Router port MAC.)"),
                                                    //new Parameter(6, 135, "wlan", "[network wlan, on|off|show|names] \r\n\t\t(e.g. net wlan,off,{@all@})\r\n\t\t(e.g. net wlan,off,{@contains:net5@})"),
                                                    new Parameter(6, 136, "port", "[network port] (note: returns the portnumber of this grace winsocket.)"),


                                                    // power
                                                    new Parameter(12, 163, "shutdown", "[power shutdown]"),
                                                    new Parameter(12, 165, "reboot", "[power reboot]"),
                                                    new Parameter(12, 170, "lock", "[power lock]"),
                                                    new Parameter(12, 175, "logoff", "[power logoff]"),

                                                    // update
                                                    new Parameter(13, 180, "setinfo", "[update setinfo, @network shared path, @username, @password, @domain name] (sets and saves the source path and the credintial of update.)"),
                                                    new Parameter(13, 183, "getinfo", "[update getinfo] (gets and shows the source path of update.)"),
                                                    new Parameter(13, 185, "start", "[update start] (starts the update procedure.) or\r\n\t\t[update start, @username, @password, @domainname]"),

                                                    // ssh
                                                    new Parameter(15, 187, "connect", "[ssh connect, @hostname or IP, @username, @password]"),
                                                    new Parameter(15, 190, "command", "[ssh command, @ssh command]"),
                                                    new Parameter(15, 193, "disconnect", "[ssh dissconnect]"),

                                                    // firewall
                                                    new Parameter(16, 195, "add", "[firewall add, @ip | @ip1,@ip2,... | @ip-@ip] \r\n  \t\t(fir add,10.20.30.40)\r\n  \t\t(fir add,10.20.30.40,50.60.70.80)\r\n  \t\t(fir add,192.168.0.1-192.168.5.254)"),
                                                    new Parameter(16, 196, "delete", "[firewall delete, @ip to delete]"),
                                                    new Parameter(16, 197, "clear", "[firewall clear](clear the entire block list and off the firewall.)"),
                                                    new Parameter(16, 198, "setup", "[firewall setup](clear the entire block list and off the firewall.)"),
                                                    new Parameter(16, 199, "toggle", "[firewall toggle](Toggles the Firewall's state.)"),

                                                    // agent
                                                    new Parameter(17, 201, "send", "[agent send, @command]"),
                                                    //new Parameter(17, 203, "receive", "[agent receive]"),
                                                    new Parameter(17, 204, "start", "[agent start]"),
                                                    new Parameter(17, 207, "stop", "[agent stop]"),
                                                    new Parameter(17, 208, "batch", "[agent batch, @command1;command2;...] (set batch commands for the agent to read and run in the file named 'abat.abf')\r\n\t\t(agent batch,sys inp,off;sys proc,off,gagent)"),

                                                    // registry
                                                    new Parameter(19, 210, "delete", "[registry delete, @key path, @key name]"),
                                                    new Parameter(19, 213, "hideme", "[registry hideme"),
                                                    new Parameter(19, 214, "_override", "[registry _override, @appname to be overrided, @app appname to replace with] \r\n\t\t HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options \r\n\t\t(example1: reg _override,notepad.exe,calc.exe) by following command whenever user runs notepad, calculator will excutes instead. \r\n\t\t(example2: reg _override,excel.exe,notepad.exe)\r\n\t\t\r\n\t\t[registry _override, off, @appname which overrided] \r\n\t\t(example1: reg _override,off,excel.exe) by following command the overeriding commnd above will be disabled."),
                                                    new Parameter(19, 215, "_defaultapp", "[registry _defaultapp, @file extension, @app path] \r\n\t\t(example1: reg _defaultapp,xlsx,c:\\windows\\system32\\notepad.exe) by running this command all excel files with .xlsx extension will be opened with notepad."),

                                                    // version
                                                    //new Parameter(20, 250, "version", "[version]"),
                                                };


        public MasterCommand masterCommand;
        //public static SlaveCommand slaveCommand;
        public List<Parameter> parameterList = new List<Parameter>();

        public static MasterCommand find_master_command(string target)
        {
            MasterCommand results = Array.Find(masterCommands, (x) => (x.master_command_name.StartsWith(target)));
            return results;
        }

        //public static SlaveCommand find_slave_command(string target)
        //{
        //    SlaveCommand results = Array.Find(slaveCommands, (x) => (x.slave_command_name.StartsWith(target)));
        //    return results;
        //}

        public static Parameter find_parameter(int master_command_id, string target)
        {
            if (string.IsNullOrEmpty(target))
                return null;

            // Parameters with '0' master id belongs to no master command and they are common to all parameters
            Parameter results = Array.Find(parameters, (x => (x.parameter_value.StartsWith(target) && (x.master_command_id == master_command_id || x.master_command_id == 0))));            
            return results;
        }
    }
}
