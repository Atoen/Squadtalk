using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Shared.Data.TypedIds;
using Shared.DTOs;
using Shared.Extensions;
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

    public RoomTokenDto? CreateRoomToken(string username)
    {
        // var username = claimsPrincipal?.GetClaimValue(ClaimTypes.Name);
        // var id = claimsPrincipal?.GetClaimValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(username))
        {
            _logger.LogWarning("Missing required claims for creating room token");
            return null;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var now = DateTime.UtcNow;
        var videoClaim = JsonSerializer.Serialize(new { room = "mega room", roomJoin = true });

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _issuer,
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Name, username),
                // new Claim(JwtRegisteredClaimNames.Sub, id),
                new Claim("video", videoClaim, JsonClaimValueTypes.Json)
            }),
            NotBefore = now,
            SigningCredentials = _signingCredentials
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return new RoomTokenDto { Token = tokenString };
    }

    public RoomTokenDto? CreateRoomToken(ClaimsPrincipal? claimsPrincipal, ChannelId channelId)
    {
        var username = claimsPrincipal?.GetClaimValue(ClaimTypes.Name);
        var id = claimsPrincipal?.GetClaimValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(id))
        {
            _logger.LogWarning("Missing required claims for creating room token");
            return null;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var now = DateTime.UtcNow;
        var videoClaim = JsonSerializer.Serialize(new { room = channelId.Value, roomJoin = true });

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _issuer,
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Name, username),
                new Claim(JwtRegisteredClaimNames.Sub, id),
                new Claim("video", videoClaim, JsonClaimValueTypes.Json)
            }),
            NotBefore = now,
            SigningCredentials = _signingCredentials
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return new RoomTokenDto { Token = tokenString };
    }
}
