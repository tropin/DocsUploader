using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Web;

namespace Parcsis.PSD.Publisher
{
	/// <summary>
	/// Класс, описывающий словарь с регистронезависимыми строковыми ключами.
	/// </summary>
	[Serializable]
	public class WebParameterDictionary: Dictionary<string, string>
	{
		public static WebParameterDictionary Empty = new WebParameterDictionary();

		public WebParameterDictionary()
		{
		}

		public WebParameterDictionary(params KeyValuePair<string, string>[] parameters)
		{
			foreach (KeyValuePair<string, string> el in parameters)
			{
				Add(el.Key.ToLowerInvariant(), el.Value);
			}
		}

		public WebParameterDictionary(int capacity)
			: base(capacity)
		{
		}

		public WebParameterDictionary(IEqualityComparer<string> comparer)
			: base(comparer)
		{
		}

		public WebParameterDictionary(int capacity, IEqualityComparer<string> comparer)
			: base(capacity, comparer)
		{
		}

		public WebParameterDictionary(IDictionary<string, string> dictionary)
			: base(dictionary)
		{
		}

		public WebParameterDictionary(IDictionary<string, string> dictionary, IEqualityComparer<string> comparer)
			: base(dictionary, comparer)
		{
		}

		protected WebParameterDictionary(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public new void Add(string key, string value)
		{
			base.Add(key.ToLowerInvariant(), value);
		}

		public new bool ContainsKey(string key)
		{
			return base.ContainsKey(key.ToLowerInvariant());
		}

		public new bool Remove(string key)
		{
			return base.Remove(key.ToLowerInvariant());
		}

		public new string this[string key]
		{
			get
			{
				string value;

				TryGetValue(key.ToLowerInvariant(), out value);

				return value;
			}
			set
			{
				base[key.ToLowerInvariant()] = value;
			}
		}

		public override string ToString()
		{
			string[] concatBuff = new string[Count];
			Enumerator enm = GetEnumerator();
			int i = 0;
			while (enm.MoveNext())
			{
				concatBuff[i] = string.Format(CultureInfo.InvariantCulture, "{0}={1}",
				                              HttpUtility.UrlEncode(enm.Current.Key),
				                              HttpUtility.UrlEncode(enm.Current.Value)
					);

				i++;
			}
			return string.Join("&", concatBuff);
		}
	}
}