using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Parcsis.PSD.Publisher.Configurations
{
	public class WatchElement : ConfigurationElement
	{
		public WatchElement()
		{
		}

        public WatchElement(string sourcePath)
		{
            SourcePath = sourcePath;
		}

		[ConfigurationProperty("SourcePath", IsRequired = true, IsKey=true)]
        public string SourcePath 
		{
            get { return (string)this["SourcePath"]; }
            set { this["SourcePath"] = value; }
		}

		[ConfigurationProperty("FileMask", IsRequired = false, DefaultValue = "*.*")]
        public string FileMask
		{
            get { return (string)this["FileMask"]; }
            set { this["FileMask"] = value; }
		}

        [ConfigurationProperty("FileNameParamRegExExpression", IsRequired = false, DefaultValue = ".*")]
        public string FileNameParamRegExExpression
        {
            get { return (string)this["FileNameParamRegExExpression"]; }
            set { this["FileNameParamRegExExpression"] = value; }
        }

        [ConfigurationProperty("IncludeSubFolders", IsRequired = false, DefaultValue = true)]
        public bool IncludeSubFolders
        {
            get { 
                bool result;
                Boolean.TryParse(this["FileNameParamRegExExpression"].ToString(), out result);
                return result;
            }
            set { this["FileNameParamRegExExpression"] = value; }
        }
	}
}
