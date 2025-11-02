using System.Security.Cryptography;
using System.Text;

namespace AMESA_be.common.Helpers
{
    public static class EncryptUtils
    {
        private static readonly string PasswordHash = "pp@@Sw0rD";

        private static readonly string SaltKey = "S1LT&!!23K23EY";

        private static readonly string VIKey = "@1B2c3D4e5F6g7H8";

        public static string Encrypt(string text)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                byte[] bytes2 = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(32);
#pragma warning disable SYSLIB0022 // Type or member is obsolete
                var rijndaelManaged = new RijndaelManaged
                {
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.Zeros
                };
#pragma warning restore SYSLIB0022 // Type or member is obsolete
                ICryptoTransform transform = rijndaelManaged.CreateEncryptor(bytes2, Encoding.ASCII.GetBytes(VIKey));
                byte[] inArray;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(bytes, 0, bytes.Length);
                        cryptoStream.FlushFinalBlock();
                        inArray = memoryStream.ToArray();
                        cryptoStream.Close();
                    }

                    memoryStream.Close();
                }

                return Convert.ToBase64String(inArray);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string Encrypt(string text, string passwordHash, string saltKey, string vIKey)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                byte[] bytes2 = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(32);
#pragma warning disable SYSLIB0022 // Type or member is obsolete
                var rijndaelManaged = new RijndaelManaged
                {
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.Zeros
                };
#pragma warning restore SYSLIB0022 // Type or member is obsolete
                ICryptoTransform transform = rijndaelManaged.CreateEncryptor(bytes2, Encoding.ASCII.GetBytes(VIKey));
                byte[] inArray;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(bytes, 0, bytes.Length);
                        cryptoStream.FlushFinalBlock();
                        inArray = memoryStream.ToArray();
                        cryptoStream.Close();
                    }

                    memoryStream.Close();
                }

                return Convert.ToBase64String(inArray);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string Decrypt(string encryptedText)
        {
            try
            {
                byte[] array = Convert.FromBase64String(encryptedText);
                byte[] bytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(32);
#pragma warning disable SYSLIB0022 // Type or member is obsolete
                var rijndaelManaged = new RijndaelManaged
                {
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.None
                };
#pragma warning restore SYSLIB0022 // Type or member is obsolete
                ICryptoTransform transform = rijndaelManaged.CreateDecryptor(bytes, Encoding.ASCII.GetBytes(VIKey));
                MemoryStream memoryStream = new MemoryStream(array);
                CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
                byte[] array2 = new byte[array.Length];
                int count = cryptoStream.Read(array2, 0, array2.Length);
                memoryStream.Close();
                cryptoStream.Close();
                return Encoding.UTF8.GetString(array2, 0, count).TrimEnd("\0".ToCharArray());
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string Decrypt(string encryptedText, string passwordHash, string saltKey, string vIKey)
        {
            try
            {
                byte[] array = Convert.FromBase64String(encryptedText);
                byte[] bytes = new Rfc2898DeriveBytes(passwordHash, Encoding.ASCII.GetBytes(saltKey)).GetBytes(32);
#pragma warning disable SYSLIB0022 // Type or member is obsolete
                var rijndaelManaged = new RijndaelManaged
                {
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.None
                };
#pragma warning restore SYSLIB0022 // Type or member is obsolete
                ICryptoTransform transform = rijndaelManaged.CreateDecryptor(bytes, Encoding.ASCII.GetBytes(vIKey));
                MemoryStream memoryStream = new MemoryStream(array);
                CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
                byte[] array2 = new byte[array.Length];
                int count = cryptoStream.Read(array2, 0, array2.Length);
                memoryStream.Close();
                cryptoStream.Close();
                return Encoding.UTF8.GetString(array2, 0, count).TrimEnd("\0".ToCharArray());
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
