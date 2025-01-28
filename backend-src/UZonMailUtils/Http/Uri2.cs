using System;
using System.Diagnostics.CodeAnalysis;

namespace UZonMail.Utils.Http
{
    public class Uri2 : Uri
    {
        public UriUserInfo UserInfo2 { get; private set; }

        public Uri2([StringSyntax("Uri")] string uriString) : base(uriString)
        {
            UserInfo2 = new UriUserInfo(UserInfo);
        }
    }
}
