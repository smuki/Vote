using System;
using System.Collections.Generic;
using System.Text;

namespace Igs.Hcms.Tmpl
{
    internal enum TokenKind {
        EOF,
        Comment,  //common tokens
        ID,       // (alpha)+
        TextData, //text specific tokens

        //tag tokens

        TagStart,       // @:
        TagEnd,         // {
        TagEndClose,    // ~
        StatementClose, // }~
        TagEquals,      // =

        //expression

        ExpStart, // # at the beginning
        ExpEnd,   // # at the end
        LParen,   // (
        RParen,   // )
        Dot,      // .
        Comma,    // ,
        Integer,  // integer number
        Double,   // double number
        LBracket, // [
        RBracket, // ]

        //operators

        OpOr,    // "or" keyword
        OpAnd,   // "and" keyword
        OpIs,    // "is" keyword
        OpIsNot, // "isnot" keyword
        OpLt,    // "lt" keyword
        OpGt,    // "gt" keyword
        OpLte,   // "lte" keyword
        OpGte,   // "gte" keyword

        OpAdd,   // + keyword
        OpConcat,// & keyword
        OpSub,   // - keyword
        OpMul,   // * keyword
        OpDiv,   // / keyword
        OpMod,   // % keyword
        OpPow,   // % keyword
        OpLet,   // = keyword

        //string tokens
        StringStart, // "
        StringEnd,   // "
        StringText   // text within the string
    }

    internal class Token {
        private int _line;
        private int _col;
        private string _data;
        private TokenKind _tokenKind;
        private string    _type;

        public Token(TokenKind kind, string data, int line, int col)
        {
            _tokenKind = kind;
            _line      = line;
            _col       = col;
            _data      = data;
        }

        public int Col
        {
            get {
                return _col;
            }
        }

        public int Line
        {
            get {
                return _line;
            }
        }

        public string Data
        {
            get {
                return _data;
            } set {
                _data      = value;
            }
        }

        public string Type
        {
            get {
                return _type;
            } set {
                _type      = value;
            }
        }

        public TokenKind TokenKind
        {
            get {
                return _tokenKind;
            } set {
                _tokenKind = value;
            }
        }

    }
}
