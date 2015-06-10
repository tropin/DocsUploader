using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Parcsis.PSD.Publisher.TraceListeners
{
    internal class WriterChunk
    {
        private string _filePath = string.Empty;
        private Encoding _encoding = Encoding.UTF8;
        public TextWriter ConnectedWriter { get; private set; }
        public DateTime DateCreated { get; private set; }
        public object InternalSyncLock { get; set; }
        public object SyncLock { get; set; }

        private object archiveSyncLock = new object();

        private void bkg_Archive(object filenamePair)
        {
            KeyValuePair<string, string> pair = (KeyValuePair<string, string>)filenamePair;
            string fileName = pair.Key;
            string newFileName = pair.Value;
            string zipfileName = Path.ChangeExtension(fileName, "zip");
            string zipEntryName = Path.GetFileName(newFileName);
            lock (archiveSyncLock)
            {
                zipEntryName = GetNewZipEntryFileName(fileName, zipEntryName, 1, zipfileName, null);
                Dictionary<string, string> zipItems = new Dictionary<string, string>();
                if (File.Exists(zipfileName))
                {
                    ZipFile zip = new ZipFile(zipfileName);
                    IEnumerable<ZipEntry> zes = zip.Cast<ZipEntry>();
                    foreach (ZipEntry zie in zes)
                    {
                        zipItems.Add(
                            zie.Name,
                            new StreamReader(
                            zip.GetInputStream(zie)).ReadToEnd());
                    }
                    zip.Close();
                }
                FileStream fs = File.Open(zipfileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);                
                ZipOutputStream zos = new ZipOutputStream(fs);
                //Считываем все из старого файла
                byte[] content = File.ReadAllBytes(newFileName);
                //Создаем файлы в архиве
                foreach (var item in zipItems)
                {
                    ZipEntry ze = new ZipEntry(item.Key);
                    zos.PutNextEntry(ze);
                    byte[] itemContent = Encoding.Default.GetBytes(item.Value);
                    zos.Write(itemContent, 0, itemContent.Length);
                }
                ZipEntry zipe = new ZipEntry(zipEntryName);
                zos.PutNextEntry(zipe);
                zos.Write(content, 0, content.Length);
                zos.Flush();
                zos.Close();
                zos.Dispose();
                fs.Close();
                fs.Dispose();
                if (File.Exists(newFileName))
                    File.Delete(newFileName);
            }
        }

        /// <summary>
        /// Заархивировать кусок
        /// </summary>
        /// <param name="archiveDate">Дата архивацц</param>
        /// <param name="dateFormat">Формат даты</param>
        public void Acrchivate(DateTime archiveDate, string dateFormat)
        {
            //Закрываем Writer
            this.ConnectedWriter.Flush();
            this.ConnectedWriter.Close();
            this.ConnectedWriter.Dispose();
            string orginalNewFileName = Path.Combine(Path.GetDirectoryName(_filePath), string.Format("{0}_{1}", archiveDate.ToString(dateFormat), Path.GetFileName(_filePath)));
            string newFileName = GetFirstFileName(orginalNewFileName, orginalNewFileName, 1);
            File.Move(_filePath, newFileName);
            //Создаем архив.
            System.Threading.ThreadPool.QueueUserWorkItem(bkg_Archive, new KeyValuePair<string, string>(orginalNewFileName, newFileName));
            InitChunk(_filePath, _encoding);
        }

        private string GetFirstFileName(string fileName,string newFileName, int deep)
        {
            if (File.Exists(newFileName))
            {
                newFileName = Path.Combine(Path.GetDirectoryName(fileName),string.Format("{0}_{1}{2}", Path.GetFileNameWithoutExtension(fileName), deep, Path.GetExtension(fileName)));
                newFileName = GetFirstFileName(fileName, newFileName, deep + 1);
            }
            return newFileName;
        }


        private static string GetNewZipEntryFileName(string fileName, string newFileName, int deep, string zipfileName, ZipFile zip)
        {
            if (File.Exists(zipfileName))
            {
                if (zip == null)
                    zip = new ZipFile(zipfileName);
                if (zip.Cast<ZipEntry>().Any(c => c.Name == newFileName))
                {
                    newFileName = string.Format("{0}_{1}{2}", Path.GetFileNameWithoutExtension(fileName), deep, Path.GetExtension(fileName));
                    newFileName = GetNewZipEntryFileName(fileName, newFileName, deep + 1, zipfileName, zip);
                }
                if (deep == 1)
                    zip.Close();
            }
            return newFileName;
        }

        /// <summary>
        /// Инициализация кусочка
        /// </summary>
        /// <param name="filePath">Путь до куска</param>
        /// <param name="encoding">Кодировка файла</param>
        public void InitChunk(string filePath, Encoding encoding)
        {
            _filePath = filePath;
            _encoding = encoding;
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            this.DateCreated = DateTime.Now; 
            this.ConnectedWriter = new StreamWriter(filePath, true, encoding);
        }
    }
}
