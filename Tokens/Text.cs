#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Igs.Hcms.Tmpl.Tokens
{
    internal  class Text : Token {
        private string data;

        public Text(int line , int col , string data) : base(TokenKind.Text , line , col)
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
