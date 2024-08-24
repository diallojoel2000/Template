
namespace Application.Common.Interfaces;

public interface IEncryptionService
{
    public string DecryptAes(string cipherText);
    public string EncryptAes(string cipherText);
    public bool IsValidId(string modelId);
    public long GetDecryptedId(string modelId);
}
