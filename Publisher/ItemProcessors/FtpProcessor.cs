using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parcsis.PSD.Publisher.QueueProcessor;
using Parcsis.PSD.Publisher.FTPClient;
using Parcsis.PSD.Publisher.Properties;
using System.Threading;
using Parcsis.PSD.Publisher.TraceListeners;

namespace Parcsis.PSD.Publisher.ItemProcessors
{
    public class FtpProcessor : IQueueElementProcessor
    {
        private static SyncDictionary<string, Semaphore> _connLocks = new SyncDictionary<string, Semaphore>();
        private static object _syncRoot = new object();
        #region IQueueElementProcessor Members

        public bool Process(Queue.QueueItem item)
        {
            string ftpHost = Settings.Default.FtpHost;
            Semaphore lockOne;
            lock (_syncRoot)
            {
                if (!_connLocks.ContainsKey(ftpHost))
                {
                    int semaphorePoolSize = Settings.Default.ConcurentFTPConnectionsCount;
                    lockOne = new Semaphore(0, semaphorePoolSize);
                    lockOne.Release(semaphorePoolSize);
                    _connLocks.Add(ftpHost, lockOne);

                }
                else
                {
                    lockOne = _connLocks[ftpHost];
                }
            }
            lockOne.WaitOne();
            try
            {
                bool result = false;
                FTPConnection conn = new FTPConnection();
                conn.Open(Settings.Default.FtpHost, Settings.Default.FtpUserName, Settings.Default.FtpPassword, FTPMode.Passive);
                conn.SetCurrentDirectory(Settings.Default.FtpTargetFolder);
                conn.SendFile(item.FileFullPath, FTPFileTransferType.Binary);
                conn.Close();
                result = true;
                return result;
            }
            finally
            {
                lockOne.Release();
            }
        }

        public string TransactionGroup
        {
            get { return "VASSite"; }
        }

        public int Priority
        {
            get { return 1; }
        }

        #endregion
    }
}
