#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Igs.Hcms.Tmpl.Elements
{
    public abstract class Expression : Token {
        public Expression(int line, int col)
        : base(line, col)
        {

        }
    }
}
