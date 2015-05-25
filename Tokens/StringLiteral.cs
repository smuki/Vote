#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Igs.Hcms.Tmpl.Elements
{
    internal class StringLiteral : Expression {
        private string _content;

        public StringLiteral(int line, int col, string content) : base(line, col)
        {
            _content = content;
        }

        public string Content
        {
            get {
                return _content;
            }
        }
    }
}
