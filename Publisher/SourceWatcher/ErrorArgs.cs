using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parcsis.PSD.Publisher.SourceWatcher
{
    public class ErrorArgs:EventArgs
    {
        public Exception Exception { get; private set; }

        public ErrorArgs(Exception ex)
        {
            Exception = ex;
        }
    }
}
