namespace UZonMail.Utils.Http
{
    public class BrowserMock
    {
        public static string GetBrowserName()
        {
            return "Mozilla";
        }

        public static string GetBrowserChromeVersion()
        {
            return "5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0";
        }

        /// <summary>
        /// 获取 Win10 Chrome 浏览器的 UserAgent
        /// </summary>
        /// <returns></returns>
        public static string GetChromeUserAgent()
        {
            return $"{GetBrowserName()}/{GetBrowserChromeVersion()}";
        }

        /// <summary>
        /// 获取 sec-ch-ua
        /// 与 UserAgent 保持一致
        /// </summary>
        /// <returns></returns>
        public static string GetSecChUa()
        {
            return "\"Microsoft Edge\";v=\"131\", \"Chromium\";v=\"131\", \"Not_A Brand\";v=\"24\"";
        }

        public static string GetClientABVersions()
        {
            return "71960613,70508271,72437276,72920976,72923695,72961679,73038833,73122399,73179188,73216053,73230562,73234258,73242625,73242630,73262086,73273317,73294937,73297225,70138197,70156809,70405643,71057832,71200802,71381811,71516509,71803300,71962127,72258247,72360691,72408100,72854054,72892778,73004916,73171280,73208420,73233984";
        }
    }
}
