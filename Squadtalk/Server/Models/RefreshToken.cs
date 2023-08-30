using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Squadtalk.Server.Models;

[Owned]
public class RefreshToken
{
    public required string Token { get; set; }       
    public required DateTime Created { get; set; }
    public required DateTime Expires { get; set; }
    public DateTime? Revoked { get; set; }
    public bool IsExpired => DateTime.UtcNow >= Expires;
    public bool IsActive => Revoked == null && !IsExpired;

    public RefreshToken CloneAndHashData() => new()
    {
        Created = Created,
        Expires = Expires,
        Token = HashData(Token)
    };

    public static string HashData(string token)
    {
        var data = Encoding.UTF8.GetBytes(token);
        return Convert.ToBase64String(SHA256.HashData(data));
    }

    public override string ToString() => Token;
}