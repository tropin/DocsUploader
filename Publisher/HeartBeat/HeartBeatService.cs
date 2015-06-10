using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Parcsis.PSD.Publisher.Queue;

namespace Parcsis.PSD.Publisher.HeartBeat
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class HeartBeatService: IHeartBeatService
    {
        #region IHeartBeatService Members

        public int GetProcessingItemsCount()
        {
            lock (QueueHolder.SyncLock)
            {
                return QueueHolder.Instance.Queue.Count(el => el.Status == ItemStatus.InProcess);
            }
        }

        #endregion
    }
}
