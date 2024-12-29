using log4net;
using Timer = System.Timers.Timer;

namespace UZonMail.Core.Services.SendCore.Utils
{
    /// <summary>
    /// 冷却器
    /// </summary>
    public class Cooler
    {
        private readonly static ILog _logger = LogManager.GetLogger(typeof(Cooler));
        private DateTime _startDate = DateTime.Now;
        private Timer? _timer = null;

        /// <summary>
        /// 是否在冷却中
        /// </summary>
        public bool IsCooling { get; private set; }

        /// <summary>
        /// 设置冷却
        /// 线程非安全
        /// </summary>
        /// <param name="cooldownMilliseconds">单位: 毫秒</param>
        /// <param name="callback">冷却结束后的回调</param></param>
        /// <returns></returns>
        public void StartCooling(long cooldownMilliseconds, Action callback)
        {
            // 说明被其它线程已经使用了
            if (IsCooling)
                return;
            IsCooling = true;

            if (cooldownMilliseconds <= 0)
            {
                return;
            }

            IsCooling = true;
            _timer?.Dispose();
            _timer = new Timer(cooldownMilliseconds)
            {
                AutoReset = false,
                Enabled = true
            };
            _timer.Elapsed += async (sender, args) =>
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
                IsCooling = false;

                // 调用回调
                callback?.Invoke();
            };
            return;
        }
    }
}
