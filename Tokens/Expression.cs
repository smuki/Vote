#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Volte.Bot.Tpl.Tokens
{
    public abstract class Expression : Token {

        public Expression(TokenKind kind , int line , int col) : base(kind , line , col)
        {

        }
    }
}
