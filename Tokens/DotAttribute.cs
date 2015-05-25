#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Igs.Hcms.Tmpl.Tokens
{
    public class DotAttribute {
        private string name;
        private Expression _expression;

        public DotAttribute(string name, Expression expression)
        {
            this.name = name;
            _expression = expression;
        }

        public Expression Expression
        {
            get {
                return _expression;
            }
        }

        public string Name
        {
            get {
                return this.name;
            }
        }

    }
}
