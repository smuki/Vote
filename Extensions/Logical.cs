#region Using directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
#endregion

namespace Volte.Bot.Volt
{
    internal static class Logical {
        private static VoltEngine _mnr;
        private static Templates _Templates;

        public static void Register(VoltEngine mn, Templates _tpl)
        {
            _mnr       = mn;
            _Templates = _tpl;

            _mnr.RegisterFunction("contains"      , FuncContains);
            _mnr.RegisterFunction("isnullorempty" , FuncIsNullOrEmpty);
            _mnr.RegisterFunction("isnotempty"    , FuncIsNotEmpty);
            _mnr.RegisterFunction("isnumber"      , FuncIsNumber);
            _mnr.RegisterFunction("toupper"       , FuncToUpper);
            _mnr.RegisterFunction("tolower"       , FuncToLower);
            _mnr.RegisterFunction("isdefined"     , FuncIsDefined);
            _mnr.RegisterFunction("ifvariable"    , FuncIfVariable);
            _mnr.RegisterFunction("ifdefined"     , FuncIfDefined);
            _mnr.RegisterFunction("len"           , FuncLen);
            _mnr.RegisterFunction("join"          , FuncJoin);
            _mnr.RegisterFunction("split"         , FuncSplit);
            _mnr.RegisterFunction("listcontains"  , FuncListContains);
            _mnr.RegisterFunction("isnull"        , FuncIsNull);
            _mnr.RegisterFunction("not"           , FuncNot);
            _mnr.RegisterFunction("iif"           , FuncIif);
            _mnr.RegisterFunction("format"        , FuncFormat);
            _mnr.RegisterFunction("trim"          , FuncTrim);
            _mnr.RegisterFunction("filter"        , FuncFilter);
            _mnr.RegisterFunction("replace"       , FuncReplace);
            _mnr.RegisterFunction("sweep"         , FuncSweep);
            _mnr.RegisterFunction("replacewith"   , FuncReplaceWith);
            _mnr.RegisterFunction("round"         , FuncRound);
            _mnr.RegisterFunction("typeof"        , FuncTypeOf);
            _mnr.RegisterFunction("cint"          , FuncCInt);
            _mnr.RegisterFunction("cdouble"       , FuncCDouble);
            _mnr.RegisterFunction("cdate"         , FuncCDate);
            _mnr.RegisterFunction("now"           , FuncNow);
            _mnr.RegisterFunction("typeref"       , FuncTypeRef);
            _mnr.RegisterFunction("templates"     , FuncTemplates);
            _mnr.RegisterFunction("regions"       , FuncRegions);
            _mnr.RegisterFunction("todict"        , FuncToDict);
#if CSHARP30
            _mnr.RegisterFunction("query"         , FuncQuery);
#endif
            _mnr.RegisterFunction("readfile"      , FuncReadFile);
            _mnr.RegisterFunction("writefile"     , FuncWriteFile);
            _mnr.RegisterFunction("write"         , FuncWrite);
            _mnr.RegisterFunction("writeline"     , FuncWriteLine);
        }

        private static object FuncWrite(object[] args)
        {
            if (!_mnr.CheckArgCount(1, "write", args)) {
                return null;
            }

            _mnr.WriteValue(args[0]);

            return "";
        }

        private static object FuncWriteLine(object[] args)
        {
            if (!_mnr.CheckArgCount(1, "writeline", args)) {
                return null;
            }

            _mnr.WriteValue(args[0]);
            _mnr.WriteValue("\n");
            return "";
        }

        private static object FuncReadFile(object[] args)
        {
            List<string> _Data     = new List<string>();

            if (!_mnr.CheckArgCount(1, "readfile", args)) {
                return null;
            }

            string cFileName = args[0].ToString();

            if (File.Exists(cFileName)) {
                using(StreamReader sr = new StreamReader(cFileName)) {
                    string _ss;

                    while ((_ss = sr.ReadLine()) != null) {
                        _Data.Add(_ss);
                    }
                }
            }

            return _Data;
        }

        private static object FuncWriteFile(object[] args)
        {
            if (!_mnr.CheckArgCount(2, 3, "writefile", args)) {
                return null;
            }

            string cFileName = args[0].ToString();

            StreamWriter _debug = new StreamWriter(cFileName, false);
            _debug.Write(args[1]);
            _debug.Close();

            return "";
        }

#if CSHARP30
        private static dynamic FuncQuery(object[] args)
        {

            if (!_mnr.CheckArgCount(1, 2, "query", args)) {
                return null;
            }

            IDataReader _DataReader ;

            if (args[0] is IDataReader) {
                _DataReader = (IDataReader) args[0];
            } else {
                string strSql = args[0].ToString();
                int top = 0;

                if (args.Length == 2) {
                    int.TryParse(args[1].ToString(), out top);
                }

                try {


                    IDbCommand cmd = _mnr.Connection.CreateCommand();

                    cmd.CommandText = strSql;

                    _DataReader = cmd.ExecuteReader();

                    //StringBuilder _Fields = new StringBuilder();

                    //for (int i = 0; i < _DataReader.FieldCount; i++) {
                    //    _Fields.Append ("_");
                    //    _Fields.Append (_DataReader.GetName (i));
                    //}
                } catch (VoltException ex) {
                    _mnr.DisplayError(ex);
                    return null;
                } catch (Exception ex) {
                    _mnr.DisplayError(new VoltException(ex.Message, _mnr.CurrentExpression.Line, _mnr.CurrentExpression.Col));
                    return null;
                }

            }

            string className = System.Guid.NewGuid().ToString("N"); //Util.ComputeHash (_Fields.ToString());
            string[] _ar      = new string[0];//; { fileName };

            EntityCompiler _EntityCompiler = new EntityCompiler();
            return _EntityCompiler.GetEntities(_ar, _DataReader, className);
        }
#endif

        private static void WriteLog(object cMsg)
        {

            string rootPath  = AppDomain.CurrentDomain.BaseDirectory;
            string separator = Path.DirectorySeparatorChar.ToString();
            rootPath  = rootPath.Replace("/", separator);
            if (rootPath.Substring(rootPath.Length - 1) != separator){
              rootPath = rootPath + separator;
            }
            string sFileName = rootPath + "temp" + separator + "log" + separator+"Templates.log";
            StreamWriter _debug = new StreamWriter(sFileName, true);
            _debug.WriteLine(cMsg);
            _debug.Close();

        }

        private static object FuncListContains(object[] args)
        {
            if (!_mnr.CheckArgCount(2, 2, "listcontains", args)) {
                return null;
            }

            object list = args[0];
            string property = args[1].ToString();

            if (!(list is IEnumerable)) {
                throw new VoltException("argument 1 of arraycontains has to be IEnumerable", 0, 0);
            }

            IEnumerator ienum = ((IEnumerable) list).GetEnumerator();

            while (ienum.MoveNext()) {

                if (ienum.Current.ToString()==property){
                    return true;
                }
            }

            return false;
        }

        private static object FuncContains(object[] args)
        {
            if (!_mnr.CheckArgCount(2, 6, "contains", args)) {
                return null;
            }

            bool rtv1 = true;
            bool rtv2 = true;
            bool rtv3 = true;
            bool rtv4 = true;
            bool rtv5 = true;

            rtv1 = args[0].ToString().IndexOf(args[1].ToString()) >= 0;

            if (args.Length == 3) {
                rtv2 = args[0].ToString().IndexOf(args[2].ToString()) >= 0;
            }

            if (args.Length == 4) {
                rtv3 = args[0].ToString().IndexOf(args[3].ToString()) >= 0;
            }

            if (args.Length == 5) {
                rtv4 = args[0].ToString().IndexOf(args[4].ToString()) >= 0;
            }

            if (args.Length == 6) {
                rtv5 = args[0].ToString().IndexOf(args[5].ToString()) >= 0;
            }

            return rtv1 && rtv2 && rtv3 && rtv4 && rtv5;
        }

        private static object FuncIsNullOrEmpty(object[] args)
        {
            if (!_mnr.CheckArgCount(1, "isnullorempty", args)) {
                return null;
            }

            return  string.IsNullOrEmpty(args[0].ToString());
        }

        private static object FuncIsNotEmpty(object[] args)
        {
            if (!_mnr.CheckArgCount(1, "isnotempty", args)) {
                return null;
            }

            if (args[0] == null) {
                return false;
            }

            string value = args[0].ToString();
            return value.Length > 0;
        }

        private static object FuncToDict(object[] args)
        {
            if (!_mnr.CheckArgCount(3, "todict", args)) {
                return null;
            }

            string s1 = args[0].ToString();

            if (string.IsNullOrEmpty(s1)) {
                return false;
            }

            Dictionary<string, string> _Data = new Dictionary<string, string> (StringComparer.InvariantCultureIgnoreCase);

            foreach (string Segment in s1.Split('|')) {

                string[] aSegment = Segment.Split('=');

                if (aSegment.Length == 2) {
                    _Data[aSegment[0]] = aSegment[1];
                }
            }

            return _Data;
        }

        private static object FuncRegions(object[] args)
        {
            if (!_mnr.CheckArgCount(2, "regions", args)) {
                return null;
            }

            string _UID_CODE = args[0].ToString();

            return _Templates.Parse(_UID_CODE, _Templates.getRegion(_UID_CODE, args[1].ToString()));

        }

        private static object FuncTemplates(object[] args)
        {
            if (!_mnr.CheckArgCount(2, "templates", args)) {
                return null;
            }

            string _UID_CODE = args[0].ToString();

            if (args[1] is ArrayList) {
                return _Templates.Template(_UID_CODE, (ArrayList)args[1]);
            } else if (args[1] is List<string>) {
                return _Templates.Template(_UID_CODE, (List<string>)args[1]);
            } else {
                return _Templates.Template(_UID_CODE, args[1].ToString());
            }

        }

        private static object FuncIsNumber(object[] args)
        {
            if (!_mnr.CheckArgCount(1, "isnumber", args)) {
                return null;
            }

            return Util.IsInt(args[0]);
        }

        private static object FuncToUpper(object[] args)
        {
            if (!_mnr.CheckArgCount(1, "toupper", args)) {
                return null;
            }

            return args[0].ToString().ToUpper();
        }

        private static object FuncToLower(object[] args)
        {
            if (!_mnr.CheckArgCount(1, "toupper", args)) {
                return null;
            }

            return args[0].ToString().ToLower();
        }

        private static object FuncLen(object[] args)
        {
            if (!_mnr.CheckArgCount(1, "len", args)) {
                return null;
            }

            return args[0].ToString().Length;
        }

        private static object FuncIsDefined(object[] args)
        {
            if (!_mnr.CheckArgCount(1, "isdefined", args)) {
                return null;
            }

            return _mnr.IsDefined(args[0].ToString());
        }

        private static object FuncIfVariable(object[] args)
        {
            if (!_mnr.CheckArgCount(1, "ifvariable", args)) {
                return null;
            }

            if (_mnr.IsDefined(args[0].ToString())) {
                return _mnr.GetValue(args[0].ToString());
            } else {
                return string.Empty;
            }

        }

        private static object FuncIfDefined(object[] args)
        {
            if (!_mnr.CheckArgCount(2, "ifdefined", args)) {
                return null;
            }

            if (_mnr.IsDefined(args[0].ToString())) {
                return args[1];
            } else {
                return string.Empty;
            }
        }

        private static object FuncSplit(object[] args)
        {
            if (!_mnr.CheckArgCount(2, 5, "split", args)) {
                return null;
            }

            if (args.Length == 3) {
                string[] sep = new string[2];
                sep[0] = args[1].ToString();
                sep[1] = args[2].ToString();
                return args[0].ToString().Split(sep, StringSplitOptions.RemoveEmptyEntries);

            } else if (args.Length == 4) {
                string[] sep = new string[3];
                sep[0] = args[1].ToString();
                sep[1] = args[2].ToString();
                sep[2] = args[3].ToString();
                return args[0].ToString().Split(sep, StringSplitOptions.RemoveEmptyEntries);

            } else if (args.Length == 5) {
                string[] sep = new string[4];
                sep[0] = args[1].ToString();
                sep[1] = args[2].ToString();
                sep[2] = args[3].ToString();
                sep[3] = args[4].ToString();
                return args[0].ToString().Split(sep, StringSplitOptions.RemoveEmptyEntries);
            } else {
                string[] sep = new string[1];
                sep[0] = args[1].ToString();
                return args[0].ToString().Split(sep, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        private static object FuncJoin(object[] args)
        {
            if (!_mnr.CheckArgCount(2, 3, "join", args)) {
                return null;
            }

            object list = args[0];

            string property;
            string delim;

            if (args.Length == 3) {
                property = args[1].ToString();
                delim = args[2].ToString();
            } else {
                property = string.Empty;
                delim = args[1].ToString();
            }

            if (!(list is IEnumerable)) {
                throw new VoltException("argument 1 of join has to be IEnumerable", 0, 0);
            }

            IEnumerator ienum = ((IEnumerable) list).GetEnumerator();
            StringBuilder sb = new StringBuilder();
            int index = 0;

            while (ienum.MoveNext()) {
                if (index > 0) {
                    sb.Append(delim);
                }

                if (args.Length == 2) { // do not evalulate property
                    sb.Append(ienum.Current);
                } else {
                    sb.Append(VoltEngine.ProcessProperty(ienum.Current, property));
                }

                index++;
            }

            return sb.ToString();

        }

        private static object FuncIsNull(object[] args)
        {
            if (!_mnr.CheckArgCount(1, "isnull", args)) {
                return null;
            }

            return args[0] == null;
        }

        private static object FuncNot(object[] args)
        {
            if (!_mnr.CheckArgCount(1, "not", args)) {
                return null;
            }

            if (args[0] is bool) {
                return !(bool) args[0];
            } else {
                throw new VoltException("Parameter 1 of function 'not' is not boolean", 0, 0);
            }

        }

        private static object FuncIif(object[] args)
        {
            if (!_mnr.CheckArgCount(3, "iif", args)) {
                return null;
            }

            if (args[0] is bool) {
                bool test = (bool) args[0];
                return test ? args[1] : args[2];
            } else {
                throw new VoltException("Parameter 1 of function 'iif' is not boolean", 0, 0);
            }
        }

        private static object FuncFormat(object[] args)
        {
            if (!_mnr.CheckArgCount(2, "format", args)) {
                return null;
            }

            string format = args[1].ToString();

            if (args[0] is IFormattable) {
                return ((IFormattable) args[0]).ToString(format, null);
            } else {
                return args[0].ToString();
            }
        }

        private static object FuncTrim(object[] args)
        {
            if (!_mnr.CheckArgCount(1, 2, "trim", args)) {
                return null;
            }

            if (args.Length == 2) {
                return args[0].ToString().Trim(args[1].ToString().ToCharArray());
            } else {
                return args[0].ToString().Trim();
            }
        }

        private static object FuncFilter(object[] args)
        {
            if (!_mnr.CheckArgCount(2, "filter", args)) {
                return null;
            }

            object list = args[0];

            string property;
            property = args[1].ToString();

            if (!(list is IEnumerable)) {
                throw new VoltException("argument 1 of filter has to be IEnumerable", 0, 0);
            }

            IEnumerator ienum = ((IEnumerable) list).GetEnumerator();
            List<object> newList = new List<object>();

            while (ienum.MoveNext()) {
                object val = VoltEngine.ProcessProperty(ienum.Current, property);

                if (val is bool && (bool) val) {
                    newList.Add(ienum.Current);
                }
            }

            return newList;

        }

        private static object FuncReplace(object[] args)
        {
            if (!_mnr.CheckArgCount(3, "replace", args)) {
                return null;
            }

            string s1 = args[0].ToString();
            string f1 = args[1].ToString();
            string r1 = args[2].ToString();
            return s1.Replace(f1, r1);

        }

        private static object FuncSweep(object[] args)
        {
            if (!_mnr.CheckArgCount(2, 6, "sweep", args)) {
                return null;
            }

            string s1 = args[0].ToString();
            string f1 = args[1].ToString();

            if (args.Length >= 3) {
                s1 = s1.Replace(args[2].ToString(), "");
            }

            if (args.Length >= 4) {
                s1 = s1.Replace(args[3].ToString(), "");
            }

            if (args.Length >= 5) {
                s1 = s1.Replace(args[4].ToString(), "");
            }

            if (args.Length >= 6) {
                s1 = s1.Replace(args[5].ToString(), "");
            }

            s1 = s1.Replace(f1, "");

            return s1;
        }

        private static object FuncReplaceWith(object[] args)
        {
            if (!_mnr.CheckArgCount(3, 5, "replacewith", args)) {
                return null;
            }

            string s1 = args[0].ToString();
            string a1 = args[1].ToString();
            string[] sep = new string[1];

            if (args.Length == 3) {
                sep = new string[1];
                sep[0] = args[2].ToString();

            } else if (args.Length == 4) {
                sep = new string[2];
                sep[0] = args[2].ToString();
                sep[1] = args[3].ToString();
            } else if (args.Length == 5) {
                sep = new string[3];
                sep[0] = args[2].ToString();
                sep[1] = args[3].ToString();
                sep[2] = args[4].ToString();
            }

            string[]  array = a1.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            int i = 1;

            foreach (string a in array) {
                s1 = s1.Replace("<ref" + i + ">", a);
                s1 = s1.Replace("<r" + i + ">", a);
                i++;
            }

            return s1;
        }

        private static object FuncRound(object[] args)
        {
            if (!_mnr.CheckArgCount(1, 3, "round", args)) {
                return null;
            }

            if (args.Length == 3) {
                if (args[2].ToString() == "0") {
                    return Math.Round(Convert.ToDouble(args[0]), Convert.ToInt32(args[1]) , MidpointRounding.AwayFromZero);
                } else {
                    return Math.Round(Convert.ToDouble(args[0]), Convert.ToInt32(args[1]));
                }
            } else {
                if (args.Length == 1) {
                    return Math.Round(Convert.ToDouble(args[0]) , MidpointRounding.AwayFromZero);
                } else {
                    return Math.Round(Convert.ToDouble(args[0]), Convert.ToInt32(args[1]) , MidpointRounding.AwayFromZero);
                }
            }
        }

        private static object FuncTypeOf(object[] args)
        {
            if (!_mnr.CheckArgCount(1, "TypeOf", args)) {
                return null;
            }

            return args[0].GetType().Name;

        }

        private static object FuncCInt(object[] args)
        {
            if (!_mnr.CheckArgCount(1, "cint", args)) {
                return null;
            }

            return Convert.ToInt32(args[0]);
        }

        private static object FuncCDouble(object[] args)
        {
            if (!_mnr.CheckArgCount(1, "cdouble", args)) {
                return null;
            }

            return Convert.ToDouble(args[0]);
        }

        private static object FuncCDate(object[] args)
        {
            if (!_mnr.CheckArgCount(1, "cdate", args)) {
                return null;
            }

            return Convert.ToDateTime(args[0]);
        }

        private static object FuncNow(object[] args)
        {
            if (!_mnr.CheckArgCount(0, "now", args)) {
                return null;
            }

            return DateTime.Now;
        }

        private static object FuncTypeRef(object[] args)
        {
            if (!_mnr.CheckArgCount(1, "typeref", args)) {
                return null;
            }

            string typeName = args[0].ToString();

            Type type = System.Type.GetType(typeName, false, true);

            if (type != null) {
                return new TypeRef(type);
            }

            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly asm in asms) {
                type = asm.GetType(typeName, false, true);

                if (type != null) {
                    return new TypeRef(type);
                }
            }

            throw new VoltException("Cannot create type " + typeName + ".", 0, 0);
        }

    }
}
