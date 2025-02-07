using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services;

public class EncryptionService : IEncryptionService
{
    //Remove and know peace
    //private readonly EncryptionDetails _encryptionDetials;
    //public EncryptionService(IOptions<EncryptionDetails> encryptionDetails)
    //{
    //    _encryptionDetials =encryptionDetails.Value;
    //}
    public string DecryptAes(string cipherText)
    {
        var secretkey = Encoding.UTF8.GetBytes(_encryptionDetials.ClientKey);
        var ivKey = Encoding.UTF8.GetBytes(_encryptionDetials.ClientSalt);

        var encrypted = Convert.FromBase64String(cipherText);
        var decriptedFromJavascript = DecryptStringFromBytes(encrypted, secretkey, ivKey);
        return decriptedFromJavascript;
    }
    public string EncryptAes(string plainText)
    {
        var secretkey = Encoding.UTF8.GetBytes(_encryptionDetials.ClientKey);
        var ivKey = Encoding.UTF8.GetBytes(_encryptionDetials.ClientSalt);

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedFromJavascript = EncryptStringToBytes(plainText, secretkey, ivKey);
        return encryptedFromJavascript;
    }
    public bool IsValidId(string modelId)
    {
        long id = 0;
        long.TryParse(DecryptAes(modelId), out id);
        if (id == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    public long GetDecryptedId(string modelId)
    {
        long id = 0;
        long.TryParse(DecryptAes(modelId), out id);
        return id;
    }
    private static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
    {
        // Check arguments.
        if (cipherText == null || cipherText.Length <= 0)
        {
            throw new ArgumentNullException(nameof(cipherText));
        }
        if (key == null || key.Length <= 0)
        {
            throw new ArgumentNullException(nameof(key));
        }
        if (iv == null || iv.Length <= 0)
        {
            throw new ArgumentNullException(nameof(iv));
        }

        // Declare the string used to hold the decrypted text.
        string plaintext = string.Empty;

        // Create an Aes object with the specified key and IV.
        using (var aesAlg = Aes.Create())
        {
            // Set the key and IV
            aesAlg.Key = key;
            aesAlg.IV = iv;

            // Optional: Set additional settings (Mode, Padding)
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            // Create a decryptor to perform the stream transform.
            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            try
            {
                // Create the streams used for decryption.
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                plaintext = ex.Message; // Return the exception message if decryption fails
            }
        }

        return plaintext;
    }
    private static string EncryptStringToBytes(string plainText, byte[] key, byte[] iv)
    {
        // Check arguments.
        if (string.IsNullOrEmpty(plainText))
        {
            throw new ArgumentNullException(nameof(plainText));
        }
        if (key == null || key.Length <= 0)
        {
            throw new ArgumentNullException(nameof(key));
        }
        if (iv == null || iv.Length <= 0)
        {
            throw new ArgumentNullException(nameof(iv));
        }

        byte[] encrypted;

        // Create an Aes object with the specified key and IV.
        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;

            // Optional: Set additional settings (Mode, Padding)
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            // Create an encryptor to perform the stream transform.
            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        // Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
        }

        // Return the encrypted bytes as a base64 string.
        return Convert.ToBase64String(encrypted);
    }
}
