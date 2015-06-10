using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parcsis.PSD.Publisher.Configurations
{
	public class WatchElementCollection : ConfigurationElementCollection
	{
		public override ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return ConfigurationElementCollectionType.AddRemoveClearMap;
			}
		}

		public WatchElement this[int index]
		{
            get { return (WatchElement)BaseGet(index); }
			set
			{
				if (BaseGet(index) != null)
					BaseRemoveAt(index);
				BaseAdd(index, value);
			}
		}

        public void Add(WatchElement element)
		{
			BaseAdd(element);
		}

		public void Clear()
		{
			BaseClear();
		}

        public void Remove(WatchElement element)
		{
            BaseRemove(element.SourcePath);
		}

		public void Remove(string name)
		{
			BaseRemove(name);
		}

		public void RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}

		protected override ConfigurationElement CreateNewElement()
		{
            return new WatchElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
            return ((WatchElement)element).SourcePath;
		}
	}
}
