using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace UZonMail.Utils.Web
{
    /// <summary>
    /// Url地址的格式化和反格式化
    /// </summary>
    public class UrlAnalyze
    {
        /// <summary>
        /// 协议名称
        /// </summary>
        public string Protocol { get; set; }
        /// <summary>
        /// 是否以反斜杠结尾
        /// </summary>
        public bool Slashes { get; set; }
        /// <summary>
        /// 验证信息，暂时不使用
        /// </summary>
        public string Auth { get; set; }
        /// <summary>
        /// 全小写主机部分，包括端口
        /// </summary>
        public string Host
        {
            get
            {
                if (this.Port == null)
                    return this.HostName;
                return string.Format("{0}:{1}", this.HostName, this.Port);
            }
        }
        /// <summary>
        /// 端口，为空时http默认是80
        /// </summary>
        public int? Port { get; set; }
        /// <summary>
        /// 小写主机部分
        /// </summary>
        public string HostName { get; set; }
        /// <summary>
        /// 页面锚点参数部分 #one#two
        /// </summary>
        public string Hash { get; set; }
        /// <summary>
        /// 链接查询参数部分(带问号) '?one=1&two=2'
        /// </summary>
        public string Search { get; set; }
        /// <summary>
        /// 路径部分
        /// </summary>
        public string PathName { get; set; }
        /// <summary>
        /// 路径+参数部分(没有锚点)
        /// </summary>
        public string Path
        {
            get
            {
                if (string.IsNullOrEmpty(this.Search))
                    return this.PathName;
                return PathName + Search;
            }
        }
        /// <summary>
        /// 转码后的原链接
        /// </summary>
        public string Href { get; set; }

        /// <summary>
        /// 参数的key=value 列表
        /// </summary>
        private Dictionary<string, string> _SearchList = null;
        #region 初始化处理
        /// <summary>
        /// 空初始化
        /// </summary>
        public UrlAnalyze() { _SearchList = new Dictionary<string, string>(); }
        /// <summary>
        /// 初始化处理
        /// </summary>
        /// <param name="url">指定相对或绝对链接</param>
        public UrlAnalyze(string url)
        {
            //1.转码操作
            this.Href = HttpUtility.UrlDecode(url);
            InitParse(this.Href);
            //是否反斜杠结尾
            if (!string.IsNullOrEmpty(PathName))
                this.Slashes = this.PathName.EndsWith("/");
            //初始化参数列表
            _SearchList = GetSearchList();
        }
        /// <summary>
        /// 将字符串格式化成对象时初始化处理
        /// </summary>
        private void InitParse(string url)
        {
            //判断是否是指定协议的绝对路径
            if (url.Contains("://"))
            {
                // Regex reg = new Regex(@"(\w+):\/\/([^/:]+)(:\d*)?([^ ]*)");
                Regex reg = new Regex(@"(\w+):\/\/([^/:]+)(:\d*)?(.*)");
                Match match = reg.Match(url);
                //协议名称
                this.Protocol = match.Result("$1");
                //主机
                this.HostName = match.Result("$2");
                //端口
                string port = match.Result("$3");
                if (string.IsNullOrEmpty(port) == false)
                {
                    port = port.Replace(":", "");
                    this.Port = Convert.ToInt32(port);
                }
                //路径和查询参数
                string path = match.Result("$4");
                if (string.IsNullOrEmpty(path) == false)
                    InitPath(path);
            }
            else
            {
                InitPath(url);
            }
        }
        /// <summary>
        /// 字符串url格式化时，路径和参数的初始化处理
        /// </summary>
        /// <param name="path"></param>
        private void InitPath(string path)
        {
            Regex reg = new Regex(@"([^#?& ]*)(\??[^#]*)(#?[^?& ]*)");
            Match match = reg.Match(path);
            //路径和查询参数
            this.PathName = match.Result("$1");
            this.Search = match.Result("$2");
            this.Hash = match.Result("$3");
        }
        #endregion

        #region 参数处理
        /// <summary>
        /// 获取当前参数解析结果字典列表
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetSearchList()
        {
            if (_SearchList != null)
                return _SearchList;
            _SearchList = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(Search))
            {
                Regex reg = new Regex(@"(^|&)?(\w+)=([^&]*)", RegexOptions.Compiled);
                MatchCollection coll = reg.Matches(Search);
                foreach (Match item in coll)
                {
                    string key = item.Result("$2").ToLower();
                    string value = item.Result("$3");
                    _SearchList.Add(key, value);
                }
            }
            return _SearchList;
        }
        /// <summary>
        /// 获取查询参数的值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public string GetSearchValue(string key)
        {
            return _SearchList[key];
        }
        /// <summary>
        /// 添加参数key=value,如果值已经存在则修改
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="Encode">是否加密处理值，默认不处理</param>
        /// <returns></returns>
        public void AddOrUpdateSearch(string key, string value, bool Encode = false)
        {
            if (Encode)
                value = HttpUtility.UrlEncode(value);
            //判断指定键值是否存在
            if (_SearchList.ContainsKey(key))
            {
                _SearchList[key] = value;
            }
            else
            {
                _SearchList.Add(key, value);
            }
        }
        /// <summary>
        /// 删除指定key 的键值对
        /// </summary>
        /// <param name="key">键</param>
        public void Remove(string key)
        {
            if (_SearchList.Any(q => q.Key == key))
                _SearchList.Remove(key);
        }
        /// <summary>
        /// 获取锚点列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetHashList()
        {
            List<string> list = new List<string>();
            if (!string.IsNullOrEmpty(Hash))
            {
                list = Hash.Split('#').Where(q => string.IsNullOrEmpty(q) == false)
                    .ToList();
            }
            return list;
        }
        #endregion
        /// <summary>
        /// 获取最终url地址,
        /// 对参数值就行UrlEncode 编码后，有可能和原链接不相同
        /// </summary>
        /// <returns></returns>
        public string GetUrl(bool EncodeValue = false)
        {
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrEmpty(Protocol))
            {
                //如果有协议
                builder.Append(Protocol).Append("://");
            }
            //如果有主机标识
            builder.Append(Host);
            //如果有目录和参数
            if (!string.IsNullOrEmpty(PathName))
            {
                string pathname = PathName;
                if (pathname.EndsWith("/"))
                    pathname = pathname.Substring(0, pathname.Length - 1);
                builder.Append(pathname);
            }
            //判断是否反斜杠
            if (Slashes)
            {
                builder.Append('/');
            }
            Dictionary<string, string> searchList = GetSearchList();
            if (searchList != null && searchList.Count > 0)
            {
                builder.Append('?');
                bool isFirst = true;
                foreach (var item in searchList)
                {
                    if (isFirst == false)
                    {
                        builder.Append('&');
                    }
                    isFirst = false;
                    builder.AppendFormat("{0}={1}", item.Key, EncodeValue ? HttpUtility.UrlEncode(item.Value) : item.Value);
                }
            }
            //锚点
            builder.Append(Hash);
            return builder.ToString();
        }

        #region 静态方法
        /// <summary>
        /// 获取源字符串中所有的链接（可能有重复）
        /// </summary>
        /// <param name="content">源字符串</param>
        /// <returns></returns>
        public static List<string> GetUrlList(string content)
        {
            List<string> list = new List<string>();
            Regex re = new Regex(@"(?<url>http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?)");
            MatchCollection mc = re.Matches(content);
            foreach (Match m in mc)
            {
                if (m.Success)
                {
                    string url = m.Result("${url}");
                    list.Add(url);
                }
            }
            return list;
        }
        /// <summary>
        /// 将字符串中的链接成 a 标签
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string ReplaceToA(string content)
        {
            Regex re = new Regex(@"(?<url>http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?)");
            MatchCollection mc = re.Matches(content);
            foreach (Match m in mc)
            {
                content = content.Replace(m.Result("${url}"), String.Format("<a href='{0}'>{0}</a>", m.Result("${url}")));
            }
            return content;
        }
        #endregion
    }
}
