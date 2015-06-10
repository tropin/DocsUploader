using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

namespace Parcsis.PSD.Publisher.Queue
{
	public static class XmlStorage<T>
		where T : BaseXmlSaveable, new()
	{
		private static DataContractSerializer _serializer = new DataContractSerializer(typeof(T));

		public static void Save(T value, string fileName)
		{
            value.LastActionTime = DateTime.Now;
            using (StreamWriter writer = new StreamWriter(fileName))
			{
				_serializer.WriteObject(writer.BaseStream, value);
			}
		}

		public static T Load(string fileName)
		{
			T result;
			if (File.Exists(fileName))
			{
				using (StreamReader reader = new StreamReader(fileName))
				{
					result = (T)_serializer.ReadObject(reader.BaseStream);
				}
			}
			else
			{
				result = new T();
			}
			result.NeedUpdateProperties = true;
			return result;
		}
	}
}
