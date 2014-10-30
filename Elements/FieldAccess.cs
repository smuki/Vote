#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Igs.Hcms.Tmpl.Elements
{
    internal class FieldAccess : Expression {
        private Expression exp;
        private string field;

        public FieldAccess(int line, int col, Expression exp, string field)
        : base(line, col)
        {
            this.exp = exp;
            this.field = field;
        }

        public Expression Exp
        {
            get {
                return this.exp;
            }
        }

        public string Field
        {
            get {
                return this.field;
            }
        }

    }
}
