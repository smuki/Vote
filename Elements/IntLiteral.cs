#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Igs.Hcms.Tmpl.Elements
{
    internal class IntLiteral : Expression {
        private int value;

        public IntLiteral(int line, int col, int value) : base(line, col)
        {
            this.value = value;
        }

        public int Value
        {
            get {
                return this.value;
            }
        }

    }
}
