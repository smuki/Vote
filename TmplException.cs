#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Igs.Hcms.Tmpl
{
    internal class TmplException : Exception {
        private int _line;
        private int _col;

        public TmplException(string msg, int line, int col) : base(msg)
        {
            _line = line;
            _col = col;
        }

        public TmplException(string msg, Exception innerException, int line, int col) : base(msg, innerException)
        {
            _line = line;
            _col = col;
        }

        public int Col  { get { return _col;  }  }
        public int Line { get { return _line; }  }

    }
}
