using System;

namespace Uamazing.Utils.Envs
{
    public class Env
    {
#if DEBUG
        /// <summary>
        /// 是否是调试模式
        /// </summary>
        public static bool IsDebug { get; } = true;
#else
        public static bool IsDebug
        {
            get
            {
                // 从环境变量: DebugUZonMail 中获取值
                var debug = Environment.GetEnvironmentVariable("DebugUZonMail");
                if (bool.TryParse(debug, out var value)) return value;
                return false;
            }
        }
#endif
    }
}
