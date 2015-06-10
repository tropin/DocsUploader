using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parcsis.PSD.Publisher.Configurations
{
	public class WatchSection : ConfigurationSection
	{
		[ConfigurationProperty("Watches", IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(WatchElementCollection), AddItemName = "add", ClearItemsName = "clear")]
        public WatchElementCollection Watches
		{
            get { return (WatchElementCollection)this["Watches"]; }
            set { this["Watches"] = value; }
		}

		public static IList<WatchElement> GetActiveWatches()
		{
            List<WatchElement> result = new List<WatchElement>();
            WatchSection config = (WatchSection)ConfigurationManager.GetSection("WatchSection");
			if (config != null)
			{
                foreach (WatchElement watch in config.Watches)
				{
                    if (!string.IsNullOrEmpty(watch.SourcePath))
					{
                        result.Add(watch);
					}
				}
			}
			return result;
		}
	}
}
