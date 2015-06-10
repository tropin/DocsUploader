using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parcsis.PSD.Publisher.SourceWatcher
{
    public class NeedActionArgs:EventArgs
    {
        public string FilePath { get; private set; }
        public string SourcePath { get; private set; }
        public string NameExpression { get; private set; }
        public NeedActionArgs(string filePath, string sourcePath, string nameExpression)
        {
            FilePath = filePath;
            SourcePath = sourcePath;
            NameExpression = nameExpression;
        }
    }
}
