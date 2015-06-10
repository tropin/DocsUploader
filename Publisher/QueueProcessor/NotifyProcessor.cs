using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parcsis.PSD.Publisher.Queue;
using System.Diagnostics;
using Parcsis.PSD.Publisher.Properties;

namespace Parcsis.PSD.Publisher.QueueProcessor
{
    public class NotifyProcessor: QueueProcessorBase
    {
        private class WorkItem
        {
            public QueueItem QueueItem { get; set; }
            public IEnumerable<IGrouping<string, IQueueElementProcessor>> Processors { get; set; }
        }
        
        public NotifyProcessor(long interval) : base(interval) { }
        
        private List<IQueueElementProcessor> _processors = ItemProcessorsAcessor.GetProcessors();
        
        protected override void OnProcess(IEnumerable<QueueItem> nowInQueue)
        {
            var processorGroups = _processors.GroupBy(processor => processor.TransactionGroup)
              .OrderBy(group => group.Max(processor => processor.Priority));
            foreach (QueueItem qi in nowInQueue)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(ThreadProc, new WorkItem() { QueueItem = qi, Processors = processorGroups });
            }
        }

        static void ThreadProc(object stateInfo)
        {
            WorkItem wi = stateInfo as WorkItem;
            bool isAllGood = true;
            QueueItem qi = wi.QueueItem;
            qi.Status = ItemStatus.InProcess;
            foreach (var group in  wi.Processors)
            {
                var processors = group.OrderByDescending(processor => processor.Priority);
                foreach (IQueueElementProcessor processor in processors.Where(prc => !qi.PassedProcessors.Contains(prc.GetType().FullName)))
                {
                    try
                    {
                        if (processor.Process(qi))
                        {
                            qi.PassedProcessors.Add(processor.GetType().FullName);
                        }
                        else
                        {
                            throw new Exception(string.Format("Управлемое исключение при обрабоке процессора {0}", processor.GetType().Name));
                        }
                    }
                    catch (Exception ex)
                    {
                        isAllGood = false;
                        string failureKey = string.Format("{0}_{1}", group.Key, processor.GetType().Name);
                        if (qi.Failures.ContainsKey(failureKey))
                        {
                            qi.Failures[failureKey] += 1;
                        }
                        else
                        {
                            qi.Failures.Add(failureKey, 1);
                        }
                        Trace.WriteLine(string.Format("Ошибка обработки элемента очереди {0},  обработчик {1}, ошибка {2}", qi.FileName, failureKey, ex), Constants.TRACE_ERROR);
                        if (!string.IsNullOrEmpty(group.Key)) //Транзакционная группа должны выполниться все иначе все счиатется фейлом
                        {
                            break;
                        }
                    }
                }
            }
            if (isAllGood)
                qi.Status = ItemStatus.Processed;
            else
            {
                if (qi.Failures.Any(item => item.Value >= Settings.Default.RetryCountPerProcessor))
                {
                    qi.Status = ItemStatus.Failed;
                }
                else
                {
                    //Продолжим в следующий заход
                    qi.Status = ItemStatus.Queued;
                }
            }   
        }

       

    }
}
