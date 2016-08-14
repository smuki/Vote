namespace Volte.Bot.Tpl
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.IO;
    using System.Threading;

#if CSHARP30
    internal class EntityCompiler {
        const string ZFILE_NAME = "EntityCompiler";
        public static object _PENDING = new object();
        public static Dictionary<string, MyDelegate> Delegate_Dict = new Dictionary<string, MyDelegate>();
        private readonly StringBuilder _code = new StringBuilder();
        public delegate object MyDelegate(IDataReader _DataReader);

        public CompilerResults Compile(string code, string[] strArray, string entitykey)
        {

            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);

            string Url = path;

            if (path.IndexOf("file:\\") >= 0) {
                Url = Url.Replace("file:\\", "");
            }

            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters cp = new CompilerParameters {
                GenerateExecutable = false,
                GenerateInMemory = true,
                TreatWarningsAsErrors = false //,
//            OutputAssembly=entitykey+".cac"
            };

            cp.ReferencedAssemblies.Add("System.Data.dll");

            if (strArray != null) {
                cp.ReferencedAssemblies.AddRange(strArray);
            }

            return provider.CompileAssemblyFromSource(cp, new string[] { code });
        }

        public string GenerateCode(IDataReader _DataReader, string className)
        {
            _code.Length = 0;
            _code.AppendLine("using System;");
            _code.AppendLine("using System.Collections.Generic;");
            _code.AppendLine("using System.Data;");
            _code.AppendLine("using System.Data.Common;");
            _code.AppendLine("using System.Text;");
            _code.AppendLine("using System.Configuration;");

            _code.AppendLine("namespace Volte.Bot.Object{");

            _code.AppendLine("public class Entity" + className + ": EntityObject {");

            int fieldsCount = _DataReader.FieldCount;

            for (int i = 0; i < fieldsCount; i++) {
                string fName = _DataReader.GetName(i);
                string type = _DataReader.GetFieldType(i).ToString().Replace("System.", "");

                if (type == "DateTime") {
                    _code.AppendLine("private " + type + " _" + fName + "=DateTime.MinValue;");
                } else if (type == "decimal") {
                    _code.AppendLine("private " + type + " _" + fName + "=0;");
                } else if (type == "Int32") {
                    _code.AppendLine("private " + type + " _" + fName + "=0;");

                } else if (type == "String") {
                    _code.AppendLine("private " + type + " _" + fName + "=\"\";");
                } else {
                    _code.AppendLine("private " + type + " _" + fName + ";");
                }

                _code.AppendLine("public " + type + " " + fName + "{ get{return _" + fName + ";} set{_" + fName + "=value;} }");
            }

            _code.AppendLine("}");

            _code.AppendLine("public class Retrieve" + className + " {");

            _code.AppendLine("public static object ReadWhileLoop(IDataReader reader){");
            _code.AppendLine("List<Entity" + className + "> list=new List<Entity" + className + ">();");
            _code.AppendLine("using(reader) {");
            _code.AppendLine("while (reader.Read()) {");
            _code.AppendLine("Entity" + className + " obj;");

            int index = 0;

            for (index = 0; index < fieldsCount; index++) {
                string fName = _DataReader.GetName(index);
                string type = _DataReader.GetFieldType(index).ToString().Replace("System.", "");

                if (index == 0) {
                    _code.Append("if (!reader.IsDBNull(" + index + ")){");
                    _code.Append("obj=new Entity" + className + "{" + fName + "=reader.Get" + type + "(" + index + ")};");
                    _code.Append("}else{");
                    _code.Append("obj=new Entity" + className + "();");
                    _code.Append("}");
                } else {
                    _code.Append("if (!reader.IsDBNull(" + index + ")) ");

                    if (type == "Object") {
                        _code.Append("obj." + fName + "=reader[" + index + "];");
                    } else {
                        _code.Append("obj." + fName + "=reader.Get" + type + "(" + index + ");");
                    }
                }

                _code.AppendLine("");
            }

            _code.AppendLine("list.Add(obj);");
            _code.AppendLine("}");
            _code.AppendLine("reader.Close();");
            _code.AppendLine("}");
            _code.AppendLine("return list;");
            _code.AppendLine("}");
            _code.AppendLine("}");
            _code.AppendLine("}");
            //ZZTrace.Debug (ZFILE_NAME, _code.ToString());
            return _code.ToString();
        }

        public dynamic GetEntities(string[] strArray, IDataReader _DataReader, string entitykey)
        {
            MyDelegate handler;
            object CS_2_0001;
            bool exist = false;

            lock (_PENDING) {
                exist = Delegate_Dict.ContainsKey(entitykey);
            }

            if (!exist) {
                Parameter p = new Parameter();
                p.EntityTypeHashCode = entitykey;
                p.StrArray = strArray;
                p.DataReader = _DataReader;
                StartCompile(p);

                lock (_PENDING) {
                    handler = Delegate_Dict[entitykey];
                }

                return handler(_DataReader);
            }

            lock (_PENDING) {
                handler = Delegate_Dict[entitykey];
            }

            return handler(_DataReader);
        }

        private void StartCompile(Parameter parameter)
        {
            string entitykey = parameter.EntityTypeHashCode;
            Assembly _Assembly;

            if (File.Exists(parameter.EntityTypeHashCode + ".cac")) {
                FileStream stream = File.OpenRead(parameter.EntityTypeHashCode + ".cac");
                byte[]     buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int) stream.Length);
                stream.Close();
                _Assembly = System.Reflection.Assembly.Load(buffer);
            } else {
                CompilerResults result = Compile(GenerateCode(parameter.DataReader, entitykey), parameter.StrArray, entitykey);
                string error = null;

                if (result.Errors.Count > 0) {
                    for (int i = 0; i < result.Errors.Count; i++) {
                        error = error + "\r\n" + result.Errors[i];
                    }

                    throw new DataException(error + "\r\n");
                }

                _Assembly = result.CompiledAssembly;
            }

            string typeName = "Volte.Bot.StreamObject.Retrieve" + entitykey;
            Type type = _Assembly.GetType(typeName);

            MyDelegate handler = (MyDelegate) Delegate.CreateDelegate(typeof(MyDelegate), type, "ReadWhileLoop");

            lock (_PENDING) {
                if (!Delegate_Dict.ContainsKey(entitykey)) {
                    Delegate_Dict.Add(entitykey, handler);
                }
            }
        }


        public class Parameter {
            private IDataReader _DataReader;
            private string entitykey;
            private string[] strArray;
            private Thread th;

            public IDataReader DataReader    { get { return _DataReader;    } set { _DataReader    = value; }  }
            public string EntityTypeHashCode { get { return this.entitykey; } set { this.entitykey = value; }  }
            public string[] StrArray         { get { return this.strArray;  } set { this.strArray  = value; }  }
            public Thread Th                 { get { return this.th;        } set { this.th        = value; }  }

        }
    }
#endif
}
