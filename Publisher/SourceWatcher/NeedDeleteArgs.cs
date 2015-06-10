using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parcsis.PSD.Publisher.SourceWatcher
{
    public class NeedDeleteArgs: EventArgs
    {
        public string FullName { get; private set; }

        public NeedDeleteArgs(string fullName)
        {
            FullName = fullName;
        }
    }
}
