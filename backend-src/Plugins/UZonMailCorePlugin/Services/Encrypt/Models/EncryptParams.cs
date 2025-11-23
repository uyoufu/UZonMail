namespace UZonMail.Core.Services.Encrypt.Models
{
    public class EncryptParams
    {
        /// <summary>
        /// key
        /// </summary>
        public string Key { get; set; } = "uzonmail-key";

        /// <summary>
        /// Gets or sets the initialization vector (IV) used for cryptographic operations.
        /// </summary>
        /// <remarks>The IV should be a base64-encoded string that matches the requirements of the
        /// cryptographic algorithm in use. Supplying an incorrect or improperly sized IV may result in errors or
        /// insecure encryption.</remarks>
        public string Iv { get; set; } = "uzonmail-iv";
    }
}
