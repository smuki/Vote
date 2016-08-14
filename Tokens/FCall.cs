using System;
using System.Collections.Generic;
using System.Text;

namespace Volte.Bot.Tpl.Tokens
{
    //Function Call
    internal  class FCall : Expression {
        private string name;
        private Expression[] args;

        public FCall(int line , int col , string name , Expression[] args) : base(TokenKind.FCall , line , col)
        {
            this.name = name;
            this.args = args;
        }

        public Expression[] Args { get { return this.args;  }  }
        public string Name       { get { return this.name;  }  }

    }
}
