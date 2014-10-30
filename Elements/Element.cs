#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Igs.Hcms.Tmpl.Elements
{
    public class Element {
        private int _line;
        private int _col;

        public Element(int line, int col)
        {
            _line = line;
            _col  = col;
        }

        public int Col
        {
            get {
                return _col;
            }
        }

        public int Line
        {
            get {
                return _line;
            }
        }

    }
}
