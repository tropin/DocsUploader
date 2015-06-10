using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parcsis.PSD.Publisher.Queue;

namespace Parcsis.PSD.Publisher.QueueProcessor
{
    public interface IQueueElementProcessor
    {
        /// <summary>
        /// Обработка элемента очереди
        /// </summary>
        /// <param name="item">Элемент очереди</param>
        /// <returns>Успешность выполнения</returns>
        bool Process(QueueItem item);
        /// <summary>
        /// Имя транзакционной группы обработчиков
        /// </summary>
        string TransactionGroup { get; }
        /// <summary>
        /// Приоритет обработчика
        /// </summary>
        int Priority { get; }
    }
}
