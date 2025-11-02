namespace AMESA_be.AMESAJWTAuthentication.Config
{
    public class CryptographyConfig
    {
        public Boolean EncriptionEnabled { get; set; }
        public AES Aes { get; set; }
    }

    public class AES
    {
        public string key { get; set; }
        public string salt { get; set; }
    }
}
