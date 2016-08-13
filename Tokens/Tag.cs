#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Volte.Bot.Volt.Tokens
{
    public class Tag : Token {
        private string _name = "";
        private TagClose _closeTag;
        private bool _isClosed;
        private List<DotAttribute> _attribs;
        private List<Token>        _Tokens;
        private Dictionary<string , Expression>   _Expressions;

        public Tag(TokenKind kind , int line , int col) : base(kind , line , col)
        {
            _attribs     = new List<DotAttribute>();
            _Tokens      = new List<Token>();
            _Expressions = new Dictionary<string , Expression>();
        }

        public Tag(int line , int col , string name) : base(line , col)
        {
            _name        = name;
            _attribs     = new List<DotAttribute>();
            _Tokens      = new List<Token>();
            _Expressions = new Dictionary<string , Expression>();
        }

        public Expression AttributeValue(string name)
        {
            foreach (DotAttribute attrib in _attribs) {
                if (string.Compare(attrib.Name , name , true) == 0) {
                    return attrib.Expression;
                }
            }

            if (_Expressions.ContainsKey(name)) {
                return _Expressions[name];
            }

            return null;
        }

        public void AttributeValue(string name , Expression _Expression)
        {
            _Expressions[name] = _Expression;
        }

        public string Name
        {
            get {
                if (_name == "tag") {
                    _name = AttributeValue("name").ToString();
                }

                return _name;
            }
            set {
                _name = value;
            }
        }

        public Dictionary<string , Expression> Expressions { get { return _Expressions; }  }
        public List<Token> Tokens                          { get { return _Tokens;      }  }
        public List<DotAttribute> Attributes               { get { return _attribs;     }  }

        public TagClose CloseTag { get { return _closeTag; } set { _closeTag = value; }  }
        public bool IsClosed     { get { return _isClosed; } set { _isClosed = value; }  }

    }
}
