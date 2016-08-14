using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Threading;

namespace Volte.Bot.Volt
{
    public class Templates {
        public Dictionary<string, List<string>> _Regions = new Dictionary<string, List<string>>(StringComparer.CurrentCultureIgnoreCase);
        private Dictionary<string, string> _RegionNames  = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        private Dictionary<string, string> _TriggerNames = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        private Dictionary<string, bool> _RegionFiles    = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
        private Dictionary<string, int> _UsingRegion     = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);

        private object _PENDING      = new object();
        private string _appPath      = "";
        private string _DirPath      = "";
        private bool   _XmlFile      = false;
        private string _UID_CODE     = "";
        private string _templateFile = "";
        private string _codePath     = "";
        private string _debugMode    = "N";
        private string _Lang         = "02";
        private string _regionPath   = "";
        private string _extensions   = ".tpl;.cs;.shtml";

        public string AppPath      { get { return _appPath;      } set { _appPath      = value; }  }
        public string UID_CODE     { get { return _UID_CODE;     } set { _UID_CODE     = value; }  }
        public string CodePath     { get { return _codePath;     } set { _codePath     = value; }  }
        public string DebugMode    { get { return _debugMode;    } set { _debugMode    = value; }  }
        public string Lang         { get { return _Lang;         } set { _Lang         = value; }  }
        public string TemplateFile { get { return _templateFile; } set { _templateFile = value; }  }

        public Dictionary<string, string> RegionNames { get { return _RegionNames; } set { _RegionNames = value; }  }

        public Templates()
        {
            _regionPath = @"{DirPath}\{Lang};";
            _regionPath = _regionPath+ @"{DirPath}\template\{Lang};";
            _regionPath = _regionPath+ @"{AppPath}\template\{Lang};";
            _regionPath = _regionPath+ @"{AppPath}\code;";
            _regionPath = _regionPath+ @"{AppPath}\template;";
            _regionPath = _regionPath+ @"{AppPath};";
            _regionPath = _regionPath+ @"{DirPath}\code;";
            _regionPath = _regionPath+ @"{DirPath}\template;";
        }

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

        public void UsingRegion(string _RegionName , int Initialize = 1)
        {

            string _s=_RegionName.ToLower();
            if (Initialize==0){
                if (!_UsingRegion.ContainsKey(_s)) {
                    _UsingRegion[_s] = 0;
                }
            }else{
                if (_UsingRegion.ContainsKey(_s)) {
                    _UsingRegion[_s] = _UsingRegion[_s]+1;
                }else{
                    _UsingRegion[_s] = 1;
                }
            }
        }

        public Dictionary<string, int> UnUsing()
        {
            return _UsingRegion;
        }

        public string TriggerTemplate(string cUID_CODE, string sTrigger)
        {

            string _ss      = "";
            string fileName = DetectFileName(cUID_CODE + "_N_R_Template" , "N_R_Template");

            if (fileName != "T_0_1_2_3_4_5_6_7_8_9" && File.Exists(fileName)) {
                UTF8Encoding _UTF8Encoding = new UTF8Encoding(false, true);
                using(StreamReader sr = new StreamReader(fileName, _UTF8Encoding)) {
                    _ss = sr.ReadToEnd();
                }
                _ss = _ss.Replace("{sTrigger}", sTrigger);
                _ss = _ss.Replace("{UID_CODE}", cUID_CODE);
            }

            return _ss;
        }

        public string Template(string cUID_CODE, string cFileName)
        {
            string _rtv = "";

            if (File.Exists(cFileName)) {
                List<string> _Data = new List<string>();

                string _t_path = Path.GetDirectoryName(cFileName);
                _DirPath       = _t_path.Substring(0, _t_path.LastIndexOf('\\'));
                int _line      = 0;
                bool _script   = false;
                _XmlFile       = false;

                UTF8Encoding _UTF8Encoding = new UTF8Encoding(false, true);
                using(StreamReader sr = new StreamReader(cFileName, _UTF8Encoding)) {
                    string _ss;

                    while ((_ss = sr.ReadLine()) != null) {
                        string _s = _ss;
                        if (_line<=3 )
                        {
                            if (_ss.IndexOf("<?xml") >= 0) {
                                _XmlFile=true;
                            }
                        }

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
            } else {
                Console.WriteLine("Template-not found " + cFileName);
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

            if (_data.Length > 5) {
                int _bef_position = _data.IndexOf("@{") + 2;

                if (_bef_position >= 2) {
                    int _aft_position = _data.IndexOf("}", _bef_position);

                    if (_aft_position > 0) {
                        string s = _data.Substring(_bef_position, _aft_position - _bef_position);

                        s = s.Replace("$<UID_CODE>" , _UID_CODE);

                        if (s.IndexOf("&") >= 0) {
                            s = s.Replace("&", "");
                            _TriggerNames[s] = "";
                        }

                        _RegionNames[s] = s;
                        return s;
                    }
                }
            }

            return "";

        }

        public void ParseName(ref string sUID_CODE, ref string _RegionName)
        {
            int _position = _RegionName.IndexOf(".");

            if (_position > 0) {
                sUID_CODE   = _RegionName.Substring(0, _position);
                _RegionName = _RegionName.Substring(_position + 1, _RegionName.Length - _position - 1);
                _RegionName = _RegionName.ToLower();

                _RegionNames[_RegionName] = _RegionName;
            }

        }

        public string Parse(string cUID_CODE, List<string> cData)
        {
            if (_UID_CODE == "") {
                _UID_CODE = cUID_CODE;
            }

            StringBuilder _Data = new StringBuilder();

            foreach (string ss in cData) {
                string _RegionName = getRegionName(cUID_CODE, ss);

                if (_RegionName != "") {


                    string sUID_CODE = cUID_CODE;

                    this.ParseName(ref sUID_CODE,ref _RegionName);

                    List<string> _X_Region_Code = this.getRegion(sUID_CODE, _RegionName);

                    List<string> _R = this.ParseRegion(sUID_CODE, _RegionName, _X_Region_Code);

                    foreach (string s in _R) {
                        _Data.AppendLine(s);
                    }

                } else {
                    _Data.AppendLine(ss);
                }
            }

            return _Data.ToString();
        }

        public  List<string> ParseRegion(string sUID_CODE , string _RegionName , List<string> _Data)
        {

            List<string>  _Region_Data = new List<string>();

            int _f = -1;
            int _l = -1;

            for (int i = 0; i < _Data.Count && _f == -1; i++) {
                if (_Data[i] == "{") {
                    _f = i;
                }
            }

            for (int i = _Data.Count - 1; i >= 0 && _l == -1; i--) {
                if (_Data[i].Trim() == "}") {
                    _l = i;
                }
            }

            if (_XmlFile==false && DebugMode == "Y" && _RegionName.Substring(0, 1) != "_") {
                _Region_Data.Add("//<--" + sUID_CODE + "." + _RegionName);
            }

            if (_Data.Count > 0) {
                if ((_f == -1 && _l != -1) || (_f != -1 && _l == -1)) {
                    Console.WriteLine("invalid regions" + _RegionName);
                } else {
                    int _i = 0;

                    foreach (string ss in _Data) {
                        if (_i > _f && _i < _l) {
                            _Region_Data.Add(ss);
                        }
                        _i++;
                    }

                }

            }

            if (_TriggerNames.ContainsKey(_RegionName)) {
                _Region_Data.Add(TriggerTemplate(sUID_CODE, _RegionName));
            }

            if (_XmlFile==false && DebugMode == "Y" && _RegionName.Substring(0, 1) != "_") {
                _Region_Data.Add("//-->" + sUID_CODE + "." + _RegionName);
            }

            return _Region_Data;

        }

        private string DetectFileName(string cUID_CODE, string template)
        {

            string _Region_FileName = "T_0_1_2_3_4_5_6_7_8_9";

            foreach (string _path in(_regionPath + ";" + _codePath).Split(';')) {
                if (!string.IsNullOrEmpty(_path) && _Region_FileName == "T_0_1_2_3_4_5_6_7_8_9") {

                    string spath = _path.Replace("{AppPath}" , _appPath);
                    spath        = spath.Replace("{DirPath}" , _DirPath);
                    spath        = spath.Replace("{Lang}"    , this.Lang);
                    spath        = spath.Trim('\\') + "\\";
                    spath        = spath.Replace(@"\\" , @"\");

                    if (Directory.Exists(spath)){

                        foreach (string _Extension in _extensions.Split(';')) {

                            if (_Region_FileName == "T_0_1_2_3_4_5_6_7_8_9"){

                                if (File.Exists(spath + cUID_CODE + _Extension)) {
                                    _Region_FileName = spath + cUID_CODE + _Extension;
                                } else if (File.Exists(spath + template + _Extension)) {
                                    _Region_FileName = spath + template + _Extension;
                                }

                            }
                        }
                    }
                }
            }

            return _Region_FileName;
        }

        public List<string> getRegion(string cUID_CODE, string cRegion)
        {
            string _RegionName  = (cUID_CODE + "_t_" + cRegion).ToLower();
            List<string>  _RegionCode = new List<string>();


            lock (_PENDING) {

                if (_Regions.ContainsKey(_RegionName)) {

                    _RegionCode = _Regions[_RegionName];
                    UsingRegion(_RegionName);

                } else {

                    string _Region_FileName = DetectFileName(cUID_CODE , TemplateFile);

                    bool _NeedParse = true;

                    if (_RegionFiles.ContainsKey(_Region_FileName)) {
                        _NeedParse = _RegionFiles[_Region_FileName];
                    }

                    if (_NeedParse && _Region_FileName != "T_0_1_2_3_4_5_6_7_8_9" && File.Exists(_Region_FileName)) {

                        List<string> _FileData    = new List<string>();
                        List<string> _Region_Data = new List<string>();
                        _Regions[_RegionName]     = _RegionCode;

                        string _region_name = "";
                        int    _l           = 0;

                        UTF8Encoding _UTF8Encoding = new UTF8Encoding(false, true);
                        using(StreamReader sr = new StreamReader(_Region_FileName, _UTF8Encoding)) {
                            string c = "";

                            while ((c = sr.ReadLine()) != null) {

                                string cc = c.TrimStart(' ');

                                if (cc.IndexOf("#region<T>") == 0) {

                                    string ss    = cc.Replace("#region<T>", "");
                                    _region_name = ss.Replace(" ", "");
                                    _Region_Data = new List<string>();

                                } else if (cc.IndexOf("#endregion</T>") == 0 && _region_name == "") {

                                    Console.WriteLine(_Region_FileName + " invlid region " + cc + " in " + _l);

                                } else if (cc.IndexOf("#endregion</T>") == 0 && _region_name != "") {

                                    _Region_Data.Add("//=====******" +_Region_FileName + cUID_CODE);
                                    _Regions[(cUID_CODE + "_t_" + _region_name).ToLower()] = _Region_Data;

                                    UsingRegion(cUID_CODE + "_t_" + _region_name , 0);

                                    _region_name  = "";
                                    _l            = 0;

                                } else {

                                    string _t_name = this.getRegionName(cUID_CODE, cc);

                                    if (_t_name != "") {

                                        string sUID_CODE = _UID_CODE;

                                        this.ParseName(ref sUID_CODE,ref _t_name);

                                        string _name = (sUID_CODE + "_t_" + _t_name).ToLower();

                                        List<string> _X_Region_Code = new List<string>();

                                        if (_Regions.ContainsKey(_name)) {

                                            _X_Region_Code = _Regions[_name];

                                        } else if (_name.ToLower() != _RegionName.ToLower()) {

                                            _X_Region_Code = this.getRegion(sUID_CODE, _t_name);

                                        }

                                        if (_X_Region_Code.Count > 0) {

                                            List<string> _R = this.ParseRegion(sUID_CODE, _t_name, _X_Region_Code);


                                            foreach (string ss in _R) {

                                                _Region_Data.Add(ss);
                                            }

                                        } else {
                                            _Regions[_name] = new List<string>();;
                                            UsingRegion(_name , 0);
                                        }
                                    } else {
                                        _Region_Data.Add(cc);
                                        _l++;
                                    }
                                }
                            }
                        }
                    }

                    _RegionFiles[_Region_FileName] = false;

                    if (_Regions.ContainsKey(_RegionName)) {
                        _RegionCode = _Regions[_RegionName];
                        UsingRegion(_RegionName);
                    } else {
                        _Regions[_RegionName] = new List<string>();;
                    }

                }

            }

            return _RegionCode;
        }
    }
}
