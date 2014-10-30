#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Igs.Hcms.Tmpl.Elements
{
    public class Tag : Element {
        private string name = "";
        private StatementClose closeTag;
        private bool _isClosed;  // set to true if tag ends with />
        private List<DotAttribute> attribs;
        private List<Element> innerElements;

        public Tag(int line, int col, string name)
        : base(line, col)
        {
            this.name = name;
            this.attribs = new List<DotAttribute>();
            this.innerElements = new List<Element>();
        }

        public Expression AttributeValue(string name)
        {
            foreach (DotAttribute attrib in attribs) {
                if (string.Compare(attrib.Name, name, true) == 0) {
                    return attrib.Expression;
                }
            }

            return null;
        }

        public List<Element> InnerElements
        {
            get {
                return this.innerElements;
            }
        }

        public List<DotAttribute> Attributes
        {
            get {
                return this.attribs;
            }
        }

        public string Name
        {
            get {
                if (this.name == "tag") {
                    this.name = AttributeValue("name").ToString();
                }

                return this.name;
            }
        }

        public StatementClose CloseTag
        {
            get {
                return this.closeTag;
            } set {
                this.closeTag = value;
            }
        }

        public bool IsClosed
        {
            get {
                return _isClosed;
            } set {
                _isClosed = value;
            }
        }

    }
}
