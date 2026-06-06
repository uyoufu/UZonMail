using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace UZonMail.Utils.Helpers
{
    public class NetworkHelper
    {
        /// <summary>
        /// 获取当前主机的 IP 地址
        /// </summary>
        /// <returns></returns>
        public static List<IPAddress> GetCurrentHostIPs()
        {
            // 获取主机名
            string hostName = Dns.GetHostName();
            // 获取当前主机的 IP 地址
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            return ipEntry.AddressList.Where(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .ToList();
        }
    }
}
