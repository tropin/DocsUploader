using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parcsis.PSD.Publisher.Queue
{
    /// <summary>
    /// Статус элемента очереди
    /// </summary>
    public enum ItemStatus
    {
        /// <summary>
        /// В очереди
        /// </summary>
        Queued,
        /// <summary>
        /// В процессе обработки
        /// </summary>
        InProcess,
        /// <summary>
        /// Провалена
        /// </summary>
        Failed,
        /// <summary>
        /// Обработана
        /// </summary>
        Processed
    }
}
