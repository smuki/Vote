using System;
using System.Collections.Generic;
using System.Text;

using Volte.Bot.Volt.Tokens;

namespace Volte.Bot.Volt
{
    internal class Parser {
        private Lexer _lexer;
        private Token current;
        private List<Token> _elements;
        private int _indent = 0;
        private Stack<string> _level;

        public Parser(Lexer lexer)
        {
            _level        = new Stack<string>();
            _lexer        = lexer;
            _elements = new List<Token>();
        }

        private Token Consume()
        {
            Token old = current;
            current   = _lexer.Next();

            if (old != null) {
                Print(old);
            }
            return old;
        }

        private void Print(Token t)
        {
            //Console.WriteLine("");
            //Console.Write(t.TokenKind);
            //Console.Write("  ");
            //Console.Write(_lexer._currentMode);
            //Console.WriteLine("        "+t.Data);

        }

        private Token Consume(TokenKind kind)
        {
            Token old = current;
            current = _lexer.Next();
            Print(old);

            if (old.TokenKind != kind) {
                throw new VoltException("Consume Unexpected token: " + current.TokenKind.ToString() + ". Was expecting: " + kind + " " + current.Line + "," + current.Col, current.Line, current.Col);
            }

            return old;
        }

        private Token Current
        {
            get {
                return current;
            }
        }

        public List<Token> Parse()
        {
            _elements.Clear();
            Consume();

            while (true) {
                Token elem = ReadElement();

                if (elem == null) {
                    break;
                } else {
                    _elements.Add(elem);
                }
            }

            return _elements;
        }

        internal List<Token> CreateHierarchy()
        {
            List<Token> result = new List<Token>();

            for (int index = 0; index < _elements.Count; index++) {
                Token elem = _elements[index];

                //Console.WriteLine("==========xxx======="+elem.TokenKind);

                if (elem is Text) {
                    result.Add(elem);
                } else if (elem is Expression) {
                    result.Add(elem);
                } else if (elem is IfStatement) {
                    //Console.WriteLine("==========Type3=======");

                    result.Add(CollectForTag((Tag) elem, ref index));
                } else if (elem is Tag) {
                    //Console.WriteLine("==========Type2=======");
                    result.Add(CollectForTag((Tag) elem, ref index));
                } else if (elem is TagClose) {
                    throw new VoltException("1Close tag for [" + ((TagClose) elem).Name + "] doesn't have matching start tag. " + elem.Line + "," + elem.Col, elem.Line, elem.Col);
                } else {
                    throw new VoltException("Invalid element: [" + elem.GetType().ToString() + "] " + elem.Line + "," + elem.Col, elem.Line, elem.Col);
                }
            }

            return result;
        }

        private Tag CollectForTag(Tag tag, ref int index)
        {
            if (tag.IsClosed) {
                return tag;
            }

            _indent = _indent + 4;

            if (tag.Name=="if" || tag.Name=="foreach" || tag.Name=="using" || tag.Name=="define" || tag.Name=="for") {
                //Console.WriteLine(" in "+tag.Name);
                _level.Push(tag.Name);
            }

            if (string.Compare(tag.Name, "if", true) == 0) {
                tag = (IfStatement)tag;
            }

            Tag collectTag = tag;

            for (index++; index < _elements.Count; index++) {
                Token elem = _elements[index];
                //Console.WriteLine("x="+elem.TokenKind);

                if (elem is Text) {
                    collectTag.Tokens.Add(elem);
                } else if (elem is Expression) {
                    collectTag.Tokens.Add(elem);
                } else if (elem is Tag) {
                    Tag innerTag = (Tag) elem;

                    //Console.WriteLine("==="+innerTag.Name);
                    if (string.Compare(innerTag.Name, "else", true) == 0) {
                        if (collectTag is IfStatement) {
                            ((IfStatement) collectTag).FalseBranch = innerTag;
                            collectTag = innerTag;
                        } else {
                            throw new VoltException("else tag has to be positioned inside of if or elseif tag " + innerTag.Line + "," + innerTag.Col, innerTag.Line, innerTag.Col);
                        }
                    } else if (string.Compare(innerTag.Name, "elseif", true) == 0) {
                        if (collectTag is IfStatement) {

                            Tag newTag = (IfStatement)innerTag;
                            ((IfStatement)collectTag).FalseBranch = newTag;
                            collectTag = newTag;
                        } else {
                            throw new VoltException("elseif tag is not positioned properly" + innerTag.Line + "," + innerTag.Col, innerTag.Line, innerTag.Col);
                        }
                    } else {
                        collectTag.Tokens.Add(CollectForTag(innerTag, ref index));
                    }

                } else if (elem is TagClose) {
                    TagClose tagClose = (TagClose) elem;
                    _indent = _indent - 4;
                    _level.Pop();
                    return tag;
                } else {
                    throw new VoltException("Invalid element: [" + elem.GetType().ToString() + "] " + elem.Line + "," + elem.Col, elem.Line, elem.Col);
                }
            }

            throw new VoltException("Start tag: [" + tag.Name + "] does not have matching end tag." + tag.Line + "," + tag.Col, tag.Line, tag.Col);
        }

        private Token ReadElement()
        {
            switch (Current.TokenKind) {
                case TokenKind.EOF:
                    return null;

                case TokenKind.If:
                    return ReadIfTag();

                case TokenKind.ElseIf:
                    return ReadElseIfTag();

                case TokenKind.TagStart:
                    return ReadTag();

                case TokenKind.TagClose:
                    return ReadCloseTag();

                case TokenKind.ExpStart:
                    return ReadExpression();

                case TokenKind.TextData:
                    Text text = new Text(Current.Line, Current.Col, Current.Data);
                    Consume();
                    return text;

                default:
                    throw new VoltException("Invalid token: " + Current.TokenKind.ToString() + " " + Current.Line + "," + Current.Col, Current.Line, Current.Col);
            }
        }

        private TagClose ReadCloseTag()
        {
            Token idToken = Consume(TokenKind.TagClose);

            if (Current.TokenKind == TokenKind.ID) {
                idToken = Consume(TokenKind.ID);
            }
            Consume(TokenKind.TagEnd);

            return new TagClose(idToken.Line, idToken.Col, idToken.Data);
        }

        private Expression ReadExpression()
        {
            Consume(TokenKind.ExpStart);

            Expression exp = TopExpression();

            Consume(TokenKind.ExpEnd);

            return exp;
        }

        private Tag ReadElseIfTag()
        {

            Consume(TokenKind.ElseIf);
            Token name = Consume(TokenKind.ID);

            _lexer.EnterMode(LexMode.Expression);

            Expression TestExp;

            if (Current.TokenKind == TokenKind.LParen) {
                TestExp = TopExpression();
            } else {
                TestExp = TopExpression();
            }

            IfStatement tag = new IfStatement(name.Line, name.Col, TestExp);
            tag.Name = "elseif";

            while (true) {
                if (Current.TokenKind == TokenKind.TagEnd) {
                    Consume();
                    break;
                } else if (Current.TokenKind == TokenKind.ExpEnd) {
                    Consume(TokenKind.ExpEnd);
                } else if (Current.TokenKind == TokenKind.TagEndClose) {
                    Consume();
                    tag.IsClosed = true;
                    break;
                } else {
                    throw new VoltException("ElseIf tag Invalid token in tag: " + Current.TokenKind + " " + Current.Line + "," + Current.Col, Current.Line, Current.Col);
                }
            }

            return tag;

        }

        private Tag ReadIfTag()
        {

            Consume(TokenKind.If);
            Token name = Consume(TokenKind.ID);

            _lexer.EnterMode(LexMode.Expression);

            Expression TestExp;

            if (Current.TokenKind == TokenKind.LParen) {
                TestExp = TopExpression();
            } else {
                TestExp = TopExpression();
            }

            IfStatement tag = new IfStatement(name.Line, name.Col, TestExp);
            tag.Name = "if";


            while (true) {
                if (Current.TokenKind == TokenKind.TagEnd) {
                    Consume();
                    break;
                } else if (Current.TokenKind == TokenKind.ExpEnd) {
                    Consume(TokenKind.ExpEnd);
                } else if (Current.TokenKind == TokenKind.TagEndClose) {
                    Consume();
                    tag.IsClosed = true;
                    break;
                } else {
                    throw new VoltException("If tag Invalid token in tag: " + Current.TokenKind + " " + Current.Line + "," + Current.Col, Current.Line, Current.Col);
                }
            }

            return tag;

        }

        private Tag ReadTag()
        {
            Consume(TokenKind.TagStart);
            Token name = Consume(TokenKind.ID);
            Tag tag = new Tag(name.Line, name.Col, name.Data);

            if (tag.Name == "elseif") {
                _lexer.EnterMode(LexMode.Expression);

                Expression TestExp;

                if (Current.TokenKind == TokenKind.LParen) {
                    TestExp = TopExpression();
                } else {
                    TestExp = TopExpression();
                }

                tag = new IfStatement(name.Line, name.Col, TestExp);
            }else if (tag.Name == "foreach") {
                Token _var = Consume(TokenKind.ID);

                Token _in = Consume(TokenKind.ID);
                if (_in.Data.ToLower()!="in"){
                    throw new VoltException("foreach expected 'in': " , _in.Line , _in.Col);
                }

                _lexer.EnterMode(LexMode.Expression);

                Expression _ListExp = TopExpression();

                tag.AttributeValue("var"  , new StringLiteral(_var.Line , _var.Col , _var.Data));
                tag.AttributeValue("list" , _ListExp);

            }else if (tag.Name == "define") {
                Token _var = Consume(TokenKind.ID);

                tag.AttributeValue("name"  , new StringLiteral(_var.Line , _var.Col , _var.Data));

            }else if (tag.Name == "using") {
                Token _var = Consume(TokenKind.ID);

                tag.AttributeValue("tmpl"  , new StringLiteral(_var.Line , _var.Col , _var.Data));
            }

            while (true) {
                if (Current.TokenKind == TokenKind.ID) {
                    tag.Attributes.Add(ReadAttribute());
                } else if (Current.TokenKind == TokenKind.RParen) {
                    Consume(TokenKind.RParen);
                } else if (Current.TokenKind == TokenKind.LParen) {
                    Consume(TokenKind.LParen);
                } else if (Current.TokenKind == TokenKind.ExpEnd) {
                    Consume(TokenKind.ExpEnd);
                } else if (Current.TokenKind == TokenKind.TagEnd) {
                    Consume();
                    break;
                } else if (Current.TokenKind == TokenKind.TagEndClose) {
                    Consume();
                    tag.IsClosed = true;
                    break;
                } else {
                    throw new VoltException("Invalid token in tag: " + Current.TokenKind + " " + Current.Line + "," + Current.Col, Current.Line, Current.Col);
                }
            }

            return tag;

        }

        private DotAttribute ReadAttribute()
        {
            Token name = Consume(TokenKind.ID);
            Consume(TokenKind.TagEquals);

            Expression exp = null;

            if (Current.TokenKind == TokenKind.StringStart) {
                exp = ReadString();
            } else {
                throw new VoltException("ReadAttribute Unexpected token: " + Current.TokenKind + ". Was expection '\"'" + Current.Line + "," + Current.Col, Current.Line, Current.Col);
            }

            return new DotAttribute(name.Data, exp);
        }

        private Expression ReadString()
        {
            Token start = Consume(TokenKind.StringStart);
            StringExpression exp = new StringExpression(start.Line, start.Col);

            while (true) {
                Token tok = Current;

                if (tok.TokenKind == TokenKind.StringEnd) {
                    Consume();
                    break;
                } else if (tok.TokenKind == TokenKind.EOF) {
                    throw new VoltException("ReadString Unexpected end of file" + tok.Line + "," + tok.Col, tok.Line, tok.Col);
                } else if (tok.TokenKind == TokenKind.StringText) {
                    Consume();
                    exp.Add(new StringLiteral(tok.Line, tok.Col, tok.Data));
                } else if (tok.TokenKind == TokenKind.ExpStart) {
                    exp.Add(ReadExpression());
                } else {
                    throw new VoltException("ReadString Unexpected token in string: " + tok.TokenKind + " " + tok.Line + "," + tok.Col, tok.Line, tok.Col);
                }
            }

            if (exp.ExpCount == 1) {
                return exp[0];
            } else {
                return exp;
            }
        }
        //------------------------------------------------------------------------
        private Expression TopExpression()
        {
            return LetExpression();
        }

        private Expression LetExpression()
        {
            Expression ret = OrExpression();

            while (Current.TokenKind == TokenKind.OpLet) {
                Consume();
                Expression rhs = OrExpression();
                ret = new BinaryExpression(ret.Line, ret.Col, ret, TokenKind.OpLet, rhs);
            }

            return ret;
        }

        private Expression OrExpression()
        {
            Expression ret = AndExpression();

            while (Current.TokenKind == TokenKind.OpOr) {
                Consume();
                Expression rhs = AndExpression();
                ret = new BinaryExpression(ret.Line, ret.Col, ret, TokenKind.OpOr, rhs);
            }

            return ret;
        }

        private Expression AndExpression()
        {
            Expression ret = EqualityExpression();

            while (Current.TokenKind == TokenKind.OpAnd) {
                Consume();
                Expression rhs = EqualityExpression();
                ret = new BinaryExpression(ret.Line, ret.Col, ret, TokenKind.OpAnd, rhs);
            }

            return ret;
        }

        private Expression EqualityExpression()
        {
            Expression ret = RelationalExpression();

            while (Current.TokenKind == TokenKind.OpIs
                    || Current.TokenKind == TokenKind.OpIsNot) {
                Token tok = Consume();
                Expression rhs = RelationalExpression();

                ret = new BinaryExpression(ret.Line, ret.Col, ret, tok.TokenKind, rhs);
            }

            return ret;
        }

        private Expression RelationalExpression()
        {
            Expression ret = AddSubExpression();

            while (Current.TokenKind == TokenKind.OpLt
                    || Current.TokenKind == TokenKind.OpLte
                    || Current.TokenKind == TokenKind.OpGt
                    || Current.TokenKind == TokenKind.OpGte) {
                Token tok      = Consume();
                Expression rhs = AddSubExpression();
                ret            = new BinaryExpression(ret.Line, ret.Col, ret, tok.TokenKind, rhs);
            }

            return ret;
        }

        private Expression AddSubExpression()
        {
            Expression ret = MulDivExpression();

            while (Current.TokenKind == TokenKind.OpAdd
                    || Current.TokenKind == TokenKind.OpSub
                  ) {
                Token tok      = Consume();
                Expression rhs = MulDivExpression();
                ret            = new BinaryExpression(ret.Line, ret.Col, ret, tok.TokenKind, rhs);
            }

            return ret;
        }

        private Expression MulDivExpression()
        {
            Expression ret = PowerExpression();

            while (Current.TokenKind == TokenKind.OpMul
                    || Current.TokenKind == TokenKind.OpDiv
                    || Current.TokenKind == TokenKind.OpMod
                  ) {
                Token tok      = Consume();
                Expression rhs = PowerExpression();
                ret            = new BinaryExpression(ret.Line, ret.Col, ret, tok.TokenKind, rhs);
            }

            return ret;
        }

        private Expression PowerExpression()
        {
            Expression ret = AtomExpression();

            while (Current.TokenKind == TokenKind.OpPow
                  ) {
                Token tok      = Consume();
                Expression rhs = AtomExpression();
                ret            = new BinaryExpression(ret.Line, ret.Col, ret, tok.TokenKind, rhs);
            }

            return ret;
        }


        private Expression AtomExpression()
        {
            if (Current.TokenKind == TokenKind.StringStart) {
                return ReadString();
            } else if (Current.TokenKind == TokenKind.ID) {
                Token id = Consume();
                Expression exp = null;

                if (Current.TokenKind == TokenKind.LParen) {
                    Consume();
                    Expression[] args = ReadArguments();
                    Consume(TokenKind.RParen);

                    exp = new FCall(id.Line, id.Col, id.Data, args);
                } else {
                    exp = new Name(id.Line, id.Col, id.Data);
                }

                while (Current.TokenKind == TokenKind.Dot || Current.TokenKind == TokenKind.LBracket) {
                    if (Current.TokenKind == TokenKind.Dot) {
                        Consume();
                        Token field = Consume(TokenKind.ID);

                        if (Current.TokenKind == TokenKind.LParen) {
                            Consume();
                            Expression[] args = ReadArguments();
                            Consume(TokenKind.RParen);

                            exp = new MCall(field.Line, field.Col, exp, field.Data, args);
                        } else {
                            exp = new FieldAccess(field.Line, field.Col, exp, field.Data);
                        }
                    } else {
                        Token bracket = Current;
                        Consume();
                        Expression indexExp = TopExpression();
                        Consume(TokenKind.RBracket);

                        exp = new ArrayAccess(bracket.Line, bracket.Col, exp, indexExp);
                    }
                }

                return exp;

            } else if (Current.TokenKind == TokenKind.Integer) {
                int value = int.Parse(Current.Data);
                IntLiteral intLiteral = new IntLiteral(Current.Line, Current.Col, value);
                Consume();
                return intLiteral;

            } else if (Current.TokenKind == TokenKind.Double) {
                double value = double.Parse(Current.Data);
                DoubleLiteral dLiteral = new DoubleLiteral(Current.Line, Current.Col, value);
                Consume();
                return dLiteral;

            } else if (Current.TokenKind == TokenKind.LParen) {
                Consume();
                Expression exp = TopExpression();
                Consume(TokenKind.RParen);
                return exp;

            } else {
                throw new VoltException(string.Format("Invalid token in expression: " + Current.TokenKind + ". Was expecting ID or string.L{0}/C{1}", Current.Line, Current.Col), Current.Line, Current.Col);
            }
        }

        private Expression[] ReadArguments()
        {
            List<Expression> exps = new List<Expression>();

            int index = 0;

            while (true) {
                if (Current.TokenKind == TokenKind.RParen) {
                    break;
                }

                if (index > 0) {
                    Consume(TokenKind.Comma);
                }

                exps.Add(TopExpression());

                index++;
            }

            return exps.ToArray();
        }
    }
}
