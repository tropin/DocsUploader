using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Parcsis.PSD.Publisher.Configurations;
using Parcsis.PSD.Publisher.SourceWatcher;
using System.Diagnostics;
using Parcsis.PSD.Publisher.Queue;
using System.IO;
using System.Text.RegularExpressions;
using System.ServiceModel;
using Parcsis.PSD.Publisher.HeartBeat;
using Parcsis.PSD.Publisher.QueueProcessor;
using Parcsis.PSD.Publisher.Properties;
using System.ServiceProcess;
using Parcsis.PSD.Publisher.SystemService;

namespace Parcsis.PSD.Publisher
{
    static class Program
    {
        private static PublishQueueWindow _qwindow;
        private static ServiceHost<HeartBeatService> _hbService = null;
        private static QueueProcessorBase _processor = null;
        private static FileWatcher _fwatcher = null;
        private static bool _winMode = false;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main(string[] arg)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Init();
            _qwindow = new PublishQueueWindow();
            if (arg.Contains("-win"))
            {
                _winMode = true;
                Application.Run(_qwindow);
            }
            else
            {
                ServiceBase.Run(new ServiceBase[] { new HostService(_hbService, _processor, _fwatcher) });
            }
        }


        private static void Init()
        {
            try
            {
                _hbService = new ServiceHost<HeartBeatService>();
                _hbService.Open();
                IList<WatchElement> elements = WatchSection.GetActiveWatches();
                EnqueueMissedFiles(elements);
                _fwatcher = new FileWatcher(elements);
                _fwatcher.Error += new EventHandler<ErrorArgs>(Error);
                _fwatcher.NeedAction += new EventHandler<NeedActionArgs>(NeedAction);
                _fwatcher.Delete += new EventHandler<NeedDeleteArgs>(Delete);
                _fwatcher.Start();
                _processor = new NotifyProcessor(Settings.Default.QueueProcessorInterval);
                _processor.NeedDelete += new EventHandler<NeedDeleteArgs>(Delete);
                _processor.Start(); 
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString(), Constants.TRACE_ERROR);
            }                       
        }

        private static void EnqueueMissedFiles(IList<WatchElement> elements)
        {
            List<DateTime> fil = new List<DateTime>();
            lock (QueueHolder.SyncLock)
            {
                int i = 0;
                while(i<QueueHolder.Instance.Queue.Count)
                {
                    QueueItem qi = QueueHolder.Instance.Queue[i];
                    if (File.Exists(qi.FileFullPath))
                    {
                        fil.Add(File.GetLastWriteTime(qi.FileFullPath));
                        i++;
                    }
                    else
                    {
                        QueueHolder.Instance.Queue.Remove(qi);
                    }
                }
            }
            fil.Add(QueueHolder.Instance.LastActionTime);

            DateTime maxDate = fil.Max();

            foreach (WatchElement element in elements)
            {
                if (Directory.Exists(element.SourcePath))
                {
                    var filesNeedToEnqueue = Directory.GetFiles(element.SourcePath).Select(fileName => new {FileName = fileName, LastWriteTime = File.GetLastWriteTime(fileName)}).Where(file => file.LastWriteTime > maxDate);
                    foreach (var enqueuingFile in filesNeedToEnqueue)
                    {
                        NeedActionArgs args = new NeedActionArgs(enqueuingFile.FileName, element.SourcePath, element.FileNameParamRegExExpression);
                        NeedAction(null, args); 
                    }
                }               
            }
        }

        private static void Delete(object sender, NeedDeleteArgs e)
        {
            lock (QueueHolder.SyncLock)
            {
                QueueItem qi = QueueHolder.Instance.Queue.FirstOrDefault(el => el.FileFullPath == e.FullName);
                if (qi != null)
                {
                    MethodInvoker delItem = (MethodInvoker)delegate
                    {
                        QueueHolder.Instance.Queue.Remove(qi);
                    };
                    if (_winMode)
                        _qwindow.Invoke(delItem);
                    else
                        delItem();
                }
            }
        }

        private static void NeedAction(object sender, NeedActionArgs e)
        {
            if (File.Exists(e.FilePath))
            {
                string fileName = Path.GetFileNameWithoutExtension(e.FilePath);
                Dictionary<string, int> errors = new Dictionary<string, int>();
                List<string> processed = new List<string>();
                Dictionary<string, string> itemParams = new Dictionary<string,string>();
                try
                {
                    Regex regex = new Regex(e.NameExpression, RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    string[] groups = regex.GetGroupNames();
                    MatchCollection matches = regex.Matches(fileName);
                    foreach (string group in groups)
                    {
                        string value = string.Empty;
                        foreach (Match match in matches)
                        {
                            Group matchGroup = match.Groups[group];
                            if (matchGroup.Success)
                            {
                                foreach (Capture capture in matchGroup.Captures)
                                {
                                    value += capture.Value;
                                }
                            }
                        }                        
                        itemParams.Add(group, value);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("Параметры элемента не были извлечены по причине ошибки: {0}", ex), Constants.TRACE_WARNING);
                }

                MethodInvoker addElement = (MethodInvoker)delegate
                    {
                        if (!QueueHolder.Instance.Queue.Any(item => item.FileFullPath == e.FilePath))
                        {
                            QueueHolder.Instance.Queue.Add
                            (
                                new QueueItem()
                                {
                                    FileFullPath = e.FilePath,
                                    FileExtension = Path.GetExtension(e.FilePath),
                                    FileName = fileName,
                                    Status = ItemStatus.Queued,
                                    Parameters = itemParams,
                                    Failures = errors,
                                    PassedProcessors = processed
                                }
                            );
                        }
                    };

                lock (QueueHolder.SyncLock)
                {
                    if (_winMode)
                    {
                        _qwindow.Invoke(addElement);
                    }
                    else
                        addElement();
                }
            }
        }

        private static void Error(object sender, ErrorArgs e)
        {
            Trace.WriteLine(e.Exception.ToString(), Constants.TRACE_WARNING);
        }
    }
}
