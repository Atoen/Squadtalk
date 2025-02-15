using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs;
using Squadtalk.Extensions;

namespace Squadtalk.Services;

public class LiveKitService
{
    private readonly ILogger<LiveKitService> _logger;

    private readonly string _issuer;
    private readonly SigningCredentials _signingCredentials;

    public LiveKitService(IConfiguration configuration, ILogger<LiveKitService> logger)
    {
        _logger = logger;
        _issuer = configuration.GetString("LiveKit:Issuer");

        var apiSecret = configuration.GetString("LiveKit:ApiSecret");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiSecret));
        _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
    }

    public RoomTokenDto CreateRoomToken(IChatUser chatUser, GroupId groupId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var now = DateTime.UtcNow;
        var videoClaim = JsonSerializer.Serialize(new { room = groupId.Value, roomJoin = true });

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _issuer,
            Subject = new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Name, chatUser.Username),
                new Claim(JwtRegisteredClaimNames.Sub, chatUser.Id),
                new Claim("video", videoClaim, JsonClaimValueTypes.Json)
            ]),
            NotBefore = now,
            SigningCredentials = _signingCredentials
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return new RoomTokenDto { Token = tokenString };
    }
}
