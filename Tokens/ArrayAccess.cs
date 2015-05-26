#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Igs.Hcms.Tmpl.Tokens
{
    internal class ArrayAccess : Expression {
        private Expression _exp;
        private Expression _index;

        public ArrayAccess(int line , int col , Expression exp, Expression index) :
        base(TokenKind.ArrayAccess   , line    , col)
        {
            _exp = exp;
            _index = index;
        }

        public Expression Exp   { get { return _exp;   }  }
        public Expression Index { get { return _index; }  }

    }
}
