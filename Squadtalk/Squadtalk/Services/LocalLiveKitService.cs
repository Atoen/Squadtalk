using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Shared.Data.TypedIds;
using Shared.DTOs;
using Shared.Extensions;
using Shared.Services;
using Squadtalk.Extensions;

namespace Squadtalk.Services;

public class LocalLiveKitService : ILiveKitService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<LocalLiveKitService> _logger;

    private readonly string _issuer;
    private readonly SigningCredentials _signingCredentials;

    public LocalLiveKitService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<LocalLiveKitService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _issuer = configuration.GetString("LiveKit:Issuer");

        var apiSecret = configuration.GetString("LiveKit:ApiSecret");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiSecret));
        _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
    }

    public Task<RoomTokenDto?> CreateRoomTokenAsync(ChannelId channelId)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var username = user?.GetClaimValue(ClaimTypes.Name);

        if (string.IsNullOrEmpty(username))
        {
            _logger.LogWarning("Missing username claim required for creating room token");
            return Task.FromResult<RoomTokenDto?>(null);
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var now = DateTime.UtcNow;
        var videoClaim = JsonSerializer.Serialize(new { room = channelId.Value, roomJoin = true });

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _issuer,
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim("video", videoClaim, JsonClaimValueTypes.Json)
            }),
            NotBefore = now,
            SigningCredentials = _signingCredentials
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Task.FromResult<RoomTokenDto?>(new RoomTokenDto { Token = tokenString });
    }
}
