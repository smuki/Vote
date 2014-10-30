#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Igs.Hcms.Tmpl.Elements
{
    internal  class Text : Element {
        private string data;

        public Text(int line, int col, string data)
        : base(line, col)
        {
            this.data = data;
        }

        public string Data
        {
            get {
                return this.data;
            }
        }

    }
}
