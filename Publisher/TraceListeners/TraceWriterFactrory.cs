using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace Parcsis.PSD.Publisher.TraceListeners
{
    /// <summary>
    /// Фабрика Writer-ов для логгинрования
    /// </summary>
    internal class TraceWriterFactrory
    {
        private static SyncDictionary<String, WriterChunk> _writerCollection = new SyncDictionary<String, WriterChunk>();
        private TimeSpan _rollTime;
        private Encoding _encoding;
        private string _dateFormat;

        public TraceWriterFactrory(TimeSpan rollTime, Encoding encoding, string dateFormat)
        {
            _rollTime = rollTime;
            _encoding = encoding;
            _dateFormat = dateFormat;
        }

        public WriterChunk GetWriterChunk(string filePath)
        {
            WriterChunk chunk = null;
            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    DateTime now = DateTime.Now;
                    if (_writerCollection.Contains(filePath)) //Если мы уже пишем сюда
                    {
                        chunk = _writerCollection[filePath];
                        Monitor.Enter(chunk.InternalSyncLock);
                        if (chunk.ConnectedWriter == null)
                            chunk.InitChunk(filePath, _encoding);                        
                    }
                    else //Если еще не пишем, то надо отметиться, что собираемся
                    {
                        Monitor.Enter(
                        _writerCollection.Add
                         (
                                 filePath,
                                 new WriterChunk()
                                    {
                                        InternalSyncLock = new object(),
                                        SyncLock = new object()
                                    }
                         ).InternalSyncLock);
                        chunk = _writerCollection[filePath];
                        //Если файла нет, то он создастся, если есть, то будем дописывать
                        chunk.InitChunk(filePath, _encoding);
                    }
                    //Дальше надо определиться с архивацией
                    if (chunk != null && _rollTime < (now - chunk.DateCreated)) //Если пора архивировать, то архивируем и переопределяем Writer
                    {
                        chunk.Acrchivate(now, _dateFormat);
                    }
                }
                catch(Exception)
                {
                    //Логгирование упало :-) ну а чего тут сделаешь?
                }
                finally
                {
                    Monitor.Exit(_writerCollection[filePath].InternalSyncLock);
                }
            }
            return chunk;
        }
    }
}
