using System;

namespace grace
{
    [Serializable]
    class cls_Workstations
    {
        public enum ConnectionStatus {unknown=1, normal, down, provedDown};

        public class NICClass
        {
            public string s_NIC_description;
            public string s_NIC_name;
            public string s_NIC_ip;
            public string s_NIC_subnetMask;
            public string s_NIC_defaultGateway;
            public string s_NIC_metric;
            public string s_NIC_DNSs;

            public int connectionCheck_phase_1_Interval;
            public int connectionCheck_phase_2_Interval;
            public int connectionCheck_pingTimeout;
            public string connectionCheck_destinationIP;
            public ConnectionStatus connectionCheck_status = ConnectionStatus.unknown;
            //public DateTime connectionCheck_downTime;
            public bool flag1 = false;
            public bool flag2 = false;

            public NICClass(string NIC_description, string NIC_name, string NIC_ip, string NIC_subnetMask, string NIC_defaultGateway, string NIC_metric, string NIC_DNSs, string destIP, int interval1 = 2*60*1000, int interval2 = 2*60*1000, int PingTimeout=500)
            {               
                this.s_NIC_description = NIC_description;
                this.s_NIC_name = NIC_name;
                this.s_NIC_ip = NIC_ip;
                this.s_NIC_subnetMask = NIC_subnetMask;
                this.s_NIC_defaultGateway = NIC_defaultGateway;
                this.s_NIC_metric = NIC_metric;
                this.s_NIC_DNSs = NIC_DNSs;

                this.connectionCheck_phase_1_Interval = interval1;
                this.connectionCheck_phase_2_Interval = interval2;

                this.connectionCheck_pingTimeout = PingTimeout;
                this.connectionCheck_destinationIP = destIP;
            }
        }

        //public struct NICStruct
        //{
        //    public  string s_NIC_description;
        //    public  string s_NIC_name;
        //    public  string s_NIC_ip;
        //    public  string s_NIC_subnetMask;
        //    public  string s_NIC_defaultGateway;
        //    public  string s_NIC_metric;
        //    public  string s_NIC_DNSs;

        //    public NICStruct(string NIC_description, string NIC_name, string NIC_ip, string NIC_subnetMask, string NIC_defaultGateway, string NIC_metric, string NIC_DNSs)
        //    {
        //        this.s_NIC_description = NIC_description;
        //        this.s_NIC_name = NIC_name;
        //        this.s_NIC_ip = NIC_ip;
        //        this.s_NIC_subnetMask = NIC_subnetMask;
        //        this.s_NIC_defaultGateway = NIC_defaultGateway;
        //        this.s_NIC_metric = NIC_metric;
        //        this.s_NIC_DNSs = NIC_DNSs;
        //    }
        //}

        public cls_Workstations()
        {
            _ram = 1024;
            _cpu = 2.5f;
            _hdd = 500f;

            //_NIC1.connectionCheck_phase_1_Interval = 2 * 60 * 1000;
            //_NIC1.connectionCheck_phase_2_Interval = 2 * 60 * 1000;

            //_NIC2.connectionCheck_phase_1_Interval = 2 * 60 * 1000;
            //_NIC2.connectionCheck_phase_2_Interval = 5 * 60 * 1000;
        }

        /// <summary>
        /// 
        /// </summary>
        private string _OS;
        public string OS
        {
            get
            {
                return _OS;
            }
            set
            {
                _OS = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string _computerName;
        public string computerName
        {
            get
            {
                return _computerName;
            }
            set
            {
                _computerName = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string _ip;
        public string ip
        {
            get
            {
                return _ip;
            }
            set
            {
                _ip = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string _state;
        public string state
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private NICClass _NIC1;
        public NICClass NIC1
        {
            get
            {
                return _NIC1;
            }
            set
            {
                _NIC1 = value;
            }
        }

        private NICClass _NIC2;
        public NICClass NIC2
        {
            get
            {
                return _NIC2;
            }
            set
            {
                _NIC2 = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string _scanner1;
        public string scanner1
        {
            get
            {
                return _scanner1;
            }
            set
            {
                _scanner1 = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string _scanner2;
        public string scanner2
        {
            get
            {
                return _scanner2;
            }
            set
            {
                _scanner2 = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string _printer1;
        public string printer1
        {
            get
            {
                return _printer1;
            }
            set
            {
                _printer1 = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string _printer2;
        public string printer2
        {
            get
            {
                return _printer2;
            }
            set
            {
                _printer2 = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string _printer3;
        public string printer3
        {
            get
            {
                return _printer3;
            }
            set
            {
                _printer3 = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string _printer4;
        public string printer4
        {
            get
            {
                return _printer4;
            }
            set
            {
                _printer4 = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool _hasAutomation;
        public bool hasAutomation
        {
            get
            {
                return _hasAutomation;
            }
            set
            {
                _hasAutomation = value;
            }
        }

        /// <summary>
        /// RAM in MB
        /// </summary>
        private float _ram;
        public float RAM
        {
            get
            {
                return _ram;
            }
            set
            {
                _ram = value;
            }
        }

        /// <summary>
        /// CPU in GHz
        /// </summary>
        private float _cpu;
        public float CPU
        {
            get
            {
                return _cpu;
            }
            set
            {
                _cpu = value;
            }
        }

        /// <summary>
        /// H.D.D in GB
        /// </summary>
        private float _hdd;
        public float HDD
        {
            get
            {
                return _hdd;
            }
            set
            {
                _hdd = value;
            }
        }

        /// <summary>
        /// Monitor Name
        /// </summary>
        private float _monitorName;
        public float monitorName
        {
            get
            {
                return _monitorName;
            }
            set
            {
                _monitorName = value;
            }
        }
    }
}
