using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parcsis.PSD.Publisher.QueueProcessor;
using System.Net;
using Parcsis.PSD.Publisher.Properties;
using System.IO;
using System.Web;
using Parcsis.PSD.Publisher.Common.Json.Linq;
using System.Diagnostics;

namespace Parcsis.PSD.Publisher.ItemProcessors
{
    public class HttpGetProcessor : IQueueElementProcessor
    {

        #region IQueueElementProcessor Members

        public bool Process(Queue.QueueItem item)
        {
            bool result = false;
            WebParameterDictionary wpd = new WebParameterDictionary
				(
					new[]
						{
                            new KeyValuePair<string,string>("module", "video"),
                            new KeyValuePair<string,string>("action", "save_file"),
                            new KeyValuePair<string,string>("file",HttpUtility.UrlEncode(string.Concat(item.FileName, item.FileExtension)))
						}
				);
            string requestString = string.Format("{0}?{1}", Settings.Default.HttpGetAddress, wpd.ToString());
			HttpWebRequest webRequest =
				(HttpWebRequest)WebRequest.Create(requestString);

			webRequest.Method = @"GET";
			webRequest.ContentType = @"application/x-www-form-urlencoded";

            webRequest.ContentLength = 0;

			using (WebResponse webResponse = webRequest.GetResponse())
			using (Stream stream = webResponse.GetResponseStream())
			using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
			{
				string responseText = reader.ReadToEnd();
                JObject responce_obj = JObject.Parse(responseText);

                JToken status = responce_obj["api"]["result"]["status"];

                if (status == null)
                {
                    Trace.WriteLine(string.Format(@"Ошибка оповещения публикации для Web.UI (FileName = {0}) RequestString = {1}",
                                             item.FileName, requestString), Constants.TRACE_ERROR);
                    return result;
                }

                if (string.Compare(((JValue)status).Value.ToString(), ResponseStatus.Success.ToString(), true) != 0)
                {
                    Trace.WriteLine(string.Format(@"Ошибка оповещения публикации для Web.UI (FileName = {0}) RequestString = {1}, Инфо: {2}",
                                             item.FileName, requestString, responce_obj["api"]["result"]["message"]), Constants.TRACE_ERROR);
                    return result;
                }
                else
                    result = true;
			}
            return result;
        }

        public string TransactionGroup
        {
            get { return "VASSite"; }
        }

        public int Priority
        {
            get { return 0; }
        }

        #endregion
    }
}
