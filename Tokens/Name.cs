using System;
using System.Collections.Generic;
using System.Text;

namespace Igs.Hcms.Volt.Tokens
{
    internal class Name : Expression {
        private string _id;

        public Name(int line , int col , string id) : base(TokenKind.ID , line , col)
        {
            _id = id;
        }

        public string Id { get { return _id; } }

    }
}
