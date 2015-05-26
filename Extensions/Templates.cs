using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Threading;

namespace Igs.Hcms.Tmpl
{
    public class Templates {
        private Dictionary<string, List<string>> _Regions = new Dictionary<string, List<string>>();

        private object _PENDING    = new object();
        private string _appPath    = "";
        private string _templateFile    = "";
        private string _codePath   = "";
        private string _debugMode  = "N";
        private string _regionPath = @"{AppPath}\code;{AppPath}\template;{AppPath}";
        private string _Extensions = ".tpl;.cs;.shtml";

        public string AppPath      { get { return _appPath;      } set { _appPath      = value; }  }
        public string CodePath     { get { return _codePath;     } set { _codePath     = value; }  }
        public string DebugMode    { get { return _debugMode;    } set { _debugMode    = value; }  }
        public string TemplateFile { get { return _templateFile; } set { _templateFile = value; }  }

        public string KeepSingleEmptyLine(string data)
        {

            data = data.Replace("\r", "\n");
            data = data.Replace("\n\n", "\n");

            StringBuilder _Data = new StringBuilder();

            bool isblank = false;

            foreach (string _s in data.Split('\n')) {
                if (string.IsNullOrEmpty(_s.Trim())) {
                    if (!isblank) {
                        _Data.Append("\n");
                    }

                    isblank = true;
                } else {
                    isblank = false;
                    _Data.Append(_s);
                    _Data.Append("\n");
                }
            }

            return _Data.ToString();
        }

        public string PolicyTemplate(string UID_CODE, string sTrigger)
        {

            string _ss;
            string fileName = "";

            foreach (string _path in(_regionPath + ";" + _codePath).Split(';')) {
                if (!string.IsNullOrEmpty(_path)) {
                    string spath = _path.Replace("{AppPath}", _appPath);
                    spath = spath.Trim('\\') + "\\";

                    if (Directory.Exists(spath)) {

                        foreach (string _Extension in _Extensions.Split(';')) {
                            if (File.Exists(spath + "N_R_Template" + _Extension)) {
                                fileName = spath + "N_R_Template" + _Extension;
                            }
                        }
                    }
                }
            }
            using(StreamReader sr = new StreamReader(fileName, System.Text.Encoding.Default)) {
                _ss = sr.ReadToEnd();
            }
            _ss = _ss.Replace("{sTrigger}", sTrigger);
            _ss = _ss.Replace("{UID_CODE}", UID_CODE);
            return _ss;
        }

        public string Template(string cUID_CODE, string cFileName)
        {
            string _rtv = "";

            if (File.Exists(cFileName)) {
                string _t_path = Path.GetDirectoryName(cFileName);
                _appPath       = _t_path.Substring(0, _t_path.LastIndexOf('\\'));

                List<string> _Data = new List<string>();
                int _line          = 0;
                bool _script       = false;

                using(StreamReader sr = new StreamReader(cFileName, System.Text.Encoding.Default)) {
                    string _ss;

                    while ((_ss = sr.ReadLine()) != null) {
                        string _s = _ss;

                        if (_script && _s.Trim() != "" && !((_s.Trim().IndexOf("@") == 0 && _s.IndexOf("@@") < 0) || _s.Trim().IndexOf("}~") == 0)) {
                            _s = "${" + _s + "}";
                        }

                        if (_line <= 5) {
                            if (_ss.IndexOf("@script") < 0) {
                                _Data.Add(_s);
                            } else {
                                _script = true;
                            }
                        } else {
                            _Data.Add(_s);
                        }

                        _line++;
                    }
                }
                _rtv = Parse(cUID_CODE, _Data);
            }

            return _rtv ;
        }

        public string Template(string cUID_CODE, List<string> cData)
        {
            return Parse(cUID_CODE, cData);
        }

        public string Template(string cUID_CODE, ArrayList cData)
        {
            List<string> _Data = new List<string>();

            foreach (string s in cData) {
                _Data.Add(s);
            }

            return Parse(cUID_CODE, _Data);
        }

        private string getRegionName(string cUID_CODE, string _data)
        {
            if (string.IsNullOrEmpty(_data)) {
                return "";
            }

            int _bef_position = _data.IndexOf("@{") + 2;
            int _aft_position = _data.IndexOf("}", _bef_position);

            if (_bef_position >= 2 && _aft_position > 0) {
                string s = _data.Substring(_bef_position, _aft_position - _bef_position);

                if (s.IndexOf("&") >= 0) {
                    StreamWriter _File = new StreamWriter("Rules.ini", true);
                    _File.WriteLine(cUID_CODE + "|" + s);
                    _File.Close();
                }

                return s;
            } else {
                return "";
            }

        }

        private string Parse(string cUID_CODE, List<string> cData)
        {
            StringBuilder _Data = new StringBuilder();

            foreach (string ss in cData) {
                string _RegionName = getRegionName(cUID_CODE, ss);

                if (_RegionName != "") {
                    bool Policy = _RegionName.IndexOf("&") >= 0;

                    if (Policy) {
                        _RegionName = _RegionName.Replace("&", "");
                    }

                    string _UID_CODE = cUID_CODE;
                    int    _position = _RegionName.IndexOf(".");

                    if (_position > 0) {
                        _UID_CODE   = _RegionName.Substring(0, _position);
                        _RegionName = _RegionName.Substring(_position + 1, _RegionName.Length - _position - 1);
                        _RegionName = _RegionName.ToUpper();
                    }

                    List<string> _X_Region_Code = this.getRegion(_UID_CODE, _RegionName);

                    if (DebugMode == "Y" && _UID_CODE.ToLower() != "include" && _RegionName.ToLower() != "init_X_Codetdriver") {
                        _Data.AppendLine("//<--" + _UID_CODE + "." + _RegionName);
                    }

                    if (_X_Region_Code.Count > 0) {
                        int _ttl = 0;

                        for (int i = 1; i < _X_Region_Code.Count && _ttl == 0; i++) {
                            if (_X_Region_Code[_X_Region_Code.Count - i] == "}") {
                                _ttl = _X_Region_Code.Count - i;
                            }
                        }

                        bool _w_start_flag = false;
                        int  _end_line     = 0;

                        foreach (string _ss in _X_Region_Code) {
                            if (_w_start_flag && _end_line != _ttl) {
                                _Data.AppendLine(_ss);
                            }

                            if (_ss == "{") {
                                _w_start_flag = true;
                            }

                            _end_line++;
                        }
                    }

                    if (Policy) {
                        _Data.AppendLine(PolicyTemplate(_UID_CODE, _RegionName));
                    }

                    if (DebugMode == "Y" && _UID_CODE.ToLower() != "include" && _RegionName.ToLower() != "init_X_Codetdriver") {
                        _Data.AppendLine("//-->" + _UID_CODE + "." + _RegionName);
                    }

                } else {
                    _Data.AppendLine(ss);
                }
            }

            return _Data.ToString();
        }

        private List<string> getRegion(string cUID_CODE, string cRegion)
        {
            string _RegionName  = (cUID_CODE + "_t_" + cRegion).ToLower();
            List<string>  _RegionCode = new List<string>();

            lock (_PENDING) {

                if (_Regions.ContainsKey(_RegionName)) {

                    _RegionCode = _Regions[_RegionName];

                } else {
                    string _Region_File_Name = "x_x_x_x";

                    foreach (string _path in(_regionPath + ";" + _codePath).Split(';')) {
                        if (!string.IsNullOrEmpty(_path)) {
                            string spath = _path.Replace("{AppPath}", _appPath);
                            spath = spath.Trim('\\') + "\\";

                            if (Directory.Exists(spath)) {

                                foreach (string _Extension in _Extensions.Split(';')) {
                                    if (File.Exists(spath + cUID_CODE + _Extension)) {
                                        _Region_File_Name = spath + cUID_CODE + _Extension;
                                    } else if (File.Exists(spath + TemplateFile + _Extension)) {
                                        _Region_File_Name = spath + TemplateFile + _Extension;
                                    }
                                }
                            }
                        }
                    }

                    if (_Region_File_Name != "x_x_x_x" && File.Exists(_Region_File_Name)) {

                        List<string> _FileData    = new List<string>();
                        List<string> _Region_Data = new List<string>();
                        _Regions[_RegionName]     = _RegionCode;

                        string _region_name = "";
                        int    _l           = 0;

                        using(StreamReader sr = new StreamReader(_Region_File_Name, System.Text.Encoding.Default)) {
                            string c = "";

                            while ((c = sr.ReadLine()) != null) {

                                string cc = c.TrimStart(' ');

                                if (cc.IndexOf("#region<T>") == 0) {

                                    string ss    = cc.Replace("#region<T>", "");
                                    _region_name = ss.Replace(" ", "");
                                    _Region_Data = new List<string>();

                                } else if (cc.IndexOf("#endregion</T>") == 0 && _region_name == "") {

                                    Console.WriteLine(_Region_File_Name + " invlid region " + cc + " in " + _l);

                                } else if (cc.IndexOf("#endregion</T>") == 0 && _region_name != "") {

                                    _Regions[(cUID_CODE + "_t_" + _region_name).ToLower()] = _Region_Data;
                                    _region_name  = "";
                                    _l            = 0;

                                } else {

                                    string _t_name = this.getRegionName(cUID_CODE, cc);

                                    if (_t_name != "") {

                                        bool Policy = _RegionName.IndexOf("&") >= 0;

                                        if (Policy) {
                                            _RegionName = _RegionName.Replace("&", "");
                                        }

                                        string _UID_CODE = cUID_CODE;
                                        int _position    = _t_name.IndexOf(".");

                                        if (_position > 0) {
                                            _UID_CODE = _t_name.Substring(0, _position);
                                            _t_name   = _t_name.Substring(_position + 1, _t_name.Length - _position - 1);
                                            _t_name   = _t_name.ToUpper();
                                        }

                                        string _name = (_UID_CODE + "_t_" + _t_name).ToLower();

                                        List<string> _X_Region_Code = new List<string>();

                                        if (_Regions.ContainsKey(_name)) {

                                            _X_Region_Code = _Regions[_name];

                                        } else if (_name.ToLower() != _RegionName.ToLower()) {

                                            _X_Region_Code = this.getRegion(_UID_CODE, _t_name);

                                        }

                                        if (_X_Region_Code.Count > 0) {
                                            if (DebugMode == "Y") {
                                                _Region_Data.Add("//<--" + _UID_CODE + "." + _t_name);
                                            }

                                            bool _w_start_flag = false;
                                            int  _end_line     = 0;
                                            int _ttl           = 0;

                                            for (int i = 1; i < _X_Region_Code.Count && _ttl == 0; i++) {
                                                if (_X_Region_Code[_X_Region_Code.Count - i] == "}") {
                                                    _ttl = _X_Region_Code.Count - i;
                                                }
                                            }

                                            foreach (string ss in _X_Region_Code) {
                                                if (_w_start_flag && _end_line != _ttl) {
                                                    _Region_Data.Add(ss);
                                                }

                                                if (ss == "{") {
                                                    _w_start_flag = true;
                                                }

                                                _end_line++;
                                            }

                                            if (Policy) {
                                                _Region_Data.Add(PolicyTemplate(_UID_CODE, _RegionName));
                                            }

                                            if (DebugMode == "Y") {
                                                _Region_Data.Add("//-->" + _UID_CODE + "." + _t_name);
                                            }
                                        }
                                    } else {
                                        _Region_Data.Add(cc);
                                        _l++;
                                    }
                                }
                            }
                        }
                    }

                    if (_Regions.ContainsKey(_RegionName)) {
                        _RegionCode = _Regions[_RegionName];
                    }

                }

            }

            return _RegionCode;
        }
    }
}
