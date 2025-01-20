namespace UZonMail.Utils.Http.Request
{
    public class UrlParam : Parameter
    {
        public UrlParam(string name, string value) : base(name, value, ParameterType.Params)
        {
        }
    }
}
