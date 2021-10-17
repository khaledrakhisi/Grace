using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace grace_soul
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            ServiceController controller = new ServiceController(serviceInstaller1.ServiceName);

            try
            {
                if (controller.Status == ServiceControllerStatus.Running | controller.Status == ServiceControllerStatus.Paused)
                {
                    controller.Stop();

                    controller.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 0, 15));

                    controller.Close();
                }
            }
            catch (Exception)
            {
                //string source = "My Service Installer";
                //string log = "Application";
                //if (!EventLog.SourceExists(source))
                //{
                //    EventLog.CreateEventSource(source, log);
                //}

                //EventLog eLog = new EventLog();
                //eLog.Source = source;
                //eLog.WriteEntry(string.Concat(@"The service could not be stopped. Please stop the service manually. Error: ", ex.Message), EventLogEntryType.Error);

            }
            finally
            {
                base.Uninstall(savedState);
            }
        }

        private void ServiceInstaller1_BeforeInstall(object sender, InstallEventArgs e)
        {
            ServiceController controller = new ServiceController(serviceInstaller1.ServiceName);

            if (controller.Status == ServiceControllerStatus.Running | controller.Status == ServiceControllerStatus.Paused)
            {

                controller.Stop();

                controller.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 0, 15));

                controller.Close();

            }

            base.OnBeforeUninstall(e.SavedState);
        }
    }
}
