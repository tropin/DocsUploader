using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.ServiceProcess;
using Parcsis.PSD.Publisher.Properties;
using System.Diagnostics;

namespace Parcsis.PSD.Publisher
{
    public class WatchDog
    {

        WcfNamedPipeServiceClient<Parcsis.PSD.Publisher.HeartBeat.IHeartBeatService> _watchdogServiceClient;
        
        private ServiceController _controller;
        
        /// <summary>
		/// Поле для обеспечения реентрабельности проверки новых элементов в очереди.
		/// </summary>
		private bool _isProcessingNow;

		/// <summary>
		/// Объект синхронизации для корректного доступа к <see cref="_isProcessingNow"/>.
		/// </summary>
		private readonly object _isProcessingNowSyncRoot;

		/// <summary>
		/// Таймер обработки очереди на проверку присутствия документов во внешней системе
		/// </summary>
		private readonly Timer _processTimer;

        public WatchDog(double interval)
		{
			_isProcessingNowSyncRoot = new object();
			_processTimer = new Timer(interval);
			_processTimer.Elapsed += ProcessingTimerElapsed;
            _controller = new ServiceController();
            _controller.MachineName = Settings.Default.ServiceMachine;
            _controller.ServiceName = Settings.Default.ServiceName;
            InitBeatService();
		}

        private void InitBeatService()
        {
            _watchdogServiceClient = new WcfNamedPipeServiceClient<Parcsis.PSD.Publisher.HeartBeat.IHeartBeatService>(
            Settings.Default.BeatServiceAddress);
            _watchdogServiceClient.ClientChannel.Faulted += new EventHandler(ClientChannel_Faulted);
        }

        void ClientChannel_Faulted(object sender, EventArgs e)
        {
            //_watchdogServiceClient.ClientChannel.Close();
            RestartService();
        }

		public void Start()
		{
            RestartService();
            _processTimer.Start();
		}

		public void Stop()
		{
			_processTimer.Stop();
            _watchdogServiceClient.ClientChannel.Close();
		}


        private void Process(object sender, ElapsedEventArgs e)
        {
            if (_watchdogServiceClient.ClientChannel.State != System.ServiceModel.CommunicationState.Opening)
            {
                _watchdogServiceClient.Channel.GetProcessingItemsCount();
            }
        }

		/// <summary>
		/// Метод обработчик таймера обработки билдерами новых готовых документов.
		/// </summary>        
		private void ProcessingTimerElapsed(object sender, ElapsedEventArgs e)
		{
			lock (_isProcessingNowSyncRoot)
			{
				if (_isProcessingNow)
					return;

				_isProcessingNow = true;
			}
            try
            {
                Process(sender, e);
            }
            catch
            {
                Trace.WriteLine(string.Format("Наблюдаемый сервис {0} подвис или был выключен, перезапускаем", Settings.Default.ServiceName));
                RestartService();
            }
			finally
			{
				_isProcessingNow = false;
			}
		}

        private void RestartService()
        {
            _controller.Refresh();
            if (!new [] {ServiceControllerStatus.Stopped, 
                         ServiceControllerStatus.StartPending
                         }.Contains(_controller.Status))
            {
                if (_controller.Status == ServiceControllerStatus.PausePending)
                    _controller.WaitForStatus(ServiceControllerStatus.Paused);
                _controller.Stop();
            }
            if (_controller.Status == ServiceControllerStatus.StopPending)
                _controller.WaitForStatus(ServiceControllerStatus.Stopped);
            if (!new[] { ServiceControllerStatus.Running, 
                         ServiceControllerStatus.StartPending }.Contains(_controller.Status))
            {
                _controller.Start();
            }
            _controller.WaitForStatus(ServiceControllerStatus.Running);
            InitBeatService();
        }
    }
}
