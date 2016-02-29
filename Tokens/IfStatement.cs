using System;
using System.Collections.Generic;
using System.Text;

namespace Igs.Hcms.Volt.Tokens
{
    internal class IfStatement : Tag {
        private Tag _falseBranch;
        private Expression _when;

        public IfStatement(int line , int col , Expression when) : base(TokenKind.If , line , col)
        {
            _when = when;
            base.Name="If";
        }

        public Tag FalseBranch
        {
            get {
                return _falseBranch;
            } set {
                _falseBranch = value;
            }
        }

        public Expression Test
        {
            get {
                return _when;
            }
        }
    }
}
