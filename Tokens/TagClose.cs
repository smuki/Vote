#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Igs.Hcms.Tmpl.Elements
{
    public class StatementClose : Token {
        private string name;

        public StatementClose(int line, int col, string name) : base(line, col)
        {
            this.name = name;
        }

        public string Name
        {
            get {
                return this.name;
            }
        }
    }
}
