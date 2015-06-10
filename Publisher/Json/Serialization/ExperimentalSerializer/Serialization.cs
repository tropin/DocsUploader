using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Parcsis.PSD.Publisher.Common.Json.Serialization
{
    public class Serialization<ReturnType>
    {
        private string _serializeData;
        private ReturnType _deserializeData;

        public Serialization<ReturnType> SerializeXML(ReturnType oItems)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                XmlSerializer serializer = new XmlSerializer(oItems.GetType());
                serializer.Serialize(stream, oItems);
                stream.Position = 0;
                Byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                _serializeData = _postProcessPrepare(Encoding.UTF8.GetString(bytes));
                return this;
            }
        }

        public Serialization<ReturnType> DeserializeXML(string XmlData)
        {
            using (var memory = new MemoryStream(Encoding.Unicode.GetBytes(XmlData)))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ReturnType));
                _deserializeData = (ReturnType)serializer.Deserialize(memory);
            }
            return this;
        }

        public XmlDocument Xml()
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(_serializeData);
            return xml;
        }

        public XElement ToXml()
        {
            XElement xElement = XElement.Parse(_serializeData);
            return xElement;
        }

        public ReturnType Object
        {
            get
            {
                return _deserializeData;
            }
        }

        public string SerializeJSON(ReturnType oItems)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                XmlObjectSerializer serializer = new DataContractJsonSerializer(typeof(ReturnType));
                serializer.WriteObject(memoryStream, oItems);
                string s = _postProcessPrepare(Encoding.UTF8.GetString(memoryStream.ToArray()));
                s = s.Replace("+", "~plus~");
                return s;
            }
        }

        public ReturnType DeserializeJSON(string data)
        {
            string data1 = data.Replace("~plus~", "+");
            data1 = data1.Replace(@"\""", "~quote~");
            ReturnType o = (ReturnType)typeof(ReturnType).GetConstructor(new Type[] { }).Invoke(new object[] { });
            try
            {
                using (MemoryStream memory = new MemoryStream(Encoding.Unicode.GetBytes(data1)))
                {
                    XmlObjectSerializer serializer = new DataContractJsonSerializer(o.GetType());
                    o = (ReturnType)serializer.ReadObject(memory);
                }
            }
            catch
            {
                using (MemoryStream memory = new MemoryStream(Encoding.UTF8.GetBytes(data1)))
                {
                    XmlObjectSerializer serializer = new DataContractJsonSerializer(o.GetType());
                    o = (ReturnType)serializer.ReadObject(memory);
                }
            }

            return (ReturnType)o;
        }

        private string _postProcessPrepare(string data)
        {
            data = data.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");
            data = data.Replace("xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
            return data;
        }
    }
}
