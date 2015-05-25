#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Igs.Hcms.Tmpl.Elements
{
    internal class Name : Expression {
        private string id;

        public Name(int line, int col, string id) : base(line, col)
        {
            this.id = id;
        }

        public string Id { get { return this.id; } }

    }
}
