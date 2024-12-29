namespace UZonMail.Core.Services.SendCore.Interfaces
{
    public interface ISendingThreadsManager
    {
        /// <summary>
        /// 开始发送
        /// </summary>
        /// <param name="activeCount"></param>
        void StartSending(int activeCount);
    }
}
