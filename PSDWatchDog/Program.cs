using Parcsis.PSD.Publisher.Properties;
using Parcsis.PSD.Publisher.SystemService;
using System;
using System.ServiceProcess;

namespace Parcsis.PSD.Publisher
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ServiceBase.Run(new ServiceBase[] { new HostService() });
            //WatchDog _wd = new WatchDog(Settings.Default.BeatInterval.TotalMilliseconds);
            //_wd.Start();
            //Console.ReadKey();
        }
    }
}
