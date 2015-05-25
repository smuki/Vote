using System;
using System.Collections.Generic;
using System.Text;

namespace Igs.Hcms.Tmpl
{
    public enum TokenKind {
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
}
