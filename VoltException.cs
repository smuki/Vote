#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Volte.Bot.Volt
{
    internal class VoltException : Exception {
        private int _line;
        private int _col;

        public VoltException(string msg, int line, int col) : base(msg)
        {
            _line = line;
            _col = col;
        }

        public VoltException(string msg, Exception innerException, int line, int col) : base(msg, innerException)
        {
            _line = line;
            _col = col;
        }

        public int Col  { get { return _col;  }  }
        public int Line { get { return _line; }  }

    }
}
