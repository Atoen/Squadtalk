using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Squadtalk.Server.Models;
using Squadtalk.Server.Services;
using Squadtalk.Shared;

namespace Tests;

public class UserService_Tests : IClassFixture<DbFixture>
{
    private readonly DbFixture _fixture;

    public UserService_Tests(DbFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData("User sometokenstring")]
    public async Task VerifyRefreshToken_Valid_Success(string cookieToken)
    {
        var hashSubstitute = Substitute.For<IHashService>();
        var tokenSubstitute = Substitute.For<ITokenService>();
        tokenSubstitute.VerifyRefreshToken(null!, null!).ReturnsForAnyArgs(true);

        var userService = new UserService(_fixture.DbContext, hashSubstitute, tokenSubstitute);

        var token = cookieToken[(cookieToken.LastIndexOf(' ') + 1)..];
        var username = cookieToken[..cookieToken.LastIndexOf(' ')];

        var user = CreateUser(token, username);

        await _fixture.DbContext.Users.AddAsync(user);
        await _fixture.DbContext.SaveChangesAsync();

        var result = await userService.VerifyRefreshTokenAsync(cookieToken);
        Assert.True(result.IsT0);

        var dbUser = await _fixture.DbContext.Users.Include(x => x.RefreshTokens)
            .FirstAsync(x => x.Username == username);

        Assert.NotEmpty(dbUser.RefreshTokens);
        Assert.Equal(RefreshToken.HashData(token), dbUser.RefreshTokens.First().Token);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("invalidCookie")]
    [InlineData("user token ")]
    [InlineData("")]
    public async Task VerifyRefreshToken_Invalid_Unauthorized(string? cookieToken)
    {
        var hashSubstitute = Substitute.For<IHashService>();
        var tokenSubstitute = Substitute.For<ITokenService>();
        var userService = new UserService(_fixture.DbContext, hashSubstitute, tokenSubstitute);

        var result = await userService.VerifyRefreshTokenAsync(cookieToken);

        Assert.True(result.IsT2);
    }

    [Theory]
    [InlineData("InvalidUser sometokenstring")]
    public async Task VerifyRefreshToken_Invalid_NotFound(string cookieToken)
    {
        var hashSubstitute = Substitute.For<IHashService>();
        var tokenSubstitute = Substitute.For<ITokenService>();
        var userService = new UserService(_fixture.DbContext, hashSubstitute, tokenSubstitute);

        var token = cookieToken[(cookieToken.LastIndexOf(' ') + 1)..];

        var user = CreateUser(token, "otherUser");

        await _fixture.DbContext.Users.AddAsync(user);
        await _fixture.DbContext.SaveChangesAsync();

        var result = await userService.VerifyRefreshTokenAsync(cookieToken);

        Assert.True(result.IsT1);
    }

    [Theory]
    [InlineData("user token")]
    [InlineData("user withspace token")]
    public void FormatCookieContent_Valid(string content)
    {
        var hashSubstitute = Substitute.For<IHashService>();
        var tokenSubstitute = Substitute.For<ITokenService>();
        var userService = new UserService(_fixture.DbContext, hashSubstitute, tokenSubstitute);

        var result = userService.GetFormattedCookieContent(content);
        Assert.True(result.IsT0);

        var (parsedUsername, parsedToken) = result.AsT0;

        var token = content[(content.LastIndexOf(' ') + 1)..];
        var username = content[..content.LastIndexOf(' ')];

        Assert.Equal(parsedUsername, username);
        Assert.Equal(parsedToken, token);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("nospace")]
    [InlineData("user ")]
    public void FormatCookieContent_Invalid(string? content)
    {
        var hashSubstitute = Substitute.For<IHashService>();
        var tokenSubstitute = Substitute.For<ITokenService>();
        var userService = new UserService(_fixture.DbContext, hashSubstitute, tokenSubstitute);

        var result = userService.GetFormattedCookieContent(content);
        Assert.True(result.IsT1);
    }

    [Theory]
    [InlineData("LoginUser", "passhash")]
    [InlineData("Login User with spaces", "passhash")]
    public async Task Login_Valid_Success(string username, string passwordHash)
    {
        var hashSubstitute = Substitute.For<IHashService>();
        hashSubstitute.HashAsync(null!, null!).ReturnsForAnyArgs("Hash");

        var tokenSubstitute = Substitute.For<ITokenService>();
        tokenSubstitute.CreateRefreshToken().Returns(new RefreshToken
        {
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(1),
            Token = "newToken"
        });

        var userService = new UserService(_fixture.DbContext, hashSubstitute, tokenSubstitute);

        var credentials = new UserCredentialsDto
        {
            Username = username,
            PasswordHash = passwordHash
        };

        var user = CreateUser("oldToken", username);
        await _fixture.DbContext.Users.AddAsync(user);
        await _fixture.DbContext.SaveChangesAsync();

        var result = await userService.LoginAsync(credentials);
        Assert.True(result.IsT0);

        var (dbUser, refreshToken) = result.AsT0.Value;
        Assert.Equal(credentials.Username, dbUser.Username);
        Assert.Equal(2, dbUser.RefreshTokens.Count);

        Assert.All(dbUser.RefreshTokens, token => Assert.True(token.IsActive));

        Assert.Equal("newToken", refreshToken.Token);
    }

    [Theory]
    [InlineData("LoginUserInvalid", "passhash")]
    public async Task Login_Invalid_NotFound(string username, string passwordHash)
    {
        var hashSubstitute = Substitute.For<IHashService>();
        var tokenSubstitute = Substitute.For<ITokenService>();
        var userService = new UserService(_fixture.DbContext, hashSubstitute, tokenSubstitute);

        var user = CreateUser("token", "otherUser");

        await _fixture.DbContext.Users.AddAsync(user);
        await _fixture.DbContext.SaveChangesAsync();

        var credentials = new UserCredentialsDto
        {
            Username = username,
            PasswordHash = passwordHash
        };

        var result = await userService.LoginAsync(credentials);
        Assert.True(result.IsT1);
        Assert.Single(user.RefreshTokens);
    }

    [Theory]
    [InlineData("User", "passhash")]
    public async Task Login_Invalid_Unauthorized(string username, string passwordHash)
    {
        var hashSubstitute = Substitute.For<IHashService>();
        hashSubstitute.HashAsync(null!, null!).ReturnsForAnyArgs("InvalidHash");

        var tokenSubstitute = Substitute.For<ITokenService>();
        var userService = new UserService(_fixture.DbContext, hashSubstitute, tokenSubstitute);
        var user = CreateUser("token", "User");

        await _fixture.DbContext.Users.AddAsync(user);
        await _fixture.DbContext.SaveChangesAsync();

        var credentials = new UserCredentialsDto
        {
            Username = username,
            PasswordHash = passwordHash
        };

        var result = await userService.LoginAsync(credentials);
        Assert.True(result.IsT2);
        Assert.Single(user.RefreshTokens);
    }

    [Theory]
    [InlineData("RegisterUser", "passhash")]
    [InlineData("Register User with spaces", "passhash")]
    public async Task Register_Valid_Success(string username, string passwordHash)
    {
        var hashSubstitute = Substitute.For<IHashService>();
        hashSubstitute.HashAsync(null!, null!).ReturnsForAnyArgs("Hash");

        var tokenSubstitute = Substitute.For<ITokenService>();
        tokenSubstitute.CreateRefreshToken().Returns(new RefreshToken
        {
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(1),
            Token = "newToken"
        });

        var userService = new UserService(_fixture.DbContext, hashSubstitute, tokenSubstitute);
        var credentials = new UserCredentialsDto
        {
            Username = username,
            PasswordHash = passwordHash
        };

        var result = await userService.RegisterAsync(credentials);

        Assert.True(result.IsT0);

        var (dbUser, refreshToken) = result.AsT0.Value;
        Assert.NotEmpty(dbUser.RefreshTokens);
        Assert.Equal("newToken", refreshToken.Token);
        Assert.True(refreshToken.IsActive);
    }

    [Theory]
    [InlineData("RegisterUser", "passhash")]
    public async Task Register_Valid_Conflict(string username, string passwordHash)
    {
        var hashSubstitute = Substitute.For<IHashService>();
        hashSubstitute.HashAsync(null!, null!).ReturnsForAnyArgs("Hash");

        var tokenSubstitute = Substitute.For<ITokenService>();
        tokenSubstitute.CreateRefreshToken().Returns(new RefreshToken
        {
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(1),
            Token = "newToken"
        });

        var userService = new UserService(_fixture.DbContext, hashSubstitute, tokenSubstitute);
        var credentials = new UserCredentialsDto
        {
            Username = username,
            PasswordHash = passwordHash
        };

        var user = CreateUser("token", username);

        await _fixture.DbContext.Users.AddAsync(user);
        await _fixture.DbContext.SaveChangesAsync();

        var result = await userService.RegisterAsync(credentials);
        Assert.True(result.IsT1);
    }

    [Fact]
    public async Task GetUser_Success()
    {
        var hashSubstitute = Substitute.For<IHashService>();
        var tokenSubstitute = Substitute.For<ITokenService>();
        var userService = new UserService(_fixture.DbContext, hashSubstitute, tokenSubstitute);

        var user = CreateUser("token", "SomeUser");

        await _fixture.DbContext.Users.AddAsync(user);
        await _fixture.DbContext.SaveChangesAsync();

        var claims = new[]
        {
            new Claim(JwtClaims.Username, user.Username),
            new Claim(JwtClaims.Uid, user.Id.ToString())
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var result = await userService.GetUserAsync(principal);
        Assert.True(result.IsT0);
    }

    [Fact]
    public async Task GetUser_NotFound()
    {
        var hashSubstitute = Substitute.For<IHashService>();
        var tokenSubstitute = Substitute.For<ITokenService>();
        var userService = new UserService(_fixture.DbContext, hashSubstitute, tokenSubstitute);

        var user = CreateUser("token", "SomeUser");

        await _fixture.DbContext.Users.AddAsync(user);
        await _fixture.DbContext.SaveChangesAsync();

        var claims = new[]
        {
            new Claim(JwtClaims.Username, "OtherUsername"),
            new Claim(JwtClaims.Uid, Guid.NewGuid().ToString())
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var result = await userService.GetUserAsync(principal);
        Assert.True(result.IsT1);
    }

    [Fact]
    public async Task GetUser_Error()
    {
        var hashSubstitute = Substitute.For<IHashService>();
        var tokenSubstitute = Substitute.For<ITokenService>();
        var userService = new UserService(_fixture.DbContext, hashSubstitute, tokenSubstitute);

        var user = CreateUser("token", "SomeUser");

        await _fixture.DbContext.Users.AddAsync(user);
        await _fixture.DbContext.SaveChangesAsync();

        var claims = new[]
        {
            new Claim(JwtClaims.Username, user.Username),
            new Claim(JwtClaims.Uid, "invalid guid")
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var result = await userService.GetUserAsync(principal);
        Assert.True(result.IsT2);
    }

    private User CreateUser(string token, string? username = null)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = username ?? Guid.NewGuid().ToString(),
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