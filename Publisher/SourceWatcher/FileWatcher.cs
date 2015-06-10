using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parcsis.PSD.Publisher.Configurations;
using System.IO;
using System.Diagnostics;

namespace Parcsis.PSD.Publisher.SourceWatcher
{
    public class FileWatcher: IDisposable
    {
        private Dictionary<FileSystemWatcher, WatchElement> _watchers = new Dictionary<FileSystemWatcher, WatchElement>();

        private bool _disposed;

        /// <summary>
        /// Возникла ошибка отслеживания
        /// </summary>
        public event EventHandler<ErrorArgs> Error;
        /// <summary>
        /// Требуется действие
        /// </summary>
        public event EventHandler<NeedActionArgs> NeedAction;

        /// <summary>
        /// Надо выкинуть с очереди. Файл пропал
        /// </summary>
        public event EventHandler<NeedDeleteArgs> Delete;
        
        public FileWatcher(IList<WatchElement> watchPoints)
        {
            _disposed = false;
            foreach (WatchElement we in watchPoints)
            {
                try
                {
                    FileSystemWatcher fsw = new FileSystemWatcher(we.SourcePath, we.FileMask);
                    fsw.IncludeSubdirectories = we.IncludeSubFolders;
                    fsw.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
                    fsw.Created += new FileSystemEventHandler(HandleCreated);
                    fsw.Error += new ErrorEventHandler(HandleError);
                    fsw.Changed += new FileSystemEventHandler(HandleChanged);
                    fsw.Renamed += new RenamedEventHandler(Renamed);
                    fsw.Deleted += new FileSystemEventHandler(Deleted);
                    _watchers.Add(fsw, we);
                }
                catch(Exception ex)
                {
                    Trace.WriteLine(string.Format("Невозможно запустить наблюдатель по пути {0}, Ошибка: {1}", we.SourcePath, ex), Constants.TRACE_WARNING);
                }
            }
            if (_watchers.Count == 0)
                throw new InvalidOperationException("Нет объектов для наблюдения или все наблюдатели не смогли инициализироваться");
        }

        private void Deleted(object sender, FileSystemEventArgs e)
        {
            OnDelete(new NeedDeleteArgs(e.FullPath));
        }

        private void Renamed(object sender, RenamedEventArgs e)
        {
            OnDelete(new NeedDeleteArgs(e.OldFullPath));
            FireNeedAction(sender, e.FullPath);
        }

        public void Start()
        {
            if (!_disposed)
            {
                foreach (var fswkvp in _watchers)
                {
                   fswkvp.Key.EnableRaisingEvents = true;
                }
            }
        }

        public void Stop ()
        {
            if (!_disposed)
            {
                foreach (var fswkvp in _watchers)
                {
                    fswkvp.Key.EnableRaisingEvents = false;
                }
            }
        }

        private void FireNeedAction(object sender, string fileFullPath)
        {
             FileSystemWatcher fsw = sender as FileSystemWatcher;
             WatchElement el = _watchers[fsw];
             OnNeedAction(new NeedActionArgs(fileFullPath, el.SourcePath, el.FileNameParamRegExExpression));
        }
        
        private void HandleChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Renamed)
            {
                FireNeedAction(sender, e.FullPath);  
            }
        }

        private void HandleError(object sender, ErrorEventArgs e)
        {
            ErrorArgs err = new ErrorArgs(e.GetException());
            OnError(err);
        }

        private void HandleCreated(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                FireNeedAction(sender, e.FullPath);
            }
        }

        protected virtual void OnError(ErrorArgs args)
        {
            if (Error!=null)
            {
                Error(this, args);
            }
        }

        protected virtual void OnNeedAction(NeedActionArgs args)
        {
            if (NeedAction != null)
            {
                NeedAction(this, args);
            }
        }

        protected virtual void OnDelete(NeedDeleteArgs args)
        {
            if (Delete != null)
            {
                Delete(this, args);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_watchers != null)
                    {
                        foreach (var watcher in _watchers)
                        {
                            _watchers.Remove(watcher.Key);
                            watcher.Key.EnableRaisingEvents = false;
                            watcher.Key.Dispose();
                        }
                    }
                    Trace.WriteLine("Наблюдатели уничтожены", Constants.TRACE_INFORMATION);
                }
                _watchers = null;
                _disposed = true;
            }
        }

        #endregion
    }
}
