#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Volte.Bot.Volt.Tokens
{
    internal class BinaryExpression : Expression {
        private Expression lhs;
        private Expression rhs;
        private TokenKind op;

        public BinaryExpression(int line , int col , Expression lhs , TokenKind op , Expression rhs) : base(TokenKind.BinaryExpression , line , col)
        {
            this.lhs = lhs;
            this.rhs = rhs;
            this.op  = op;
        }

        public Expression Lhs     { get { return this.lhs; }  }
        public Expression Rhs     { get { return this.rhs; }  }
        public TokenKind Operator { get { return this.op;  }  }

    }
}
