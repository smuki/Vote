using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Reflection;
using System.IO;

using Igs.Hcms.Tmpl.Elements;

namespace Igs.Hcms.Tmpl
{

    public delegate object FunctionDefinition(object[] args);

    public class TmplManager {
        const string ZFILE_NAME = "TmplManager";

        private static readonly object _PENDING = new object();
        private bool _silentErrors;
        private bool _scriptMode;

        private Dictionary<string, FunctionDefinition> _fnTbl;
        private Dictionary<string, ITmplHandler> customTags;
        private Variable _variables;
        internal Expression CurrentExpression;
        private TextWriter writer;
        private Tmpl _mainTmpl;
        private Tmpl _currentTmpl;
        private ITmplHandler _handler;
        private Templates _Templates;
        private IDbConnection _connection;

        public ITmplHandler Handler     { get { return _handler;      } set { _handler      = value; }  }
        public bool SilentErrors        { get { return _silentErrors; } set { _silentErrors = value; }  }
        public bool ScriptMode          { get { return _scriptMode;   } set { _scriptMode   = value; }  }
        public IDbConnection Connection { get { return _connection;   } set { _connection   = value; }  }

        public TmplManager(Tmpl tmpl)
        {
            _mainTmpl     = tmpl;
            _currentTmpl  = tmpl;
            _silentErrors = false;
            _scriptMode   = false;

            Init();
        }

        public static TmplManager Parser(string tmpl)
        {
            Tmpl iTmpl = Tmpl.Parser("", tmpl);
            return new TmplManager(iTmpl);
        }

        private Dictionary<string, ITmplHandler> CustomTags
        {
            get {
                if (customTags == null) {
                    customTags = new Dictionary<string, ITmplHandler> (StringComparer.CurrentCultureIgnoreCase);
                }

                return customTags;
            }
        }

        public void Register(string tagName, ITmplHandler handler)
        {
            lock (_PENDING) {
                CustomTags[tagName] = handler;
            }
        }

        public bool IsTagRegistered(string tagName)
        {
            return CustomTags.ContainsKey(tagName);
        }

        public void UnRegisterTag(string tagName)
        {
            CustomTags.Remove(tagName);
        }

        public void AddTmpl(Tmpl tmpl)
        {
            _mainTmpl.Tmpls.Add(tmpl.Name, tmpl);
        }

        private void Init()
        {
            _fnTbl     = new Dictionary<string, FunctionDefinition> (StringComparer.InvariantCultureIgnoreCase);
            _variables = new Variable();

            _variables["true"]  = true;
            _variables["false"] = false;
            _variables["null"]  = null;

            _Templates = new Templates();

            Logical.Register(this, _Templates);

        }

        #region Functions

        public void RegisterFunction(string functionName, FunctionDefinition fn)
        {
            _fnTbl.Add(functionName, new FunctionDefinition(fn));
        }

        internal bool CheckArgCount(int count, string funcName, object[] args)
        {
            if (count != args.Length) {
                DisplayError(string.Format("Function {0} requires {1} arguments and {2} were passed", funcName, count, args.Length), CurrentExpression.Line, CurrentExpression.Col);
                return false;
            } else {
                return true;
            }
        }

        internal bool CheckArgCount(int count1, int count2, string funcName, object[] args)
        {
            if (args.Length < count1 || args.Length > count2) {
                string msg = string.Format("Function {0} requires between {1} and {2} arguments and {3} were passed", funcName, count1, count2, args.Length);
                DisplayError(msg, CurrentExpression.Line, CurrentExpression.Col);
                return false;
            } else {
                return true;
            }
        }

        #endregion

        public bool IsDefined(string name)
        {
            return _variables.IsDefined(name);
        }

        public void SetValue(string name, object value)
        {
            _variables[name] = value;
        }

        public object GetValue(string name)
        {
            if (_variables.IsDefined(name)) {
                return _variables[name];
            } else {
                throw new Exception("Variable '" + name + "' cannot be found in current scope.");
            }
        }

        public void Process(TextWriter writer)
        {
            this.writer = writer;
            this._currentTmpl = _mainTmpl;

            if (_handler != null) {
                SetValue("this", _handler);
                _handler.BeforeProcess(this);
            }

            ProcessElements(_mainTmpl.Elements);

            if (_handler != null) {
                _handler.AfterProcess(this);
            }
        }

        public string Process()
        {
            StringWriter writer = new StringWriter();
            Process(writer);
            return writer.ToString();
        }

        public string Evaluate(string expression)
        {
            TmplManager Tmpl    = TmplManager.Parser("${" + expression + "}");
            StringWriter writer = new StringWriter();
            Tmpl.Process(writer);
            return writer.ToString();
        }

        public void Reset()
        {
            _variables.Clear();
        }

        private void ProcessElements(List<Token> list)
        {
            foreach (Token elem in list) {
                ProcessElement(elem);
            }
        }

        private void ProcessElement(Token elem)
        {
            if (elem is Text) {
                Text text = (Text) elem;
                WriteValue(text.Data);
            } else if (elem is Expression) {
                ProcessExpression((Expression) elem);
            } else if (elem is IfStatement) {
                ProcessIf((IfStatement) elem);
            } else if (elem is Tag) {
                ProcessTag((Tag) elem);
            }
        }

        private void ProcessExpression(Expression exp)
        {
            object value = EvalExpression(exp);
            WriteValue(value);
        }

        private object EvalExpression(Expression exp)
        {
            CurrentExpression = exp;

            try {

                if (exp is StringLiteral) {
                    return ((StringLiteral) exp).Content;
                } else if (exp is Name) {
                    return GetValue(((Name) exp).Id);
                } else if (exp is FieldAccess) {
                    FieldAccess fa = (FieldAccess) exp;
                    object obj = EvalExpression(fa.Exp);
                    string propertyName = fa.Field;
                    return ProcessProperty(obj, propertyName);

                } else if (exp is MCall) {
                    MCall ma = (MCall) exp;
                    object obj = EvalExpression(ma.CallObject);
                    string methodName = ma.Name;
                    return ProcessMCall(obj, methodName, ProcessArguments(ma.Args));

                } else if (exp is IntLiteral) {
                    return ((IntLiteral) exp).Value;
                } else if (exp is DoubleLiteral) {
                    return ((DoubleLiteral) exp).Value;
                } else if (exp is FCall) {
                    FCall fcall = (FCall) exp;

                    if (!_fnTbl.ContainsKey(fcall.Name)) {
                        string msg = string.Format("Function {0} is not defined", fcall.Name);
                        throw new TmplException(msg, exp.Line, exp.Col);
                    }

                    FunctionDefinition func = _fnTbl[fcall.Name];
                    object[] values = ProcessArguments(fcall.Args);

                    return func(values);

                } else if (exp is StringExpression) {
                    StringExpression stringExp = (StringExpression) exp;
                    StringBuilder sb = new StringBuilder();

                    foreach (Expression ex in stringExp.Expressions) {
                        sb.Append(EvalExpression(ex));
                    }

                    return sb.ToString();
                } else if (exp is BinaryExpression) {
                    return ProcessBinaryExpression(exp as BinaryExpression);
                } else if (exp is ArrayAccess) {
                    return ProcessArrayAccess(exp as ArrayAccess);
                } else {
                    throw new TmplException("Invalid expression type: " + exp.GetType().Name, exp.Line, exp.Col);
                }

            } catch (TmplException ex) {
                DisplayError(ex);
                return null;
            } catch (Exception ex) {

                string _Message = "Message=" + ex.Message + "," + "Source=" + ex.Source + ",StackTrace=" + ex.StackTrace + ",TargetSite=" + ex.TargetSite + "";
                DisplayError(new TmplException(_Message, CurrentExpression.Line, CurrentExpression.Col));
                return null;
            }
        }

        private object ProcessArrayAccess(ArrayAccess arrayAccess)
        {
            object obj   = EvalExpression(arrayAccess.Exp);
            object index = EvalExpression(arrayAccess.Index);

            if (obj is Array) {
                Array array = (Array) obj;

                if (index is int) {
                    return array.GetValue((int) index);
                } else {
                    throw new TmplException("Index of array has to be integer", arrayAccess.Line, arrayAccess.Col);
                }
            } else {
                return ProcessMCall(obj, "get_Item", new object[] { index });
            }

        }

        private object ProcessBinaryExpression(BinaryExpression exp)
        {

            object lhsValue;
            object rhsValue;
            IComparable c1;
            IComparable c2;

            switch (exp.Operator) {
            case Igs.Hcms.Tmpl.TokenKind.OpOr:

                lhsValue = EvalExpression(exp.Lhs);

                if (Util.ToBoolean(lhsValue)) {
                    return true;
                }

                rhsValue = EvalExpression(exp.Rhs);
                return Util.ToBoolean(rhsValue);

            case Igs.Hcms.Tmpl.TokenKind.OpLet:

                lhsValue = exp.Lhs;

                if (exp.Lhs is Name) {
                    string _Name = ((Name) exp.Lhs).Id;
                    rhsValue     = EvalExpression(exp.Rhs);
                    this.SetValue(_Name, rhsValue);
                    return string.Empty;

                } else if (exp.Lhs is FieldAccess) {

                    FieldAccess fa      = (FieldAccess)exp.Lhs;
                    rhsValue            = EvalExpression(exp.Rhs);
                    object obj          = EvalExpression(fa.Exp);
                    string propertyName = fa.Field;

                    setProperty(obj, propertyName, rhsValue);

                    return string.Empty;

                } else {
                    throw new TmplException("variable name." + lhsValue.ToString(), exp.Line, exp.Col);
                }

            case Igs.Hcms.Tmpl.TokenKind.OpAnd:

                lhsValue = EvalExpression(exp.Lhs);

                if (!Util.ToBoolean(lhsValue)) {
                    return false;
                }

                rhsValue = EvalExpression(exp.Rhs);
                return Util.ToBoolean(rhsValue);

            case Igs.Hcms.Tmpl.TokenKind.OpIs:

                lhsValue = EvalExpression(exp.Lhs);
                rhsValue = EvalExpression(exp.Rhs);

                c1 = lhsValue as IComparable;
                c2 = rhsValue as IComparable;

                if (c1 == null && c2 == null) {
                    return null;
                } else if (c1 == null || c2 == null) {
                    return false;
                } else if (lhsValue is int && rhsValue is int) {
                    return (int)lhsValue == (int)rhsValue;
                } else if (lhsValue is double && rhsValue is double) {
                    return (double)lhsValue == (double)rhsValue;
                } else if (lhsValue is int && rhsValue is double) {
                    return (int)lhsValue == (double)rhsValue;
                } else if (lhsValue is double && rhsValue is int) {
                    return (double)lhsValue == (int)rhsValue;
                } else if (lhsValue is string && rhsValue is string) {
                    return lhsValue.ToString() == rhsValue.ToString();
                } else {
                    return c1.CompareTo(c2) == 0;
                }

            case Igs.Hcms.Tmpl.TokenKind.OpIsNot:

                lhsValue = EvalExpression(exp.Lhs);
                rhsValue = EvalExpression(exp.Rhs);

                return !lhsValue.Equals(rhsValue);

            case Igs.Hcms.Tmpl.TokenKind.OpGt:

                lhsValue = EvalExpression(exp.Lhs);
                rhsValue = EvalExpression(exp.Rhs);

                c1 = lhsValue as IComparable;
                c2 = rhsValue as IComparable;

                if (c1 == null || c2 == null) {
                    return false;
                } else {
                    return c1.CompareTo(c2) == 1;
                }

            case Igs.Hcms.Tmpl.TokenKind.OpAdd:

                lhsValue = EvalExpression(exp.Lhs);
                rhsValue = EvalExpression(exp.Rhs);

                if (lhsValue == null || rhsValue == null) {
                    return null;
                } else if (lhsValue is int && rhsValue is int) {
                    return (int)lhsValue + (int)rhsValue;
                } else if (lhsValue is double && rhsValue is double) {
                    return (double)lhsValue + (double)rhsValue;
                } else if (lhsValue is int && rhsValue is double) {
                    return (int)lhsValue + (double)rhsValue;
                } else if (lhsValue is double && rhsValue is int) {
                    return (double)lhsValue + (int)rhsValue;
                } else if (lhsValue is string || rhsValue is string) {
                    return lhsValue.ToString() + rhsValue.ToString();
                } else {
                    return Convert.ToDouble(lhsValue) + Convert.ToDouble(rhsValue);
                }

            case Igs.Hcms.Tmpl.TokenKind.OpConcat:

                lhsValue = EvalExpression(exp.Lhs);
                rhsValue = EvalExpression(exp.Rhs);

                if (lhsValue == null || rhsValue == null) {
                    return null;
                } else {
                    return lhsValue.ToString() + rhsValue.ToString();
                }

            case Igs.Hcms.Tmpl.TokenKind.OpMul:

                lhsValue = EvalExpression(exp.Lhs);
                rhsValue = EvalExpression(exp.Rhs);

                if (lhsValue == null || rhsValue == null) {
                    return null;
                } else if (lhsValue is int && rhsValue is int) {
                    return (int)lhsValue * (int)rhsValue;
                } else if (lhsValue is double && rhsValue is double) {
                    return (double)lhsValue * (double)rhsValue;
                } else if (lhsValue is int && rhsValue is double) {
                    return (int)lhsValue * (double)rhsValue;
                } else if (lhsValue is double && rhsValue is int) {
                    return (double)lhsValue * (int)rhsValue;
                } else {
                    return Convert.ToDouble(lhsValue) * Convert.ToDouble(rhsValue);
                }

            case Igs.Hcms.Tmpl.TokenKind.OpDiv:

                lhsValue = EvalExpression(exp.Lhs);
                rhsValue = EvalExpression(exp.Rhs);

                if (lhsValue == null || rhsValue == null) {
                    return null;
                } else if (lhsValue is int && rhsValue is int) {
                    return (int)lhsValue / (int)rhsValue;
                } else if (lhsValue is double && rhsValue is double) {
                    return (double)lhsValue / (double)rhsValue;
                } else if (lhsValue is int && rhsValue is double) {
                    return (int)lhsValue / (double)rhsValue;
                } else if (lhsValue is double && rhsValue is int) {
                    return (double)lhsValue / (int)rhsValue;
                } else {
                    return Convert.ToDouble(lhsValue) / Convert.ToDouble(rhsValue);
                }

            case Igs.Hcms.Tmpl.TokenKind.OpMod:

                lhsValue = EvalExpression(exp.Lhs);
                rhsValue = EvalExpression(exp.Rhs);

                if (lhsValue == null || rhsValue == null) {
                    return null;
                } else if (lhsValue is int && rhsValue is int) {
                    return (int)lhsValue % (int)rhsValue;
                } else if (lhsValue is double && rhsValue is double) {
                    return (double)lhsValue % (double)rhsValue;
                } else if (lhsValue is int && rhsValue is double) {
                    return (int)lhsValue % (double)rhsValue;
                } else if (lhsValue is double && rhsValue is int) {
                    return (double)lhsValue % (int)rhsValue;
                } else {
                    return Convert.ToDouble(lhsValue) % Convert.ToDouble(rhsValue);
                }

            case Igs.Hcms.Tmpl.TokenKind.OpPow:

                lhsValue = EvalExpression(exp.Lhs);
                rhsValue = EvalExpression(exp.Rhs);

                if (lhsValue == null || rhsValue == null) {
                    return null;
                } else if (lhsValue is int && rhsValue is int) {
                    return Math.Pow((int)lhsValue , (int)rhsValue);
                } else if (lhsValue is double && rhsValue is double) {
                    return Math.Pow((double)lhsValue , (double)rhsValue);
                } else if (lhsValue is int && rhsValue is double) {
                    return Math.Pow((int)lhsValue , (double)rhsValue);
                } else if (lhsValue is double && rhsValue is int) {
                    return Math.Pow((double)lhsValue , (int)rhsValue);
                } else {
                    return Math.Pow(Convert.ToDouble(lhsValue) , Convert.ToDouble(rhsValue));
                }

            case Igs.Hcms.Tmpl.TokenKind.OpLt:

                lhsValue = EvalExpression(exp.Lhs);
                rhsValue = EvalExpression(exp.Rhs);

                c1 = lhsValue as IComparable;
                c2 = rhsValue as IComparable;

                if (c1 == null && c2 == null) {
                    return false;
                } else if (c1 == null || c2 == null) {
                    return false;
                } else if (lhsValue is int && rhsValue is int) {
                    return (int)lhsValue < (int)rhsValue;
                } else if (lhsValue is double && rhsValue is double) {
                    return (double)lhsValue < (double)rhsValue;
                } else if (lhsValue is int && rhsValue is double) {
                    return (int)lhsValue < (double)rhsValue;
                } else if (lhsValue is double && rhsValue is int) {
                    return (double)lhsValue < (int)rhsValue;
                } else {
                    return c1.CompareTo(c2) == -1;
                }

            case Igs.Hcms.Tmpl.TokenKind.OpGte:

                lhsValue = EvalExpression(exp.Lhs);
                rhsValue = EvalExpression(exp.Rhs);

                c1 = lhsValue as IComparable;
                c2 = rhsValue as IComparable;

                if (c1 == null && c2 == null) {
                    return false;
                } else if (c1 == null || c2 == null) {
                    return false;
                } else if (lhsValue is int && rhsValue is int) {
                    return (int)lhsValue >= (int)rhsValue;
                } else if (lhsValue is double && rhsValue is double) {
                    return (double)lhsValue >= (double)rhsValue;
                } else if (lhsValue is int && rhsValue is double) {
                    return (int)lhsValue >= (double)rhsValue;
                } else if (lhsValue is double && rhsValue is int) {
                    return (double)lhsValue >= (int)rhsValue;
                } else {
                    return c1.CompareTo(c2) >= 0;
                }

            case Igs.Hcms.Tmpl.TokenKind.OpLte:

                lhsValue = EvalExpression(exp.Lhs);
                rhsValue = EvalExpression(exp.Rhs);

                c1 = lhsValue as IComparable;
                c2 = rhsValue as IComparable;

                if (c1 == null && c2 == null) {
                    return false;
                } else if (c1 == null || c2 == null) {
                    return false;
                } else if (lhsValue is int && rhsValue is int) {
                    return (int)lhsValue <= (int)rhsValue;
                } else if (lhsValue is double && rhsValue is double) {
                    return (double)lhsValue <= (double)rhsValue;
                } else if (lhsValue is int && rhsValue is double) {
                    return (int)lhsValue <= (double)rhsValue;
                } else if (lhsValue is double && rhsValue is int) {
                    return (double)lhsValue <= (int)rhsValue;
                } else {
                    return c1.CompareTo(c2) <= 0;
                }

            default:
                throw new TmplException("Operator " + exp.Operator.ToString() + " is not supported.", exp.Line, exp.Col);
            }
        }

        private object[] ProcessArguments(Expression[] args)
        {
            object[] values = new object[args.Length];

            for (int i = 0; i < values.Length; i++) {
                values[i] = EvalExpression(args[i]);
            }

            return values;
        }

        internal static void setProperty(object obj, string propertyName, object _value)
        {
            if (obj is TypeRef) {
                Type type = (obj as TypeRef).Type;

                PropertyInfo  pinfo = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.GetProperty | BindingFlags.Static);

                if (pinfo != null) {
                    pinfo.SetValue(obj, _value, null);
                } else {

                    FieldInfo finfo = type.GetField(propertyName, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.GetField | BindingFlags.Static);

                    if (finfo != null) {
                        finfo.SetValue(null, _value);
                    } else {
                        throw new Exception("Cannot find property/field named '" + propertyName + "' in object of type '" + type.Name + "'");
                    }
                }
            } else {
                PropertyInfo pinfo = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.GetProperty | BindingFlags.Instance);

                if (pinfo != null) {
                    pinfo.SetValue(obj, _value, null);
                } else {

                    FieldInfo finfo = obj.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.GetField | BindingFlags.Instance);

                    if (finfo != null) {
                        finfo.SetValue(obj, _value);
                    } else {
                        throw new Exception("Cannot find property or field named '" + propertyName + "' in object of type '" + obj.GetType().Name + "'");
                    }
                }
            }
        }

        internal static object ProcessProperty(object obj, string propertyName)
        {
            if (obj is TypeRef) {
                Type type = (obj as TypeRef).Type;

                PropertyInfo  pinfo = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.GetProperty | BindingFlags.Static);

                if (pinfo != null) {
                    return pinfo.GetValue(null, null);
                }

                FieldInfo finfo = type.GetField(propertyName, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.GetField | BindingFlags.Static);

                if (finfo != null) {
                    return finfo.GetValue(null);
                } else {
                    throw new Exception("Cannot find property/field named '" + propertyName + "' in object of type '" + type.Name + "'");
                }
            } else {
                PropertyInfo pinfo = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.GetProperty | BindingFlags.Instance);

                if (pinfo != null) {
                    return pinfo.GetValue(obj, null);
                }

                FieldInfo finfo = obj.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.GetField | BindingFlags.Instance);

                if (finfo != null) {
                    return finfo.GetValue(obj);
                } else {
                    throw new Exception("Cannot find property or field named '" + propertyName + "' in object of type '" + obj.GetType().Name + "'");
                }
            }
        }

        private object ProcessMCall(object obj, string methodName, object[] args)
        {
            Type[] types = new Type[args.Length];

            for (int i = 0; i < args.Length; i++) {
                types[i] = args[i].GetType();
            }

            if (obj is TypeRef) {
                Type type = (obj as TypeRef).Type;
                MethodInfo method = type.GetMethod(methodName,  BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Static, null, types, null);

                if (method == null) {
                    throw new Exception(string.Format("method {0} not found for static object of type {1}", methodName, type.Name));
                }

                return method.Invoke(null, args);
            } else {

                MethodInfo method = obj.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance, null, types, null);

                if (method == null) {
                    throw new Exception(string.Format("method {0} not found for object of type {1}", methodName, obj.GetType().Name));
                }

                return method.Invoke(obj, args);
            }
        }

        private void ProcessIf(IfStatement tagIf)
        {
            bool condition = false;

            try {
                condition = Util.ToBoolean(EvalExpression(tagIf.Test));
            } catch (Exception ex) {
                DisplayError("Error evaluating condition for if statement: [" + tagIf.Test + "] " + ex.Message, tagIf.Line, tagIf.Col);
                return;
            }

            if (condition) {
                ProcessElements(tagIf.InnerElements);
            } else {
                ProcessElement(tagIf.FalseBranch);
            }
        }

        private void ProcessTag(Tag tag)
        {
            string name = tag.Name.ToLowerInvariant();

            try {
                switch (name) {
                case "define":
                    break;

                case "else":
                    ProcessElements(tag.InnerElements);
                    break;

                case "using":
                    object val = EvalExpression(tag.AttributeValue("tmpl"));
                    ProcessTmpl(val.ToString(), tag);
                    break;

                case "foreach":
                    ProcessForeach(tag);
                    break;

                case "for":
                    ProcessFor(tag);
                    break;

                default:
                    ProcessTmpl(tag.Name, tag);
                    break;
                }
            } catch (TmplException ex) {
                DisplayError(ex);
            } catch (Exception ex) {
                DisplayError("Error executing tag '" + name + "': " + ex.Message, tag.Line, tag.Col);
            }
        }

        private void ProcessForeach(Tag tag)
        {
            Expression expCollection = tag.AttributeValue("list");

            if (expCollection == null) {
                throw new TmplException("Foreach is missing required attribute: collection", tag.Line, tag.Col);
            }

            object collection = EvalExpression(expCollection);

            if (!(collection is IEnumerable)) {
                throw new TmplException("Collection used in foreach has to be enumerable", tag.Line, tag.Col);
            }

            Expression expVar = tag.AttributeValue("var");

            if (expCollection == null) {
                throw new TmplException("Foreach is missing required attribute: var", tag.Line, tag.Col);
            }

            object varObject = EvalExpression(expVar);

            if (varObject == null) {
                varObject = "foreach";
            }

            string varname = varObject.ToString();

            Expression expIndex = tag.AttributeValue("index");
            string indexname    = null;

            if (expIndex != null) {
                object obj = EvalExpression(expIndex);

                if (obj != null) {
                    indexname = obj.ToString();
                }
            }

            IEnumerator ienum = ((IEnumerable) collection).GetEnumerator();
            int index = 0;

            while (ienum.MoveNext()) {
                index++;
                object value = ienum.Current;
                _variables[varname] = value;

                if (indexname != null) {
                    _variables[indexname] = index;
                }

                ProcessElements(tag.InnerElements);
            }
        }

        private void ProcessFor(Tag tag)
        {
            Expression expFrom = tag.AttributeValue("from");

            if (expFrom == null) {
                throw new TmplException("For is missing required attribute: start", tag.Line, tag.Col);
            }

            Expression expTo = tag.AttributeValue("to");

            if (expTo == null) {
                throw new TmplException("For is missing required attribute: to", tag.Line, tag.Col);
            }

            Expression expIndex = tag.AttributeValue("index");

            if (expIndex == null) {
                throw new TmplException("For is missing required attribute: index", tag.Line, tag.Col);
            }

            object obj       = EvalExpression(expIndex);
            string indexName = obj.ToString();
            int start        = Convert.ToInt32(EvalExpression(expFrom));
            int end          = Convert.ToInt32(EvalExpression(expTo));

            for (int index = start; index <= end; index++) {
                SetValue(indexName, index);
                ProcessElements(tag.InnerElements);
            }
        }

        private void ExecuteCustomTag(Tag tag)
        {
            ITmplHandler tagHandler = customTags[tag.Name];

            bool processInnerElements = true;
            bool captureInnerContent  = false;

            tagHandler.BeforeProcess(this, tag, ref processInnerElements, ref captureInnerContent);

            string innerContent = null;

            if (processInnerElements) {
                TextWriter saveWriter = writer;

                if (captureInnerContent) {
                    writer = new StringWriter();
                }

                try {
                    ProcessElements(tag.InnerElements);

                    innerContent = writer.ToString();
                } finally {
                    writer = saveWriter;
                }
            }

            tagHandler.AfterProcess(this, tag, innerContent);

        }

        private void ProcessTmpl(string name, Tag tag)
        {
            if (customTags != null && customTags.ContainsKey(name)) {
                ExecuteCustomTag(tag);
                return;
            }

            Tmpl useTmpl = _currentTmpl.FindTmpl(name);

            if (useTmpl == null) {
                string msg = string.Format("Tmpl '{0}' not found", name);
                throw new TmplException(msg, tag.Line, tag.Col);
            }

            TextWriter saveWriter = writer;
            writer                = new StringWriter();
            string content        = string.Empty;

            try {
                ProcessElements(tag.InnerElements);

                content = writer.ToString();
            } finally {
                writer = saveWriter;
            }

            Tmpl saveTmpl           = _currentTmpl;
            _variables              = new Variable(_variables);
            _variables["innerText"] = content;

            try {
                foreach (DotAttribute attrib in tag.Attributes) {
                    object val = EvalExpression(attrib.Expression);
                    _variables[attrib.Name] = val;
                }

                _currentTmpl = useTmpl;
                ProcessElements(_currentTmpl.Elements);
            } finally {
                _variables   = _variables.Parent;
                _currentTmpl = saveTmpl;
            }
        }

        internal void WriteValue(object value)
        {
            if (_scriptMode) {

            } else {
                if (value == null) {
                    writer.Write("[null]");
                } else {
                    writer.Write(value);
                }
            }
        }

        internal void DisplayError(Exception ex)
        {

            string _Message = "Message=" + ex.Message + "," + "Source=" + ex.Source + ",StackTrace=" + ex.StackTrace + ",TargetSite=" + ex.TargetSite + "";

            if (ex is TmplException) {
                TmplException tex = (TmplException) ex;

                DisplayError(_Message, tex.Line, tex.Col);
            } else {
                DisplayError(_Message, 0, 0);
            }

        }

        internal void DisplayError(string msg, int line, int col)
        {
            if (_scriptMode) {
                throw new TmplException(msg, line, col);
            } else {

                if (!_silentErrors) {
                    writer.Write("[ERROR ({0}, {1}): {2}]", line, col, msg);
                }
            }
        }

    }
}
