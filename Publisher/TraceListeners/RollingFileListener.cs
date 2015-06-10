using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Globalization;
using System.Threading;

namespace Parcsis.PSD.Publisher.TraceListeners
{
	public class RollingFileListener : System.Diagnostics.DelimitedListTraceListener
	{
		#region Fileds

        private string _entryDateFormat = "dd.MM.yyyy HH:mm:ss";
		private string _fileNameFormat = "ddMMyyyyHHmmss";
		private string _fileName = string.Empty;
		private string _delimiter = ";";
		private TimeSpan _rollTime = new TimeSpan(24, 0, 0);
        private TraceWriterFactrory _factory = null;

		#endregion

		#region Properties

		private WriterChunk CurrentChunk
        {
            get
            {
                return _factory.GetWriterChunk(FileName);
            }
        }   
        
        
        new protected TextWriter Writer
		{
			get
			{
                return CurrentChunk.ConnectedWriter;
			}
		}

        public RollingFileListener(string fileName)
            : base(fileName)
        {
        }

		/// <summary>
		/// Формат даты, используемый в лог-файле
		/// </summary>
		public string EntryDateFormat
		{
			get
			{
				foreach (DictionaryEntry entry in this.Attributes)
				{
					if (entry.Key.ToString().ToLower() == "entrydateformat")
					{
						_entryDateFormat = entry.Value.ToString();
					}
				}
				return _entryDateFormat;
			}
			set
			{
				_entryDateFormat = value;
			}
		}

		/// <summary>
		/// Формат имени файла
		/// </summary>
		public string FileNameFormat
		{
			get
			{
				string s = GetAttributeValue("filenameformat");
				if (!string.IsNullOrEmpty(s))
				{
					_fileNameFormat = s.Clear(":", "\\", "/", "<", ">");
				}
				return _fileNameFormat;
			}
			set
			{
				_fileNameFormat = value;
			}
		}

		/// <summary>
		/// Имя базового файла
		/// </summary>
		public string FileName
		{
			get
			{
				string s = GetAttributeValue("filename");
				if (!string.IsNullOrEmpty(s))
				{
					_fileName = s.Clear(":", "\\", "/", "<", ">");
				}
				return _fileName;
			}
			set
			{
				_fileName = value;
			}
		}

		public TimeSpan RollTime
		{
			get
			{
				string s = GetAttributeValue("rollTime");
				if (!string.IsNullOrEmpty(s))
				{
					string[] c = s.Split(new[] { ':' });

					_rollTime = new TimeSpan(int.Parse(c[0]), int.Parse(c[1]), int.Parse(c[2]));
				}
				return _rollTime;
			}
			set
			{
				_rollTime = value;
			}
		}

		new public string Delimiter
		{
			get
			{
				string s = this.Attributes.OfType<DictionaryEntry>().FirstOrDefault(de =>
					de.Key.ToString() == "delimiter").Value.ToString();
				if (!string.IsNullOrEmpty(s))
				{
					_delimiter = s;
				}
				return _delimiter;
			}
			set
			{
				_delimiter = value;
			}
		}

		protected bool IsEnabled(TraceOptions opts)
		{
			return ((opts & this.TraceOutputOptions) != TraceOptions.None);
		}

		#endregion

		#region Ctors

        private void EnsureFactory()
        {
            if (_factory == null)
                _factory = new TraceWriterFactrory(RollTime, Encoding.UTF8, FileNameFormat);
        }

		#endregion

		#region

		protected override string[] GetSupportedAttributes()
		{
			return new[] { "dateFormat", "fileName", "delimiter", "rollTime", "entryDateFormat", "fileNameFormat" };
		}

		protected void WriteHeader(string source, TraceEventType eventType, int id)
		{
			this.WriteEscaped(source);
			this.Write(this.Delimiter);
			this.Write(eventType.ToString());
			this.Write(this.Delimiter);
			this.Write(id.ToString(CultureInfo.InvariantCulture));
			this.Write(this.Delimiter);
		}

		protected void WriteEscaped(string message)
		{
			if (!string.IsNullOrEmpty(message))
			{
				int num;
				StringBuilder builder = new StringBuilder("\"");
				int startIndex = 0;
				while ((num = message.IndexOf('"', startIndex)) != -1)
				{
					builder.Append(message, startIndex, num - startIndex);
					builder.Append("\"\"");
					startIndex = num + 1;
				}
				builder.Append(message, startIndex, message.Length - startIndex);
				builder.Append("\"");
				this.Write(builder.ToString());
			}
		}

		protected void WriteStackEscaped(Stack stack)
		{
			StringBuilder builder = new StringBuilder("\"");
			bool flag = true;
			foreach (object obj2 in stack)
			{
				int num;
				if (!flag)
				{
					builder.Append(", ");
				}
				else
				{
					flag = false;
				}
				string str = obj2.ToString();
				int startIndex = 0;
				while ((num = str.IndexOf('"', startIndex)) != -1)
				{
					builder.Append(str, startIndex, num - startIndex);
					builder.Append("\"\"");
					startIndex = num + 1;
				}
				builder.Append(str, startIndex, str.Length - startIndex);
			}
			builder.Append("\"");
			this.Write(builder.ToString());
		}

		protected void WriteFooter(TraceEventCache eventCache)
		{
			if (eventCache != null)
			{
				if (this.IsEnabled(TraceOptions.ProcessId))
				{
					this.Write(eventCache.ProcessId.ToString(CultureInfo.InvariantCulture));
				}
				if (this.IsEnabled(TraceOptions.LogicalOperationStack))
				{
					this.Write(this.Delimiter);
					this.WriteStackEscaped(eventCache.LogicalOperationStack);
				}
				if (this.IsEnabled(TraceOptions.ThreadId))
				{
					this.Write(this.Delimiter);
					this.WriteEscaped(eventCache.ThreadId.ToString(CultureInfo.InvariantCulture));
				}
				if (this.IsEnabled(TraceOptions.DateTime))
				{
					this.Write(this.Delimiter);
					//this.WriteEscaped(eventCache.DateTime.ToString("o", CultureInfo.InvariantCulture));
					this.WriteEscaped(eventCache.DateTime.ToString(EntryDateFormat));
				}
				if (this.IsEnabled(TraceOptions.Timestamp))
				{
					this.Write(this.Delimiter);
					this.Write(eventCache.Timestamp.ToString(CultureInfo.InvariantCulture));
				}
				if (this.IsEnabled(TraceOptions.Callstack))
				{
					this.Write(this.Delimiter);
					this.WriteEscaped(eventCache.Callstack);
				}
			}
			else
			{
				for (int i = 0; i < 5; i++)
				{
					this.Write(this.Delimiter);
				}
			}
			this.WriteLine("");

		}

		private string GetAttributeValue(string name)
		{
			string result = string.Empty;
			DictionaryEntry entry = this.Attributes.OfType<DictionaryEntry>().FirstOrDefault(de =>
					de.Key.ToString().ToLower() == name.ToLower());
			if (entry.Key != null && entry.Value != null)
			{
				result = entry.Value.ToString();
			}
			return result;
		}

		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
		{
            EnsureFactory();
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data, null))
			{
				lock (CurrentChunk.SyncLock)
				{
					this.WriteHeader(source, eventType, id);
					this.Write(this.Delimiter);
					this.WriteEscaped(data.ToString());
					this.Write(this.Delimiter);
					this.WriteFooter(eventCache);
				}
			}
		}

		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
		{
            EnsureFactory();
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data))
			{
                lock (CurrentChunk.SyncLock)
				{
					this.WriteHeader(source, eventType, id);
					this.Write(this.Delimiter);
					if (data != null)
					{
                        if (data.Length > 1)
                            this.WriteEscaped(string.Format(data[0].ToString(), data.Skip(1).ToArray()));
                        else
                            this.WriteEscaped(data[0].ToString());
					}
					this.Write(this.Delimiter);
					this.WriteFooter(eventCache);
				}
			}
		}

		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
		{
            EnsureFactory();
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, format, args, null, null))
			{
                lock (CurrentChunk.SyncLock)
				{
					this.WriteHeader(source, eventType, id);
					if (args != null && args.Length > 0)
					{
						this.WriteEscaped(string.Format(CultureInfo.InvariantCulture, format, args));
					}
					else
					{
						this.WriteEscaped(format);
					}
					this.Write(this.Delimiter);
					this.Write(this.Delimiter);
					this.WriteFooter(eventCache);
				}
			}
		}

		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
		{
            EnsureFactory();
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, message, null, null, null))
			{
                lock (CurrentChunk.SyncLock)
				{
					this.WriteHeader(source, eventType, id);
					this.WriteEscaped(message);
					this.Write(this.Delimiter);
					this.Write(this.Delimiter);
					this.WriteFooter(eventCache);
				}
			}
		}

		#endregion

		public override void Write(string message)
		{
            EnsureFactory();
            lock (CurrentChunk.SyncLock)
			{
					if (NeedIndent)
					{
						this.WriteIndent();
					}
					Writer.Write(message);
                    Flush();
			}
		}

		public override void WriteLine(string message)
		{
            EnsureFactory();
            lock (CurrentChunk.SyncLock)
			{             
    			if (NeedIndent)
    			{
    				this.WriteIndent();
    			}
    			Writer.WriteLine(message);
                Flush();
    			base.NeedIndent = true;
			}
		}

		public override void Flush()
		{
			Writer.Flush();
		}

		public override void Close()
		{
			Flush();
			Writer.Close();
			base.Close();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.Close();
			}
			base.Dispose(disposing);
		}
	}

	internal static class Extensions
	{
		/// <summary>
		/// Очищает исходную строку от подстрок
		/// </summary>
		/// <param name="s">Исходная строка</param>
		/// <param name="chars">Строки, считающиеся "мусором"</param>
		/// <returns></returns>
		public static string Clear(this string s, params string[] chars)
		{
			string result = s;
			if (chars != null && chars.Length > 0)
			{
				foreach (string str in chars)
				{
					s.Replace(str, "");
				}
			}
			return s;
		}
	}
}
