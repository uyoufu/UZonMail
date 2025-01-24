namespace UZonMail.Utils.Http.Request
{
    public class UrlQuery(string name, string value) : Parameter(name, value, ParameterType.Query)
    {
    }
}
