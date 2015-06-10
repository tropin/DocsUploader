using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using Parcsis.PSD.Publisher.Properties;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Parcsis.PSD.Publisher.Queue
{
    public class QueueHolder : BaseXmlSaveable
	{
		#region Fields

        private static object _syncLock = new object();
        private BindingList<QueueItem> _queue;
        

		#endregion

		#region Properties

        public static object SyncLock
        {
            get
            {
                return _syncLock;
            }
        }

        public BindingList<QueueItem> Queue
        {
            get
            {
                if (_queue == null)
                {
                    _queue = new BindingList<QueueItem>();
                    _queue.ListChanged += new ListChangedEventHandler(OnPropertyChanged);
                }
                return _queue;
            }
        }
		 
		#endregion

		#region Singleton

        private static QueueHolder _instance;

        public static QueueHolder Instance
		{
			get
			{
				if (_instance == null)
				{
                    _instance = XmlStorage<QueueHolder>.Load(Settings.Default.StateHolderFileName);
				}
				return _instance;
			}
		}

		#endregion

		#region Serializing

		public void ForceSave()
		{
            XmlStorage<QueueHolder>.Save(this, Settings.Default.StateHolderFileName);
		}

        protected void OnPropertyChanged(object sender, ListChangedEventArgs e)
		{
			if (NeedUpdateProperties)
			{
				XmlStorage<QueueHolder>.Save(this, Settings.Default.StateHolderFileName);
			}
		}

		#endregion
	}
}
