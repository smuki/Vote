using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Igs.Hcms.Tmpl
{
    internal class Lexer {

        enum LexMode {
            Text,
            Tag,
            Expression,
            String
        }

        const char EOF = (char) 0;

        private static Dictionary<string, TokenKind> _keywords;
        private LexMode _currentMode;
        private Stack<LexMode> _modes;

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

            _keywords["or"]     = TokenKind.OpOr;
            _keywords["and"]    = TokenKind.OpAnd;
        }

        public Lexer(TextReader reader)
        {
            if (reader == null) {
                throw new ArgumentNullException("reader");
            }

            _data = reader.ReadToEnd();

            Reset();
        }

        public Lexer(string data)
        {
            if (data == null) {
                throw new ArgumentNullException("data");
            }

            _data = data;

            Reset();
        }

        private void StartRead()
        {
            _save_Line = _line;
            _save_col   = _column;
            _save_pos   = _pos;
        }

        private void EnterMode(LexMode mode)
        {
            _modes.Push(_currentMode);
            _currentMode = mode;
        }

        private void LeaveMode()
        {
            _currentMode = _modes.Pop();
        }

        private void Reset()
        {
            _modes = new Stack<LexMode>();
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

        private string TryRead(int count)
        {
            if (_pos+count >= _data.Length) {
                count=count-(_pos+count-_data.Length);
            }
            return _data.Substring(_pos, count);
        }

        private void EatSpace()
        {
            bool eat = true;

            while (eat && _pos < _data.Length) {
                char ret = _data[_pos];

                if (ret == ' ') {
                    _pos++;
                    _column++;
                } else if (ret == '\t') {
                    _pos++;
                    _column++;
                } else {
                    eat = false;
                }
            }
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
            _column = 1;
        }

        private Token CreateToken(TokenKind kind, string value)
        {
            return new Token(kind, value, _line, _column);
        }

        private Token CreateToken(TokenKind kind)
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

        private void ReadWhitespace()
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

        private void EatWhite()
        {
            bool eat = true;

            while (eat && _pos < _data.Length) {
                char ret = _data[_pos];

                if (ret == ' ') {
                    _pos++;
                    _column++;
                } else if (ret == '\n') {
                    _pos++;
                    _column++;

                    if (LA(0) == '\r') {
                        _pos++;
                        _column++;
                    }

                    NewLine();
                    eat = false;
                } else if (ret == '\r') {
                    _pos++;
                    _column++;

                    if (LA(0) == '\n') {
                        _pos++;
                        _column++;
                    }

                    NewLine();
                    eat = false;
                } else {
                    eat = false;
                }
            }
        }

        public Token Next()
        {
            switch (_currentMode) {
            case LexMode.Text:
                return NextText();

            case LexMode.Expression:
                return NextExpression();

            case LexMode.Tag:
                return NextStatement();

            case LexMode.String:
                return NextString();

            default:
                throw new TmplException("Encountered invalid lexer mode: " + _currentMode.ToString() + " " + _line + "," + _column, _line, _column);
            }
        }

        private Token NextExpression()
        {
            StartRead();
            char ch = LA(0);

            switch (ch) {
            case EOF:
                return CreateToken(TokenKind.EOF);

            case ',':
                Consume();
                return CreateToken(TokenKind.Comma);

            case '.':
                Consume();
                return CreateToken(TokenKind.Dot);

            case '(':
                Consume();
                return CreateToken(TokenKind.LParen);

            case ')':
                Consume();
                return CreateToken(TokenKind.RParen);

            case '}':
                Consume();
                LeaveMode();
                return CreateToken(TokenKind.ExpEnd);

            case '[':
                Consume();
                return CreateToken(TokenKind.LBracket);

            case ']':
                Consume();
                return CreateToken(TokenKind.RBracket);

            case '*':
                Consume();
                return CreateToken(TokenKind.OpMul);

            case '/':
                Consume();
                return CreateToken(TokenKind.OpDiv);

            case '%':
                Consume();
                return CreateToken(TokenKind.OpMod);

            case '^':
                Consume();
                return CreateToken(TokenKind.OpPow);

            case '+':
                Consume();
                return CreateToken(TokenKind.OpAdd);

            case '&':
                if (LA(1) == '&') {
                    Consume(2);
                    return CreateToken(TokenKind.OpAnd);
                } else {

                    Consume();
                    return CreateToken(TokenKind.OpConcat);
                }

            case '|':
                if (LA(1) == '|') {
                    Consume(2);
                    return CreateToken(TokenKind.OpOr);
                }

                goto default;

            case '!':
                if (LA(1) == '=') {
                    Consume(2);
                    return CreateToken(TokenKind.OpIsNot);
                }

                goto default;

            case '>':
                if (LA(1) == '=') {
                    Consume(2);
                    return CreateToken(TokenKind.OpGte);
                }

                Consume();
                return CreateToken(TokenKind.OpGt);

            case '<':
                if (LA(1) == '=') {
                    Consume(2);
                    return CreateToken(TokenKind.OpLte);
                }

                Consume();
                return CreateToken(TokenKind.OpLt);

            case '=':
                if (LA(1) == '=') {
                    Consume(2);
                    return CreateToken(TokenKind.OpIs);
                } else {
                    Consume(1);
                    return CreateToken(TokenKind.OpLet);
                }

            case' ':
            case '\t':
            case '\r':
            case '\n':
                ReadWhitespace();
                return NextExpression();

            case '"':
                Consume();
                EnterMode(LexMode.String);
                return CreateToken(TokenKind.StringStart);

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
                    return CreateToken(TokenKind.OpSub);
                }
            }

            default:
                if (char.IsLetter(ch) || ch == '_') {
                    return ReadId();
                } else {
                    throw new TmplException("Invalid character in expression: " + ch + " " + _line + "," + _column, _line, _column);
                }

            }
        }

        private Token NextStatement()
        {
            StartRead();
            StartTagRead:
            char ch = LA(0);

            switch (ch) {
            case EOF:
                return CreateToken(TokenKind.EOF);

            case '=':
                Consume();
                return CreateToken(TokenKind.TagEquals);

            case '"':
                Consume();
                EnterMode(LexMode.String);
                return CreateToken(TokenKind.StringStart);

            case '(':
                Consume();
                StartRead();
                goto StartTagRead;

            case ' ':
            case '\t':
                ReadWhitespace();
                StartRead();
                goto StartTagRead;

            case ')':
                Consume();
                EatSpace();

                if (LA(0) == '~') {
                    Consume();
                    EatWhite();
                    LeaveMode();
                    return CreateToken(TokenKind.TagEndClose);

                } else {

                    if (LA(0) == '{') {
                        Consume();
                    }

                    EatWhite();
                    LeaveMode();
                    return CreateToken(TokenKind.TagEnd);
                }

            case '~':
                Consume();
                EatWhite();
                LeaveMode();
                return CreateToken(TokenKind.TagEndClose);

            case '{':
                Consume();
                EatWhite();
                LeaveMode();
                return CreateToken(TokenKind.TagEnd);

            case '\r':
            case '\n':
                if (LA(0) == '\r' && LA(1) == '\n') {
                    Consume();
                } else if (LA(0) == '\n' && LA(1) == '\r') {
                    Consume();
                }

                Consume();
                EatWhite();
                LeaveMode();
                return CreateToken(TokenKind.TagEnd);

            case '/':
                if (LA(1) == '>') {
                    Consume(2);
                    EatWhite();
                    LeaveMode();
                    return CreateToken(TokenKind.TagEndClose);
                }

                break;

            default:
                if (char.IsLetter(ch) || ch == '_') {
                    return ReadId();
                }

                break;
            }

            throw new TmplException("Invalid character in tag: " + ch + " " + _line + "," + _column, _line, _column);
        }

        private Token NextString()
        {
            StartRead();
            StartStringRead:
            char ch = LA(0);

            switch (ch) {
            case EOF:
                return CreateToken(TokenKind.EOF);

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
                    return CreateToken(TokenKind.ExpStart);
                } else {
                    break;
                }

            case '\r':
            case '\n':
                ReadWhitespace();
                goto StartStringRead;

            case '"':
                if (LA(1) == '"') {

                    Consume(2);
                    goto StartStringRead;
                } else if (_pos == _save_pos) {
                    Consume();
                    LeaveMode();
                    return CreateToken(TokenKind.StringEnd);
                } else {
                    break;
                }

            default:
                Consume();
                goto StartStringRead;

            }

            return CreateToken(TokenKind.StringText);
        }

        private Token NextText()
        {
            StartRead();

            StartTextRead:

            switch (LA(0)) {
            case EOF:
                if (_save_pos == _pos) {
                    return CreateToken(TokenKind.EOF);
                } else {
                    break;
                }

            case '$':
                if (LA(1) == '$') {
                    Consume(2);
                    goto StartTextRead;
                } else if (_save_pos == _pos && LA(1) == '{') {
                    Consume(2);
                    EnterMode(LexMode.Expression);
                    return CreateToken(TokenKind.ExpStart);
                } else {
                    if (_r >= 1000) {
                        string ss = "";
                        int _i = 0;

                        while (_i < 10 && LA(_i) != EOF) {
                            ss = ss + LA(_i);
                            _i++;
                        }

                        throw new TmplException("loop...: " + _line + "/" + _column + " [" + ss + "]", _line, _column);
                    }

                    _r++;
                    break;
                }

            case '}':
            case '@':

                bool _f_tag_start = false;
                bool _f_tag_end   = false;
                int eat_tag_start = 1;
                int eat_tag_end   = 1;

                if (LA(0) == '@' && LA(1) == '@') {
                    Consume(2);
                    goto StartTextRead;
                } else if (LA(0) == '@') {
                    if (LA(1) == '{') {
                        eat_tag_start = 2;
                        _f_tag_start  = true;

                    } else if (TryRead(8).ToLower() == "@define " ) {
                        _f_tag_start = true;

                    } else if (TryRead(7).ToLower() == "@using " ) {
                        _f_tag_start = true;

                    } else if (TryRead(5).ToLower() == "@for " ) {
                        _f_tag_start = true;

                    } else if (TryRead(4).ToLower() == "@if " ) {
                        _f_tag_start = true;
                    } else if (TryRead(9).ToLower() == "@foreach " ) {
                        _f_tag_start = true;
                    } else if (TryRead(5).ToLower() == "@else" ) {
                        _f_tag_start = true;
                    }
                } else if (LA(0) == '~') {

                    if (TryRead(4).ToLower() == "~if " ) {

                        _f_tag_end = true;

                    } else if (TryRead(9).ToLower() == "~foreach " ) {

                        _f_tag_end = true;
                    } else if (TryRead(5).ToLower() == "~for " ) {

                        _f_tag_end = true;

                    } else if (TryRead(8).ToLower() == "~define " ) {

                        _f_tag_end = true;

                    } else if (TryRead(7).ToLower() == "~using " ) {

                        _f_tag_end = true;

                    }

                } else if (LA(0) == '}') {
                    if (LA(1) == '~') {
                        eat_tag_end = 2;
                        _f_tag_end = true;
                    } else if (TryRead(6).ToLower() == "}@else" ) {
                        eat_tag_start = 2;
                        _f_tag_start = true;

                    } else   if (LA(1) == '~' && LA(2) == '{' && (LA(3) == 'e' || LA(3) == 'E')) {
                        eat_tag_start = 3;
                        _f_tag_start = true;
                    }
                }

                if (_f_tag_start) {
                    if (_save_pos == _pos) {
                        if (eat_tag_start == 0) {
                            Consume();
                        } else {
                            Consume(eat_tag_start);
                        }

                        EnterMode(LexMode.Tag);
                        return CreateToken(TokenKind.TagStart);
                    } else {
                        break;
                    }
                } else if (_f_tag_end) {
                    if (_save_pos == _pos) {
                        Consume(eat_tag_end);
                        EnterMode(LexMode.Tag);
                        return CreateToken(TokenKind.StatementClose);
                    } else {
                        break;
                    }
                }

                Consume();
                goto StartTextRead;

            case '\n':
            case '\r':
                ReadWhitespace();
                goto StartTextRead;

            default:
                Consume();
                goto StartTextRead;
            }

            return CreateToken(TokenKind.TextData);
        }


        private Token ReadId()
        {
            StartRead();

            Consume();

            while (true) {
                char ch = LA(0);

                if (char.IsLetterOrDigit(ch) || ch == '_') {
                    Consume();
                } else {
                    break;
                }
            }

            string tokenData = _data.Substring(_save_pos, _pos - _save_pos);

            if (_keywords.ContainsKey(tokenData)) {
                return CreateToken(_keywords[tokenData]);
            } else {
                return CreateToken(TokenKind.ID, tokenData);
            }
        }

        private Token ReadNumber()
        {
            StartRead();
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

            return CreateToken(hasDot ? TokenKind.Double : TokenKind.Integer);
        }
    }
}
