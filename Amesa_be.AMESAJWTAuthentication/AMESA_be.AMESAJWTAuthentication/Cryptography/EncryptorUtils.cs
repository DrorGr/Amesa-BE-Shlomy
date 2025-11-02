using AMESA_be.common.Exceptions;
using System.Security.Cryptography;
using System.Text;

namespace AMESA_be.AMESAJWTAuthentication.Cryptography
{
    public static class EncryptorUtils
    {
        public static string EncryptText(string input, string password, out string saltKey, string providedSalt = "")
        {
            byte[] encryptionKeyBytes = Encoding.UTF8.GetBytes(password);
            byte[] saltBytes = null!;

            if (!string.IsNullOrEmpty(providedSalt))
            {
                saltBytes = Convert.FromBase64String(providedSalt);
            }

            // Hash the password with SHA256
            encryptionKeyBytes = SHA256.Create().ComputeHash(encryptionKeyBytes);

            byte[] bytesEncrypted = AesEncrypt(input, encryptionKeyBytes, saltBytes!, out saltBytes);
            saltKey = Convert.ToBase64String(saltBytes);

            string result = Convert.ToBase64String(bytesEncrypted);

            return result;
        }

        public static string DecryptText(string input, string password, string saltKey)
        {
            try
            {
                if (saltKey != null)
                {
                    // Get the bytes of the string
                    byte[] bytesToBeDecrypted = Convert.FromBase64String(input);
                    byte[] encryptionKeyBytes = Encoding.UTF8.GetBytes(password);
                    byte[] saltBytes = Convert.FromBase64String(saltKey);

                    encryptionKeyBytes = SHA256.Create().ComputeHash(encryptionKeyBytes);

                    string result = AesDecrypt(bytesToBeDecrypted, encryptionKeyBytes, saltBytes);

                    return result;
                }

                return input;
            }
            catch (Exception ex)
            {
                ///TODO: GIL - WHEN EVERY USER PASS AND CONNECTIONSTRING WILL BE ENCRYPTED IN DB  
                Console.WriteLine("Decryption failed " + ex.Message);
                return input;
            }
        }

        private static byte[] AesEncrypt(string plainText, byte[] encryptionKeyBytes, byte[] providedSalt,
            out byte[] saltBytes)
        {
            byte[]? encryptedBytes = null;

            using (var AES = Aes.Create())
            {
                AES.Key = encryptionKeyBytes;
                if (providedSalt != null)
                {
                    AES.IV = providedSalt;
                }

                saltBytes = AES.IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = AES.CreateEncryptor(AES.Key, AES.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(cs))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                    }

                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }


        private static string AesDecrypt(byte[] cipherText, byte[] encryptionKeyBytes, byte[] saltBytes)
        {
            try
            {
                // Check arguments.
                if (cipherText == null || cipherText.Length <= 0)
                    throw new ArgumentNullException("cipherText");
                if (encryptionKeyBytes == null || encryptionKeyBytes.Length <= 0)
                    throw new ArgumentNullException("Key");
                if (saltBytes == null || saltBytes.Length <= 0)
                    throw new ArgumentNullException("IV");

                // Declare the string used to hold
                // the decrypted text.
                string plainText = string.Empty;

                using (var AES = Aes.Create())
                {
                    AES.Key = encryptionKeyBytes;
                    AES.IV = saltBytes;

                    // Create a decryptor to perform the stream transform.
                    ICryptoTransform decryptor = AES.CreateDecryptor(AES.Key, AES.IV);

                    using (MemoryStream ms = new MemoryStream(cipherText))
                    {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(cs))
                            {
                                // Read the decrypted bytes from the decrypting stream
                                // and place them in a string.
                                plainText = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }

                return plainText;
            }
            catch (Exception ex)
            {
                throw new CustomFaultException(ServiceError.GeneralError, "Decryption failed " + ex.Message);
            }
        }

        public static bool IsDecrypt(string input, string password, string saltKey)
        {
            string textAfterDecrypt = DecryptText(input, password, saltKey);

            if (String.Equals(textAfterDecrypt, input))
            {
                return false;
            }

            return true;
        }
    }
}
