using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Parcsis.PSD.Publisher.Queue;
using System.Diagnostics;
using System.Net.Mail;
using Parcsis.PSD.Publisher.Properties;
using Parcsis.PSD.Publisher.SourceWatcher;

namespace Parcsis.PSD.Publisher.QueueProcessor
{
    public abstract class QueueProcessorBase
    {
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

        public event EventHandler<NeedDeleteArgs> NeedDelete;

        protected QueueProcessorBase(double interval)
		{
			_isProcessingNowSyncRoot = new object();
			_processTimer = new Timer(interval);
			_processTimer.Elapsed += ProcessingTimerElapsed;

		}

		public void Start()
		{
			_processTimer.Start();
		}

		public void Stop()
		{
			_processTimer.Stop();
		}


        private void Process(object sender, ElapsedEventArgs e)
        {
            lock (QueueHolder.SyncLock)
            {
                OnProcess(QueueHolder.Instance.Queue.Where(item => item.Status == ItemStatus.Queued));
                ProcessFinished(QueueHolder.Instance.Queue.Where(item => item.Status == ItemStatus.Processed));
                ProcessFailed(QueueHolder.Instance.Queue.Where(item => item.Status == ItemStatus.Failed));
            }
        }

        private void ProcessFailed(IEnumerable<QueueItem> iEnumerable)
        {
            List<QueueItem> qil = iEnumerable.ToList();
            foreach (var item in qil)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(ThreadProc, item);
                OnNeedDelete(item.FileFullPath);
            }
        }

        static void ThreadProc(object stateInfo)
        {
            QueueItem item = stateInfo as QueueItem;
            if (item != null)
            {
                Trace.WriteLine(string.Format("Ошибка публикации файла {0}", item.FileFullPath), Constants.TRACE_ERROR);
                ProcessAdminNotification(item);
            }
        }

        static void ProcessAdminNotification(QueueItem ditem)
        {
            SmtpClient mailer = new SmtpClient();
            MailMessage mess = new MailMessage();
            string[] mailTos = Settings.Default.AdminMails.Split(new [] {',',';',' '}, StringSplitOptions.RemoveEmptyEntries);
            foreach(string addr in mailTos)
                mess.To.Add(addr);
            mess.IsBodyHtml = false;
            mess.Subject = Settings.Default.AdminMailSubject;
            mess.Body = 
                string.Format(Settings.Default.AdminMailContent, ditem.FileFullPath, 
                    string.Join(Environment.NewLine, 
                        ditem.Failures.Select(item => string.Format("Имя: {0} Количество повторов: {1} ",item.Key, item.Value))));
            mailer.Send(mess); 
        }

        private void ProcessFinished(IEnumerable<QueueItem> iEnumerable)
        {
            List<QueueItem> qil = iEnumerable.ToList();
            foreach (var item in qil)
            {
                Trace.WriteLine(string.Format("Успешно опубликован файл {0}", item.FileFullPath), Constants.TRACE_INFORMATION);
                var ditem = QueueHolder.Instance.Queue.FirstOrDefault(qi=>qi.FileFullPath == item.FileFullPath);
                if (ditem != null)
                {
                    OnNeedDelete(ditem.FileFullPath);
                }
            }
        }
        
        protected abstract void OnProcess(IEnumerable<QueueItem> nowInQueue);

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
			finally
			{
				_isProcessingNow = false;
			}
		}

        protected virtual void OnNeedDelete(string fileName)
        {
            if (NeedDelete != null)
            {
                NeedDelete(this, new NeedDeleteArgs(fileName));
            }
        }
	}
}
