#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Igs.Hcms.Tmpl.Elements
{
    internal class DoubleLiteral : Expression {
        private double _value;

        public DoubleLiteral(int line, int col, double value) : base(line, col)
        {
            _value = value;
        }

        public double Value
        {
            get {
                return _value;
            }
        }

    }
}
