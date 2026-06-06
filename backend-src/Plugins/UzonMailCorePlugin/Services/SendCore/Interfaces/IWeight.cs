namespace UZonMail.CorePlugin.Services.SendCore.Interfaces
{
    /// <summary>
    /// 权重接口
    /// </summary>
    public interface IWeight
    {
        /// <summary>
        /// 实际权重
        /// 实际权重必须大于 0
        /// </summary>
        public int Weight { get; }

        /// <summary>
        /// 是否可用
        /// </summary>
        public bool Enable { get; }
    }
}
