#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
#endregion

namespace Igs.Hcms.Tmpl
{
    internal static class Util {
        private static readonly Regex _RegexVarName = new Regex("^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);
        private static object syncObject   = new object();
        private static object _PENDING     = new object();

        public static bool ToBoolean(object obj)
        {
            if (obj is bool) {
                return (bool) obj;
            } else if (obj is string) {
                string str = (string) obj;

                if (string.Compare(str, "true", true) == 0) {
                    return true;
                } else if (string.Compare(str, "y", true) == 0) {
                    return true;
                } else if (string.Compare(str, "yes", true) == 0) {
                    return true;
                }
            }

            return false;
        }

        public static bool IsInt(object args)
        {
            try {
                int value = Convert.ToInt32(args);
                return true;
            } catch (FormatException) {
                return false;
            }
        }

        public static bool IsNumeric(object str)
        {
            decimal d;
            return decimal.TryParse(str.ToString(), out d);
        }

        public static bool IsVariableName(string name)
        {
            return _RegexVarName.IsMatch(name);
        }

    }
}
