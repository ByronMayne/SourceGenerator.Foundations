using System;
using System.Collections.Generic;
using System.Text;

namespace SGF.Logging.Archives
{
    internal interface ILogArchive
    {
        public void Write(string message);
    }
}
