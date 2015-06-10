using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Parcsis.PSD.Publisher.Queue
{
	[DataContract]
	public class BaseXmlSaveable
	{
        private bool _needUpdateProperties = false;
        private DateTime _lastActionDate = DateTime.MinValue;

        [DataMember]
        public DateTime LastActionTime
        {
            get
            {
                return _lastActionDate;
            }
            set
            {
                _lastActionDate = value;
            }
        }

		public bool NeedUpdateProperties
		{
			get { return _needUpdateProperties; }
			internal set { _needUpdateProperties = value; }
		}
	}
}
