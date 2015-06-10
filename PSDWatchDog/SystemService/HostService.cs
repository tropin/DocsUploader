using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using Parcsis.PSD.Publisher.Properties;

namespace Parcsis.PSD.Publisher.SystemService
{
	class HostService : ServiceBase
	{
		[DllImport("ADVAPI32.DLL", EntryPoint = "SetServiceStatus")]
		private static extern bool SetServiceStatus(IntPtr hServiceStatus, ServiceStatus lpServiceStatus);

        private WatchDog _wd;

		public HostService()
		{
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            AutoLog = true;
			ServiceName = HostServiceInstaller.ServiceName;
			CanShutdown = true;
			CanStop = true;
            _wd = new WatchDog(Settings.Default.BeatInterval.TotalMilliseconds);
		}


		private void SetServiceStatus(ServiceState serviceState)
		{
			ServiceStatus serviceStatus = new ServiceStatus { currentState = (int)serviceState };

			SetServiceStatus(ServiceHandle, serviceStatus);
		}


		protected override void OnStart(string[] args)
		{
			SetServiceStatus(ServiceState.SERVICE_START_PENDING);
			try
			{
                _wd.Start();
			}
			catch (Exception ex)
			{
				Trace.WriteLine(string.Format("Сервис не смог запуститься по причине ошибки: {0}",ex.ToString()));
                throw;
			}
			SetServiceStatus(ServiceState.SERVICE_RUNNING);
		}

		protected override void OnStop()
		{
			SetServiceStatus(ServiceState.SERVICE_STOP_PENDING);

			try
			{
                _wd.Stop();
			}
			catch (Exception ex)
			{
				Trace.WriteLine(string.Format("Сервис не смог корректно завершиться по причине ошибки: {0}",ex.ToString()));
                throw;
			}
			SetServiceStatus(ServiceState.SERVICE_STOPPED);
		}
	}
}