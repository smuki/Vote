using System;
using System.Collections.Generic;
using System.Text;

namespace Igs.Hcms.Volt
{
    public enum TokenKind {
        EOF      ,
        Comment  , //common tokens
        ID       , // (alpha)+
        TextData , //text specific tokens

        //tag tokens
        If             , // If
        Else           , // Else
        ElseIf         , // ElseIf
        EndIf          , // EndIf
        Text           , // Text
        Expression     , // Expression
        For            , // For
        EndFor         , // EndFor
        Foreach        , // ForEach
        EndForeach     , // EndForEach
        TagStart       , // TagStart
        TagEnd         , // TagEnd
        TagEndClose    , // TagEndClose
        TagClose , // TagClose
        TagEquals      , // TagEquals

        //expression
        ExpStart         , // # at the beginning
        ExpEnd           , // # at the end
        LBracket         , // [
        RBracket         , // ]
        LParen           , // (
        RParen           , // )
        Dot              , // .
        Comma            , // Comma
        Integer          , // integer number
        Double           , // double number
        ArrayAccess      , // ArrayAccess
        BinaryExpression , // BinaryExpression
        FCall            , // FCall
        MCall            , // MCall
        FieldAccess      , // FieldAccess
        StringLiteral    , // FieldAccess
        StringExpression , // FieldAccess

        //operators

        OpOr     , // "or" keyword
        OpAnd    , // "and" keyword
        OpIs     , // "is" keyword
        OpIsNot  , // "isnot" keyword
        OpLt     , // "lt" keyword
        OpGt     , // "gt" keyword
        OpLte    , // "lte" keyword
        OpGte    , // "gte" keyword
        OpAdd    , // + keyword
        OpConcat , // & keyword
        OpSub    , // - keyword
        OpMul    , // * keyword
        OpDiv    , // / keyword
        OpMod    , // % keyword
        OpPow    , // % keyword
        OpLet    , // = keyword

        //string tokens
        StringStart , // "
        StringEnd   , // "
        StringText   // text within the string
    }
}
