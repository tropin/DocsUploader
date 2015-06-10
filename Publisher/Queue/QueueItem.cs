using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Parcsis.PSD.Publisher.Queue
{
    /// <summary>
    /// Элемент очереди на оповещение
    /// </summary>
    [DataContract]
    public class QueueItem
    {
        /// <summary>
        /// Полный путь до файла в очереди
        /// </summary>
        [DataMember]
        public string FileFullPath { get; set; }
        /// <summary>
        /// Имя файла
        /// </summary>
        [DataMember]
        public string FileName { get; set; }
        /// <summary>
        /// Расширение файла
        /// </summary>
        [DataMember]
        public string FileExtension { get; set; }

        /// <summary>
        /// Дополнительные параметры элемента очереди
        /// </summary>
        [DataMember]
        public Dictionary<string, string> Parameters { get; set; }

        /// <summary>
        /// Статус обработки элемента
        /// </summary>
        /// <remarks>
        /// Сбрасывается если загружается из хранилища на Queued
        /// </remarks>
        public ItemStatus Status { get; set; }

        /// <summary>
        /// Коллекция ошибок на задаче
        /// </summary>
        [DataMember]
        public Dictionary<string, int> Failures { get; set; }

        /// <summary>
        /// Коллекция пройденных обработчиков 
        /// </summary>
        [DataMember]
        public List<string> PassedProcessors { get; set; }
    }
}
