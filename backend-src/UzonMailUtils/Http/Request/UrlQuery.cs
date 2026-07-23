namespace UzonMail.Utils.Http.Request
{
    public class UrlQuery(string name, string value) : Parameter(name, value, ParameterType.Query)
    {
    }
}
