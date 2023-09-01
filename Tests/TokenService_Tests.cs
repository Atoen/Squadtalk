using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Squadtalk.Server.Models;
using Squadtalk.Server.Services;
using Squadtalk.Shared;
using Xunit.Abstractions;

namespace Tests;

public class TokenService_Tests : IClassFixture<DbFixture>
{
    private readonly DbFixture _fixture;
    private readonly ITestOutputHelper _testOutputHelper;

    public TokenService_Tests(DbFixture fixture, ITestOutputHelper testOutputHelper)
    {
        _fixture = fixture;
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public void VerifyRefreshToken_ValidToken_ReturnsTrue()
    {
        var token = "validToken";

        var user = CreateUser(token);

        var tokenService = new TokenService(CreateConfiguration(), _fixture.DbContext);
        var result = tokenService.VerifyRefreshToken(user, token);
        
        Assert.True(result);
    }
    
    [Fact]
    public void VerifyRefreshToken_InvalidToken_ReturnsFalse()
    {
        var user = CreateUser();

        var tokenService = new TokenService(CreateConfiguration(), _fixture.DbContext);
        var result = tokenService.VerifyRefreshToken(user, "invalid");
        
        Assert.False(result);
    }

    [Fact]
    public void CreateRefreshToken_Success()
    {
        var tokenService = new TokenService(CreateConfiguration(), _fixture.DbContext)
        {
            RefreshTokenTimeSpan = TimeSpan.FromMinutes(1)
        };
        
        var token = tokenService.CreateRefreshToken();
        
        Assert.NotNull(token);
        Assert.NotEmpty(token.Token);

        var lifespan = token.Expires - token.Created;
        Assert.True(lifespan - tokenService.RefreshTokenTimeSpan < TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public void CreateAuthToken_Success()
    {
        var tokenService = new TokenService(CreateConfiguration(), _fixture.DbContext);

        var claims = new[]
        {
            new Claim(JwtClaims.Username, "MockUser")
        };

        var token = tokenService.CreateAuthToken(claims);
        
        Assert.NotEmpty(token);

        var tokenHandler = new JwtSecurityTokenHandler();
        var content = tokenHandler.ReadJwtToken(token);
        
        Assert.NotEmpty(content.Claims);
        Assert.Equal("iss", content.Issuer);
        Assert.Equal("aud", content.Audiences.First());
    }
    
    [Fact]
    public async Task RevokeRefreshToken_InvalidToken_ReturnsTrue()
    {
        var token = "someToken";
        var user = CreateUser(token);
        var tokenService = new TokenService(CreateConfiguration(), _fixture.DbContext);
        
        await _fixture.DbContext.Users.AddAsync(user);
        await _fixture.DbContext.SaveChangesAsync();
        
        var result = await tokenService.RevokeRefreshToken(user, token);
        
        Assert.True(result);

        var dbUser = await _fixture.DbContext.Users.Include(x => x.RefreshTokens)
            .FirstAsync(x => x.Username == user.Username);
        var dbToken = dbUser.RefreshTokens.First();
        
        Assert.False(dbToken.IsActive);
        Assert.NotNull(dbToken.Revoked);
    }
    
    [Fact]
    public async Task RevokeRefreshToken_InvalidToken_ReturnsFalse()
    {
        var token = "someToken";
        var user = CreateUser(token);
        var tokenService = new TokenService(CreateConfiguration(), _fixture.DbContext);

        await _fixture.DbContext.Users.AddAsync(user);
        await _fixture.DbContext.SaveChangesAsync();

        var result = await tokenService.RevokeRefreshToken(user, "invalidtoken");
        
        Assert.False(result);

        var dbUser = await _fixture.DbContext.Users.Include(x => x.RefreshTokens)
            .FirstAsync(x => x.Username == user.Username);
        var dbToken = dbUser.RefreshTokens.First();
        
        Assert.True(dbToken.IsActive);
        Assert.Null(dbToken.Revoked);
    }

    private IConfiguration CreateConfiguration()
    {
        var configuration = Substitute.For<IConfiguration>();
        configuration["Jwt:Issuer"].Returns("iss");
        configuration["Jwt:Audience"].Returns("aud");
        configuration["Jwt:Key"].Returns("keykeykeykeykeykeykeykeykeykeykeykeykeykeykey");

        return configuration;
    }

    private User CreateUser(string token = "validToken")
    {
        return new User
        {
            Id = default,
            Username = Guid.NewGuid().ToString(),
            Salt = Array.Empty<byte>(),
            PasswordHash = "Hash",
            RefreshTokens = new List<RefreshToken>
            {
                new()
                {
                    Token = RefreshToken.HashData(token),
                    Created = DateTime.Now,
                    Expires = DateTime.Now.AddMinutes(10)
                }
            }
        }; 
    }
}