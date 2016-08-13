using System;
using System.Collections.Generic;
using System.Text;

namespace Volte.Bot.Volt.Tokens
{
    public class TagClose : Token {
        private string _name;

        public TagClose(int line , int col , string name) : base(line , col)
        {
            _name = name;
        }

        public string Name
        {
            get {
                return _name;
            }
        }
    }
}
