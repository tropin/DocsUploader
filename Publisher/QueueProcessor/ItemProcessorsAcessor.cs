using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parcsis.PSD.Publisher.ItemProcessors;

namespace Parcsis.PSD.Publisher.QueueProcessor
{
    public class ItemProcessorsAcessor
    {
        public static List<IQueueElementProcessor> GetProcessors()
        {
            return new List<IQueueElementProcessor>() {new FtpProcessor() , new HttpGetProcessor()} ;
        }
    }
}
