namespace UZonMail.Core.Services.Encrypt
{
    public interface IEncryptService
    {
        string HashPassword(string hashedPwd, string salt);
        string EncrytPassword(string password);
        string DecryptPassword(string password);
    }
}
