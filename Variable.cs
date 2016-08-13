#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace Volte.Bot.Volt
{
    internal class Variable {
        private Variable _parent;
        private Dictionary<string, object> _values;

        public Variable() : this(null)
        {
            _values["true"]  = true;
            _values["false"] = false;
            _values["null"]  = null;
        }

        public Variable(Variable parent)
        {
            _parent = parent;
            _values = new Dictionary<string, object> (StringComparer.InvariantCultureIgnoreCase);
            _values["true"]  = true;
            _values["false"] = false;
            _values["null"]  = null;
        }

        public void Clear()
        {
            _values.Clear();
            _values["true"]  = true;
            _values["false"] = false;
            _values["null"]  = null;
        }

        public Variable Parent
        {
            get {
                return _parent;
            }
        }

        public bool IsDefined(string name)
        {
            if (_values.ContainsKey(name)) {
                return true;
            } else if (_parent != null) {
                return _parent.IsDefined(name);
            } else {
                return false;
            }
        }

        public object this[string name]
        {
            get {
                if (!_values.ContainsKey(name)) {
                    if (_parent != null) {
                        return _parent[name];
                    } else {
                        return null;
                    }
                } else {
                    return _values[name];
                }
            } set {
                _values[name] = value;
            }
        }

    }
}
