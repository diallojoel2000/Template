using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services;

public class EncryptionService : IEncryptionService
{
    private readonly EncryptionDetails _encryptionDetials;
    public EncryptionService(IOptions<EncryptionDetails> encryptionDetails)
    {
        _encryptionDetials =encryptionDetails.Value;
    }
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
            throw new ArgumentNullException("cipherText");
        }
        if (key == null || key.Length <= 0)
        {
            throw new ArgumentNullException("key");
        }
        if (iv == null || iv.Length <= 0)
        {
            throw new ArgumentNullException("key");
        }
        // Declare the string used to hold
        // the decrypted text.
        string plaintext = null;
        // Create an RijndaelManaged object
        // with the specified key and IV.
        using (var rijAlg = new RijndaelManaged())
        {
            //Settings
            rijAlg.Mode = CipherMode.CBC;
            rijAlg.Padding = PaddingMode.PKCS7;
            // rijAlg.FeedbackSize = 128;
            rijAlg.Key = key;
            rijAlg.IV = iv;
            // Create a decrytor to perform the stream transform.
            var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

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
                plaintext = ex.Message;
            }
        }

        return plaintext;
    }
    private static string EncryptStringToBytes(string plainText, byte[] key, byte[] iv)
    {

        byte[] encrypted;

        using (var rijAlg = new RijndaelManaged())
        {
            rijAlg.Mode = CipherMode.CBC;
            rijAlg.Padding = PaddingMode.PKCS7;
            rijAlg.FeedbackSize = 128;
            rijAlg.Key = key;
            rijAlg.IV = iv;
            // Create a decrytor to perform the stream transform.
            var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);
            // Create the streams used for encryption.
            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
        }
        //Convert.ToBase64String(encrypted);
        // Return the encrypted bytes from the memory stream.
        return Convert.ToBase64String(encrypted);
    }
}
