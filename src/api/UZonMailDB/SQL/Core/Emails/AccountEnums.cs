namespace UzonMail.DB.SQL.Core.Emails;

public enum SendingProtocol
{
    Smtp = 0,
    MicrosoftGraph = 1,
}

public enum ReceivingProtocol
{
    Imap = 0,
    MicrosoftGraph = 1,
}

public enum AuthenticationMethod
{
    Password = 0,
    OAuth2 = 1,
}

public enum OAuthProvider
{
    Generic = 0,
    Google = 1,
    Microsoft = 2,
}

public enum OAuthApplicationSource
{
    System = 0,
    Custom = 1,
}

public enum SenderAccountStatus
{
    Unverified = 0,
    Valid = 1,
    Invalid = 2,
}

public enum RecipientValidationStatus
{
    Unverified = 0,
    Invalid = 1,
    Unknown = 2,
    Valid = 3,
}

public enum ReceivingAccountStatus
{
    Unverified = 0,
    Active = 1,
    Paused = 2,
    AuthenticationFailed = 3,
    ConnectionFailed = 4,
}

public enum EmailGroupCategory
{
    EmailAccount = 1,
    RecipientEmail = 2,
}
