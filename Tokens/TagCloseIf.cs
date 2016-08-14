using System;
using System.Collections.Generic;
using System.Text;

namespace Volte.Bot.Tpl.Tokens
{
    public class TagCloseIf : Token {
        private string name;

        public TagCloseIf(int line, int col, string name) : base(line, col)
        {
            this.name = name;
        }

        public string Name
        {
            get {
                return this.name;
            }
        }
    }
}
