using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Windows.Forms;
using Parcsis.PSD.Publisher.SourceWatcher;

namespace Parcsis.PSD.Publisher.SystemService
{
	class HostService : ServiceBase
	{
        private ServiceHost<HeartBeat.HeartBeatService> _hbService;
        private QueueProcessor.QueueProcessorBase _processor;
        private SourceWatcher.FileWatcher _fwatcher;

		[DllImport("ADVAPI32.DLL", EntryPoint = "SetServiceStatus")]
		private static extern bool SetServiceStatus(IntPtr hServiceStatus, ServiceStatus lpServiceStatus);

		public HostService()
		{
			AutoLog = true;
			ServiceName = HostServiceInstaller.ServiceName;
			CanShutdown = true;
			CanStop = true;
		}

        public HostService(ServiceHost<HeartBeat.HeartBeatService> hbService, QueueProcessor.QueueProcessorBase processor, FileWatcher fwatcher)
            : this()
        {
            System.IO.Directory.SetCurrentDirectory(Application.StartupPath);
            this._hbService = hbService;
            this._processor = processor;
            this._fwatcher = fwatcher;
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
                if (!(_hbService.State == System.ServiceModel.CommunicationState.Opened))
                {
                    _hbService.Open();
                }
                _processor.Start();
                _fwatcher.Start();
			}
			catch (Exception ex)
			{
				Trace.WriteLine(string.Format("Сервис не смог запуститься по причине ошибки: {0}",ex.ToString()), Constants.TRACE_ERROR);
                throw;
			}
			SetServiceStatus(ServiceState.SERVICE_RUNNING);
		}

		protected override void OnStop()
		{
			SetServiceStatus(ServiceState.SERVICE_STOP_PENDING);

			try
			{
                _hbService.Close();
                _processor.Stop();
                _fwatcher.Stop();
			}
			catch (Exception ex)
			{
				Trace.WriteLine(string.Format("Сервис не смог корректно завершиться по причине ошибки: {0}",ex.ToString()), Constants.TRACE_ERROR);
                throw;
			}
			SetServiceStatus(ServiceState.SERVICE_STOPPED);
		}
	}
}