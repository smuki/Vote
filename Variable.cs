#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Igs.Hcms.Tmpl
{
    internal class Variable {
        private Variable parent;
        private Dictionary<string, object> values;

        public Variable() : this(null)
        {
        }

        public Variable(Variable parent)
        {
            this.parent = parent;
            this.values = new Dictionary<string, object> (StringComparer.InvariantCultureIgnoreCase);
        }

        public void Clear()
        {
            values.Clear();
        }

        public Variable Parent
        {
            get {
                return parent;
            }
        }

        public bool IsDefined(string name)
        {
            if (values.ContainsKey(name)) {
                return true;
            } else if (parent != null) {
                return parent.IsDefined(name);
            } else {
                return false;
            }
        }

        public object this[string name]
        {
            get {
                if (!values.ContainsKey(name)) {
                    if (parent != null) {
                        return parent[name];
                    } else {
                        return null;
                    }
                } else {
                    return values[name];
                }
            } set {
                values[name] = value;
            }
        }

    }
}
