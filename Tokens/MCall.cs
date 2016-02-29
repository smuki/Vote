using System;
using System.Collections.Generic;
using System.Text;

namespace Igs.Hcms.Volt.Tokens
{
    //MethodCall
    internal class MCall : Expression {
        private string name;
        private Expression obj;
        private Expression[] args;

        public MCall(int line , int col , Expression obj , string name , Expression[] args) : base(TokenKind.MCall , line , col)
        {
            this.name = name;
            this.args = args;
            this.obj  = obj;
        }

        public Expression CallObject { get { return this.obj;  }  }
        public Expression[] Args     { get { return this.args; }  }
        public string Name           { get { return this.name; }  }

    }
}
