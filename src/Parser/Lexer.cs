using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Volte.Bot.Tpl
{
    enum LexMode {
        Text,
        Tag,
        If,
        ElseIf,
        Else,
        Expression,
        String
    }

    internal class Lexer {
        class StackToken {
            public Token _Token;
            public LexMode _LexMode;

        }

        const char EOF    = (char) 0;
        const char _PREFIX = '@';

        private static Dictionary<string , TokenKind> _keywords;
        internal LexMode _currentMode;
        private Stack<LexMode> _modes;
        private Queue<StackToken> _que;
        private Stack<StackToken> _tokens = new Stack<StackToken>();

        private int _line;
        private int _column;
        private int _pos;
        private int _r = 0;
        private int _save_Line;
        private int _save_col;
        private int _save_pos;
        private string _data;

        static Lexer()
        {
            _keywords = new Dictionary<string, TokenKind> (StringComparer.InvariantCultureIgnoreCase);

            _keywords["or"]  = TokenKind.OpOr;
            _keywords["and"] = TokenKind.OpAnd;
        }

        public Lexer(TextReader reader)
        {
            if (reader == null) {
                throw new ArgumentNullException("Lexer reader");
            }

            _data = reader.ReadToEnd();

            Initialize();
        }

        public Lexer(string data)
        {
            if (data == null) {
                throw new ArgumentNullException("Lexer data");
            }

            _data = data;

            Initialize();
        }

        private void SavePoint()
        {
            _save_Line = _line;
            _save_col  = _column;
            _save_pos  = _pos;
        }

        public void EnterMode(LexMode mode)
        {
            _modes.Push(_currentMode);
            _currentMode = mode;
        }

        public void LeaveMode()
        {
            _currentMode = _modes.Pop();
        }

        private void Initialize()
        {
            _modes       = new Stack<LexMode>();
            _que         = new Queue<StackToken>();
            _currentMode = LexMode.Text;
            _modes.Push(_currentMode);

            _line   = 1;
            _column = 1;
            _pos    = 0;
        }

        private char LA(int count)
        {
            if (_pos + count >= _data.Length) {
                return EOF;
            } else {
                return _data[_pos + count];
            }
        }

        private string Peek(int count)
        {
            if (_pos + count >= _data.Length) {
                count = count - (_pos + count - _data.Length);
            }

            return _data.Substring(_pos , count);
        }

        private char Consume()
        {
            char ret = _data[_pos];
            _pos++;
            _column++;

            return ret;
        }

        private char Consume(int count)
        {
            if (count <= 0) {
                throw new ArgumentOutOfRangeException("count", "count has to be greater than 0");
            }

            char ret = ' ';

            while (count > 0) {
                ret = Consume();
                count--;
            }

            return ret;
        }

        private void NewLine()
        {
            _line++;
            _column   = 1;
        }

        private void QueueToken(Token _token, LexMode _LexMode)
        {
            StackToken _StackToken = new StackToken();

            _StackToken._Token   = _token;
            _StackToken._LexMode = _LexMode;

            _que.Enqueue(_StackToken);
        }

        private void PushToken(Token _token, LexMode _LexMode)
        {
            StackToken _StackToken = new StackToken();

            _StackToken._Token   = _token;
            _StackToken._LexMode = _LexMode;

            _tokens.Push(_StackToken);
        }

        private Token DequeueToken()
        {
            StackToken _StackToken = _que.Dequeue();

            _currentMode = _StackToken._LexMode;

            return _StackToken._Token;
        }

        private Token PopToken()
        {
            StackToken _StackToken = _tokens.Pop();

            _currentMode = _StackToken._LexMode;

            return _StackToken._Token;
        }

        private Token NewToken(TokenKind kind, string value)
        {
            return new Token(kind, value, _line, _column);
        }

        private Token NewToken(TokenKind kind)
        {
            string tokenData = _data.Substring(_save_pos, _pos - _save_pos);

            if (kind == TokenKind.StringText) {
                tokenData = tokenData.Replace("\"\"", "\"");
            }

            if (kind == TokenKind.StringText || kind == TokenKind.TextData) {
                tokenData = tokenData.Replace("$$", "$");
            }

            return new Token(kind, tokenData, _save_Line, _save_col);
        }

        public Token Next()
        {
            if (_tokens.Count > 0) {
                return PopToken();
            }

            if (_que.Count > 0) {
                return DequeueToken();
            }

            switch (_currentMode) {
            case LexMode.Text:
                return NextText();

            case LexMode.Expression:
                return NextExpression();

            case LexMode.If:
                return IfNextStatement();

            case LexMode.Tag:
                return NextStatement();

            case LexMode.String:
                return NextString();

            default:
                throw new VoltException("Encountered invalid lexer mode: " + _currentMode.ToString() + " " + _line + "," + _column, _line, _column);
            }
        }

        private Token NextExpression()
        {
            SavePoint();
            char ch = LA(0);

            switch (ch) {
                case EOF:
                    return NewToken(TokenKind.EOF , "EOF");

                case ',':
                    Consume();
                    return NewToken(TokenKind.Comma , ",");

                case '.':
                    Consume();
                    return NewToken(TokenKind.Dot , ".");

                case '(':
                    Consume();
                    return NewToken(TokenKind.LParen, "(");

                case ')':
                    Consume();
                    return NewToken(TokenKind.RParen, ")");

                case '}':
                    Consume();
                    LeaveMode();
                    return NewToken(TokenKind.ExpEnd, "}");

                case '{':
                    Consume();
                    PushToken(NewToken(TokenKind.TagEnd , "{") , LexMode.Text);
                    PushToken(NewToken(TokenKind.ExpEnd , "}") , LexMode.If);
                    return Next();

                case '[':
                    Consume();
                    return NewToken(TokenKind.LBracket , "[");

                case ']':
                    Consume();
                    return NewToken(TokenKind.RBracket , "]");

                case '*':
                    Consume();
                    return NewToken(TokenKind.OpMul , "*");

                case '/':
                    Consume();
                    return NewToken(TokenKind.OpDiv , "/");

                case '%':
                    Consume();
                    return NewToken(TokenKind.OpMod , "%");

                case '^':
                    Consume();
                    return NewToken(TokenKind.OpPow , "^");

                case '+':
                    Consume();
                    return NewToken(TokenKind.OpAdd , "+");

                case '&':
                    if (LA(1) == '&') {
                        Consume(2);
                        return NewToken(TokenKind.OpAnd , "&&");
                    } else {
                        Consume();
                        return NewToken(TokenKind.OpConcat , "&");
                    }

                case '|':
                    if (LA(1) == '|') {
                        Consume(2);
                        return NewToken(TokenKind.OpOr , "||");
                    }

                goto default;

            case '!':
                if (LA(1) == '=') {
                    Consume(2);
                    return NewToken(TokenKind.OpIsNot , "!=");
                }

                goto default;

            case '>':
                if (LA(1) == '=') {
                    Consume(2);
                    return NewToken(TokenKind.OpGte , ">=");
                }

                Consume();
                return NewToken(TokenKind.OpGt , ">");

            case '<':
                if (LA(1) == '=') {
                    Consume(2);
                    return NewToken(TokenKind.OpLte , "<=");
                }

                Consume();
                return NewToken(TokenKind.OpLt , "<");

            case '=':
                if (LA(1) == '=') {
                    Consume(2);
                    return NewToken(TokenKind.OpIs , "==");
                } else {
                    Consume();
                    return NewToken(TokenKind.OpLet , "=");
                }

            case' ':
            case '\t':
                SkipWhiteSpace();
                return NextExpression();

            case ';':
            case '\r':
            case '\n':
                if (LA(0)==';') {
                    Consume();
                }
                SkipWhiteSpace();
                if (LA(0) == '}') {
                    Consume();
                    LeaveMode();
                    return NewToken(TokenKind.ExpEnd , "}");
                }else{
                    QueueToken(NewToken(TokenKind.ExpEnd   , "}")  , LexMode.Expression);
                    QueueToken(NewToken(TokenKind.ExpStart , "${") , LexMode.Expression);
                    return Next();
                }

            case '"':
                Consume();
                EnterMode(LexMode.String);
                return NewToken(TokenKind.StringStart , "\"");

            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                return ReadNumber();

            case '-': {
                if (char.IsDigit(LA(1))) {
                    return ReadNumber();
                } else {
                    Consume();
                    return NewToken(TokenKind.OpSub);
                }
            }

            default:
                if (char.IsLetter(ch) || ch == '_') {
                    return ReadId();
                } else {
                    throw new VoltException("Invalid character in expression: " + ch + " " + _line + "," + _column, _line, _column);
                }

            }
        }

        private Token IfNextStatement()
        {
            SavePoint();
            StartTagRead:
            char ch = LA(0);

            switch (ch) {
            case EOF:
                return NewToken(TokenKind.EOF , "EOF");

            case '=':
                Consume();
                return NewToken(TokenKind.TagEquals , "=");

            case '"':
                Consume();
                EnterMode(LexMode.String);
                return NewToken(TokenKind.StringStart , "=");

            case '(':
                Consume();
                SkipSpace();
                return NewToken(TokenKind.LParen , "(");

            case ' ':
            case '\t':
                SkipWhiteSpace();
                SavePoint();
                goto StartTagRead;

            case ')':
                Consume();
                SkipSpace();

                if (LA(0) == '~') {
                    Consume();
                    SkipWhiteToEndOfLine();
                    LeaveMode();
                    return NewToken(TokenKind.TagEndClose);

                } else {

                    if (LA(0) == '{') {
                        Consume();
                    }

                    SkipWhiteToEndOfLine();
                    LeaveMode();
                    return NewToken(TokenKind.TagEnd);
                }

            case '~':
                Consume();
                SkipWhiteToEndOfLine();
                LeaveMode();
                return NewToken(TokenKind.TagEndClose , "~");

            case '{':
                Consume();
                SkipWhiteToEndOfLine();
                LeaveMode();
                return NewToken(TokenKind.TagEnd , "{");

            case '\r':
            case '\n':
                if (LA(0) == '\r' && LA(1) == '\n') {
                    Consume();
                } else if (LA(0) == '\n' && LA(1) == '\r') {
                    Consume();
                }

                Consume();
                SkipWhiteToEndOfLine();
                LeaveMode();
                return NewToken(TokenKind.TagEnd);

            default:
                if (char.IsLetter(ch) || ch == '_') {
                    return ReadId();
                }

                break;
            }

            throw new VoltException("Invalid character in tag: " + ch + " " + _line + "," + _column, _line, _column);
        }

        private Token NextStatement()
        {
            SavePoint();
            StartTagRead:
            char ch = LA(0);

            switch (ch) {
            case EOF:
                return NewToken(TokenKind.EOF , "EOF");

            case '=':
                Consume();
                return NewToken(TokenKind.TagEquals , "=");

            case '"':
                Consume();
                EnterMode(LexMode.String);
                return NewToken(TokenKind.StringStart , "\"");

            case '(':
                Consume();
                SavePoint();
                goto StartTagRead;

            case ' ':
            case '\t':
                SkipWhiteSpace();
                SavePoint();
                goto StartTagRead;

            case ')':
                Consume();
                SkipSpace();

                if (LA(0) == '~') {
                    Consume();
                    SkipWhiteToEndOfLine();
                    LeaveMode();
                    return NewToken(TokenKind.TagEndClose);

                } else {

                    if (LA(0) == '{') {
                        Consume();
                    }

                    SkipWhiteToEndOfLine();
                    LeaveMode();
                    return NewToken(TokenKind.TagEnd);
                }

            case '~':
                Consume();
                SkipWhiteToEndOfLine();
                LeaveMode();
                return NewToken(TokenKind.TagEndClose);

            case '{':
                Consume();
                SkipWhiteToEndOfLine();
                LeaveMode();
                return NewToken(TokenKind.TagEnd, "{");

            case '\r':
            case '\n':
                if (LA(0) == '\r' && LA(1) == '\n') {
                    Consume();
                } else if (LA(0) == '\n' && LA(1) == '\r') {
                    Consume();
                }

                Consume();
                SkipWhiteToEndOfLine();
                LeaveMode();
                return NewToken(TokenKind.TagEnd);

            default:
                if (char.IsLetter(ch) || ch == '_') {
                    return ReadId();
                }

                break;
            }

            throw new VoltException("Invalid character in tag: " + ch + " " + _line + "," + _column, _line, _column);
        }

        private Token NextString()
        {
            SavePoint();
            StartStringRead:
            char ch = LA(0);

            switch (ch) {
            case EOF:
                return NewToken(TokenKind.EOF , "EOF");

            case '$':
                if (LA(1) == '$') {
                    Consume(2);
                    goto StartStringRead;
                } else if (_save_pos == _pos) {
                    if (LA(1) == '{') {
                        Consume();
                    }

                    Consume();
                    EnterMode(LexMode.Expression);
                    return NewToken(TokenKind.ExpStart);
                } else {
                    break;
                }

            case '\r':
            case '\n':
                SkipWhiteSpace();
                goto StartStringRead;

            case '"':
                if (LA(1) == '"') {

                    Consume(2);
                    goto StartStringRead;
                } else if (_pos == _save_pos) {
                    Consume();
                    LeaveMode();
                    return NewToken(TokenKind.StringEnd);
                } else {
                    break;
                }

            default:
                Consume();
                goto StartStringRead;

            }

            return NewToken(TokenKind.StringText);
        }

        private Token NextText()
        {
            SavePoint();

            StartTextRead:

            switch (LA(0)) {
            case EOF:
                if (_save_pos == _pos) {
                    return NewToken(TokenKind.EOF , "EOF");
                } else {
                    break;
                }

            case '$':
                if (LA(1) == '$') {
                    Consume(2);
                    goto StartTextRead;
                } else if (_save_pos == _pos && LA(1) == '{') {
                    Consume(2);
                    SkipWhiteSpace();
                    EnterMode(LexMode.Expression);
                    return NewToken(TokenKind.ExpStart);
                } else if (_save_pos == _pos && (char.IsLetter(LA(1)) || LA(1) == '_')) {
                    Consume();
                    QueueToken(NewToken(TokenKind.ExpStart , "${") , LexMode.Expression);
                    QueueToken(ReadId(), LexMode.Expression);

                    if (LA(0) == '.'  && char.IsLetter(LA(1))) {
                        Consume();
                        QueueToken(NewToken(TokenKind.Dot , ".") , LexMode.Expression);
                        QueueToken(ReadId(), LexMode.Expression);
                    }

                    QueueToken(NewToken(TokenKind.ExpEnd , "}") , _currentMode);
                    return Next();
                } else {
                    //Consume();
                    //goto StartTextRead;
                    if (_r >= 1000) {
                        string ss = "";
                        int _i = 0;

                        while (_i < 10 && LA(_i) != EOF) {
                            ss = ss + LA(_i);
                            _i++;
                        }

                        throw new VoltException("loop...: " + _line + "/" + _column + " [" + ss + "]" , _line , _column);
                    }

                    _r++;
                    break;
                }

            case '}':
            case _PREFIX:

                bool _f_tag_start = false;
                bool _f_tag_end   = false;
                int eat_char      = 1;

                LexMode _Mode = LexMode.Text;

                if (LA(0) == _PREFIX && LA(1) == _PREFIX) {
                    Consume(2);
                    goto StartTextRead;
                } else if (LA(0) == _PREFIX) {
                    if (LA(1) == '{') {
                        eat_char = 2;
                        _f_tag_start  = true;

                    } else if (Peek(8).ToLower() == _PREFIX + "define ") {
                        _f_tag_start = true;

                    } else if (Peek(7).ToLower() == _PREFIX + "using ") {
                        _f_tag_start = true;

                    } else if (Peek(5).ToLower() == _PREFIX + "for ") {
                        _f_tag_start = true;

                    } else if (Peek(4).ToLower() == _PREFIX + "if ") {
                        _f_tag_start = true;
                        _Mode = LexMode.If;
                    } else if (Peek(9).ToLower() == _PREFIX + "foreach ") {
                        _f_tag_start = true;
                    } else if (Peek(5).ToLower() == _PREFIX + "else") {
                        _f_tag_start = true;
                    }
                } else if (LA(0) == '}') {
                    if (Peek(9).ToLower() == "}" + _PREFIX + "elseif ") {
                        eat_char = 2;
                        _f_tag_start = true;
                        _Mode = LexMode.ElseIf;
                    } else if (Peek(6).ToLower() == "}" + _PREFIX + "else") {
                        eat_char = 2;
                        _f_tag_start = true;
                    } else if (LA(1) == _PREFIX && LA(2) == '{' && (LA(3) == 'e' || LA(3) == 'E')) {
                        eat_char = 3;
                        _f_tag_start = true;
                    } else if (LA(1) == _PREFIX && (LA(2)=='\t' || LA(2)=='\n' || LA(2)=='\r' || LA(2)==' ') ) {
                        eat_char = 2;
                        _f_tag_end = true;
                    }
                }

                if (_f_tag_start) {
                    if (_save_pos == _pos) {
                        if (eat_char == 0) {
                            Consume();
                        } else {
                            Consume(eat_char);
                        }
                        if (_Mode == LexMode.If) {
                            EnterMode(LexMode.If);
                            return NewToken(TokenKind.If);
                        } else if (_Mode == LexMode.ElseIf) {
                            EnterMode(LexMode.If);
                            return NewToken(TokenKind.ElseIf);
                        } else {
                            EnterMode(LexMode.Tag);
                            return NewToken(TokenKind.TagStart);
                        }
                    } else {
                        break;
                    }
                } else if (_f_tag_end) {
                    if (_save_pos == _pos) {
                        if (eat_char == 0) {
                            Consume();
                        } else {
                            Consume(eat_char);
                        }
                        EnterMode(LexMode.Tag);

                        return NewToken(TokenKind.TagClose);
                    } else {
                        break;
                    }
                }

                Consume();
                goto StartTextRead;

            case '\n':
            case '\r':
                SkipWhiteSpace();
                goto StartTextRead;

            default:
                Consume();
                goto StartTextRead;
            }

            return NewToken(TokenKind.TextData);
        }

        private Token ReadId()
        {
            SavePoint();

            Consume();

            while (true) {
                char ch = LA(0);

                if (char.IsLetterOrDigit(ch) || ch == '_') {
                    Consume();
                } else {
                    break;
                }
            }

            string tokenData = _data.Substring(_save_pos , _pos - _save_pos);

            if (_keywords.ContainsKey(tokenData)) {
                return NewToken(_keywords[tokenData]);
            } else {
                return NewToken(TokenKind.ID , tokenData);
            }
        }

        private Token ReadNumber()
        {
            SavePoint();
            Consume();

            bool hasDot = false;

            while (true) {
                char ch = LA(0);

                if (char.IsNumber(ch)) {
                    Consume();
                } else if (ch == '.' && !hasDot && char.IsNumber(LA(1))) {
                    Consume();
                    hasDot = true;
                } else {
                    break;
                }
            }

            string tokenData = _data.Substring(_save_pos , _pos - _save_pos);

            return NewToken(hasDot ? TokenKind.Double : TokenKind.Integer , tokenData);
        }

        private void SkipWhiteSpace()
        {
            while (true) {
                char ch = LA(0);

                switch (ch) {
                case ' ':
                case '\t':
                    Consume();
                    break;

                case '\n':
                    Consume();

                    if (LA(0) == '\r') {
                        Consume();
                    }

                    NewLine();
                    break;

                case '\r':
                    Consume();

                    if (LA(0) == '\n') {
                        Consume();
                    }

                    NewLine();
                    break;

                default:
                    return;
                }
            }
        }

        private void SkipWhiteToEndOfLine()
        {
            bool eat = true;

            while (eat && _pos < _data.Length) {
                char ret = _data[_pos];

                if (ret == ' ' || ret=='\t') {
                    Consume();
                } else if (ret == '\n') {
                    Consume();

                    if (LA(0) == '\r') {
                        Consume();
                    }

                    NewLine();
                    eat = false;
                } else if (ret == '\r') {
                    Consume();

                    if (LA(0) == '\n') {
                        Consume();
                    }

                    NewLine();
                    eat = false;
                } else {
                    eat = false;
                }
            }
        }

        private void SkipSpace()
        {
            bool eat = true;

            while (eat && _pos < _data.Length) {
                char ret = _data[_pos];

                if (ret == ' ' || ret == '\t') {
                    Consume();
                } else {
                    eat = false;
                }
            }
        }
    }
}
