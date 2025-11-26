namespace UZonMail.Utils.Web.Configs
{
    public interface IAppSettings<T>
    {
        public T Value { get; }
    }
}
