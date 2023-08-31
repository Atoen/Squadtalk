using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Squadtalk.Shared;

namespace Squadtalk.Server.Services;

public class Argon2HashService : IHashService
{
    public int HashBytes { get; set; } = 64;
    
    public async Task<string> HashAsync(UserCredentialsDto credentialsDto, byte[] salt)
    {
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(credentialsDto.PasswordHash))
        {
            Salt = salt,
            DegreeOfParallelism = 8,
            MemorySize = 8192,
            Iterations = 8,
            AssociatedData = Encoding.UTF8.GetBytes(credentialsDto.Username)
        };

        var hash = await argon2.GetBytesAsync(HashBytes);

        return Convert.ToBase64String(hash);
    }
    
    public byte[] GetSalt(int count) => RandomNumberGenerator.GetBytes(count);
}