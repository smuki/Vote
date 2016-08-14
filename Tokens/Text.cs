#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Volte.Bot.Tpl.Tokens
{
    internal  class Text : Token {

        public Text(int line , int col , string data) : base(TokenKind.Text , line , col)
        {
            base.Data = data;
        }
    }
}
