using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Parcsis.PSD.Publisher.HeartBeat
{
    [ServiceContract]
    public interface IHeartBeatService
    {
        [OperationContract]
        int GetProcessingItemsCount();
    }
}
