using System.Net.NetworkInformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UZonMail.Utils.Network
{
    /// <summary>
    /// 基于多次 Ping 的 Ping 类
    /// </summary>
    /// <param name="host"></param>
    /// <param name="pingCount"></param>
    public class Ping2(string host, int pingCount)
    {
        private List<PingReply> _pingReplies = [];

        /// <summary>
        /// 开始进行 Ping
        /// 当成功数大于 2/3 时返回 true
        /// </summary>
        public async Task<bool> Ping()
        {
            await Task.Run(() =>
            {
                _pingReplies.Clear();
                using Ping ping = new();
                try
                {
                    Console.WriteLine($"正在 Ping {host}，次数：{pingCount}");
                    for (int i = 0; i < pingCount; i++)
                    {
                        PingReply reply = ping.Send(host, 1000); // 超时时间为 1000 毫秒
                        _pingReplies.Add(reply);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ping 过程中发生错误: {ex.Message}");
                }
            });

            return _pingReplies.Where(x => x.Status == IPStatus.Success).Count() > Math.Min(pingCount * 2 / 3, pingCount - 1);
        }

        /// <summary>
        /// 获取成功的平均响应时间
        /// </summary>
        /// <returns></returns>
        public double GetAverageRoundtripTime()
        {
            return _pingReplies.Where(x => x.Status == IPStatus.Success).Average(x => x.RoundtripTime);
        }

    }
}
