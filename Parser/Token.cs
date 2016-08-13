﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Volte.Bot.Volt
{

    public class Token {
        private int _line;
        private int _col;
        private string _data;
        private string _type;
        private TokenKind _tokenKind;

        public Token(int line, int col)
        {
            _line      = line;
            _col       = col;
        }

        public Token(TokenKind kind, int line, int col)
        {
            _tokenKind = kind;
            _line      = line;
            _col       = col;
        }

        public Token(TokenKind kind, string data, int line, int col)
        {
            _tokenKind = kind;
            _line      = line;
            _col       = col;
            _data      = data;
        }

        public int Col  { get { return _col;  }  }
        public int Line { get { return _line; }  }

        public string Data         { get { return _data;      } set { _data      = value; }  }
        public string Type         { get { return _type;      } set { _type      = value; }  }
        public TokenKind TokenKind { get { return _tokenKind; } set { _tokenKind = value; }  }

    }
}
