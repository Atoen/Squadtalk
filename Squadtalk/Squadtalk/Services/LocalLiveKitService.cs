using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Shared.DTOs;
using Shared.Services;
using Squadtalk.Extensions;

namespace Squadtalk.Services;

public class LocalLiveKitService : ILiveKitService
{
    private readonly string _apiKey;

    private readonly SigningCredentials _signingCredentials;

    public LocalLiveKitService(IConfiguration configuration)
    {
        _apiKey = configuration.GetString("LiveKit:ApiKey");

        var apiSecret = configuration.GetString("LiveKit:ApiSecret");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiSecret));
        _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
    }

    public Task<RoomTokenDto?> CreateRoomTokenAsync(string username, string roomName)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var now = DateTime.UtcNow;
        var expires = now.AddHours(60);
        var videoClaim = JsonSerializer.Serialize(new { room = roomName, roomJoin = true });

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _apiKey,
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim("video", videoClaim, JsonClaimValueTypes.Json)
            }),
            Expires = expires,
            NotBefore = now,
            SigningCredentials = _signingCredentials
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Task.FromResult<RoomTokenDto?>(new RoomTokenDto { Token = tokenString });
    }
}
