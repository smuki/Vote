using System;
using System.Collections.Generic;
using System.Text;

namespace Volte.Bot.Volt.Tokens
{
    internal class DoubleLiteral : Expression {
        private double _value;

        public DoubleLiteral(int line , int col, double value) : base(TokenKind.Double , line , col)
        {
            _value = value;
        }

        public double Value
        {
            get {
                return _value;
            }
        }

    }
}
