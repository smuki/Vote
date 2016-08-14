#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
#endregion

using Volte.Bot.Tpl.Tokens;

namespace Volte.Bot.Tpl
{
    public class Volt {
        private string _name;
        private List<Token> _elements;
        private Volt _parent;
        private Dictionary<string, Volt> _tmpls;

        public Volt(string name, List<Token> elements)
        {
            _name     = name;
            _elements = elements;
            _parent   = null;

            InitTmpls();
        }

        public Volt(string name , List<Token> elements , Volt parent)
        {
            _name     = name;
            _elements = elements;
            _parent   = parent;

            InitTmpls();
        }

        public static Volt Parser(string name, string data)
        {
            Lexer _lexer   = new Lexer(data);
            Parser _parser = new Parser(_lexer);
            _parser.Parse();

            return new Volt(name, _parser.CreateHierarchy());
        }

        private void InitTmpls()
        {
            _tmpls = new Dictionary<string, Volt> (StringComparer.InvariantCultureIgnoreCase);

            foreach (Token elem in _elements) {
                if (elem is Tag) {
                    Tag tag = (Tag) elem;

                    if (string.Compare(tag.Name, "define", true) == 0) {
                        Expression ename = tag.AttributeValue("name");
                        string tname;

                        if (ename is StringLiteral) {
                            tname = ((StringLiteral) ename).Content;
                        } else {
                            tname = "?";
                        }

                        Volt tmpl   = new Volt(tname, tag.Tokens, this);
                        _tmpls[tname] = tmpl;
                    }
                }
            }
        }

        public string Name
        {
            get {
                return _name;
            } set {

                _name = value;
            }
        }

        public List<Token> Elements
        {
            get {
                return _elements;
            }
        }


        public bool HasParent
        {
            get {
                return _parent != null;
            }
        }

        public Volte.Bot.Tpl.Volt Parent
        {
            get {
                return _parent;
            }
        }

        public virtual Volt FindTmpl(string name)
        {
            if (_tmpls.ContainsKey(name)) {
                return _tmpls[name];
            } else if (_parent != null) {
                return _parent.FindTmpl(name);
            } else {
                return null;
            }
        }

        public Dictionary<string, Volte.Bot.Tpl.Volt> Tmpls
        {
            get {
                return _tmpls;
            }
        }
    }
}
