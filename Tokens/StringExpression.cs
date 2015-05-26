#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Igs.Hcms.Tmpl.Tokens
{
    internal class StringExpression : Expression {
        private List<Expression> exps;

        public StringExpression(int line    , int col):
        base(TokenKind.StringExpression , line     , col)
        {
            exps = new List<Expression>();
        }

        public int ExpCount
        {
            get {
                return exps.Count;
            }
        }

        public void Add(Expression exp)
        {
            exps.Add(exp);
        }

        public Expression this[int index]
        {
            get {
                return exps[index];
            }
        }

        public IEnumerable<Expression> Expressions
        {
            get {
                for (int i = 0; i < exps.Count; i++) {
                    yield return exps[i];
                }
            }
        }
    }
}
