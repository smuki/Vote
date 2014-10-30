#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
#endregion

using Igs.Hcms.Tmpl.Elements;

namespace Igs.Hcms.Tmpl
{
    public class Tmpl {
        private string name;
        private List<Element> elements;
        private Tmpl parent;
        private Dictionary<string, Tmpl> tmpls;

        private StringWriter writer;
        public Tmpl(string name, List<Element> elements)
        {
            this.name = name;
            this.elements = elements;
            this.parent = null;

            InitTmpls();
        }

        public Tmpl(string name, List<Element> elements, Tmpl parent)
        {
            this.name = name;
            this.elements = elements;
            this.parent = parent;

            InitTmpls();
        }

        public static Tmpl Parser(string name, string data)
        {
            Lexer lexer = new Lexer(data);
            Parser _parser = new Parser(lexer);
            _parser.Parse();

            return new Tmpl(name, _parser.CreateHierarchy());
        }

        ///======================================================================
        private void visitElement(Element elem)
        {
            if (elem is Expression)
                visitExpression((Expression) elem);
            else if (elem is Text)
                visitText((Text) elem);
            else if (elem is IfStatement)
                visitTagIf((IfStatement) elem);
            else if (elem is Tag)
                visitTag((Tag) elem);
            else if (elem is StatementClose)
                visitTagClose((StatementClose) elem);
            else
                WriteLine("Unknown Element: " + elem.GetType().ToString());
        }

        private void visitExpression(Expression expression)
        {
            if (expression is Name)
                WriteLine("Name: " + ((Name) expression).Id);
            else if (expression is FCall) {
                FCall fcall = (FCall) expression;

                WriteLine("FCall: " + fcall.Name);

                WriteLine("Parameters: ");

                foreach (Expression exp in fcall.Args) {
                    visitExpression(exp);
                }

            } else if (expression is FieldAccess) {
                FieldAccess fa = (FieldAccess) expression;
                WriteLine("FieldAccess: " + fa.Exp + "." + fa.Field);

            } else if (expression is StringLiteral) {
                StringLiteral literal = (StringLiteral) expression;

                if (literal.Content.Length > 50)
                    WriteLine("String: " + literal.Content.Substring(0, 50) + "...");
                else
                    WriteLine("String: " + literal.Content);

            } else if (expression is StringExpression) {
                StringExpression sexp = (StringExpression) expression;
                WriteLine("StringExpression");

                foreach (Expression exp in sexp.Expressions) {
                    visitExpression(exp);
                }
            } else if (expression is BinaryExpression) {
                BinaryExpression sexp = (BinaryExpression) expression;
                WriteLine("BinaryExpression");

                visitExpression(sexp.Lhs);

                WriteLine("Operator " + sexp.Operator.ToString());

                visitExpression(sexp.Rhs);

            } else {
                WriteLine("Expression: " + expression.GetType().ToString());
            }
        }

        private void visitText(Text text)
        {
            string str = text.Data.Replace("\r\n", " ");

            if (str.Length > 25)
                WriteLine(str.Substring(0, 25) + "...");
            else
                WriteLine(str);

        }

        private void AddAttribs(List<DotAttribute> attribs)
        {
            if (attribs.Count > 0) {
                WriteLine("Attributes:");

                foreach (DotAttribute att in attribs) {
                    WriteLine(att.Name);
                    visitExpression(att.Expression);
                }
            }
        }

        private  void visitTag(Tag tag)
        {
            WriteLine("Tag: " + tag.Name);

            AddAttribs(tag.Attributes);

            foreach (Element elem in tag.InnerElements) {
                visitElement(elem);
            }
        }


        private void visitTagIf(IfStatement tag)
        {
            WriteLine("Tag: " + tag.Name);

            AddAttribs(tag.Attributes);

            foreach (Element elem in tag.InnerElements) {
                visitElement(elem);
            }

            if (tag.FalseBranch != null) {
                visitElement(tag.FalseBranch);
            }

        }

        private void visitTagClose(StatementClose tagClose)
        {
            WriteLine("StatementClose:" + tagClose.Name);
        }

        public void WriteLine(string data)
        {
            writer.WriteLine(data);
        }

        public void Debug()
        {

            writer = new StringWriter();

            foreach (Element elem in elements) {
                visitElement(elem);
                WriteLine("");
            }

            StreamWriter _File = new StreamWriter(this.name + ".lexs", false);
            _File.Write(writer);
            _File.Close();


        }

        private void InitTmpls()
        {
            this.tmpls = new Dictionary<string, Tmpl> (StringComparer.InvariantCultureIgnoreCase);

            foreach (Element elem in elements) {
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

                        Tmpl tmpl = new Tmpl(tname, tag.InnerElements, this);
                        tmpls[tname] = tmpl;
                    }
                }
            }
        }

        public List<Element> Elements
        {
            get {
                return this.elements;
            }
        }

        public string Name
        {
            get {
                return this.name;
            }
        }

        public bool HasParent
        {
            get {
                return parent != null;
            }
        }

        public Igs.Hcms.Tmpl.Tmpl Parent
        {
            get {
                return this.parent;
            }
        }

        public virtual Tmpl FindTmpl(string name)
        {
            if (tmpls.ContainsKey(name)) {
                return tmpls[name];
            } else if (parent != null) {
                return parent.FindTmpl(name);
            } else {
                return null;
            }
        }

        public Dictionary<string, Igs.Hcms.Tmpl.Tmpl> Tmpls
        {
            get {
                return this.tmpls;
            }
        }
    }
}
