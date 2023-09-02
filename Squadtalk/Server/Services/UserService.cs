using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using Squadtalk.Server.Models;
using Squadtalk.Shared;

namespace Squadtalk.Server.Services;

public class UserService
{
    private readonly AppDbContext _dbContext;
    private readonly IHashService _hashService;
    private readonly ITokenService _tokenService;

    public UserService(AppDbContext dbContext, IHashService hashService, ITokenService tokenService)
    {
        _dbContext = dbContext;
        _hashService = hashService;
        _tokenService = tokenService;
    }

    public async Task<OneOf<Success<User>, NotFound, Unauthorized>> VerifyRefreshTokenAsync(string? cookieContent)
    {
        if (cookieContent is null) return new Unauthorized();
        var cookieResult = GetFormattedCookieContent(cookieContent.AsSpan());

        if (cookieResult.IsT1) return new Unauthorized();
        var (username, token) = cookieResult.AsT0;

        var user = await CompiledQueries.UserByNameWithRefreshTokensAsync(_dbContext, username);

        if (user is null)
        {
            return new NotFound();
        }

        var isValid = _tokenService.VerifyRefreshToken(user, token);

        if (!isValid)
        {
            return new Unauthorized();
        }

        return new Success<User>(user);
    }

    public OneOf<(string username, string refreshToken), Error> GetFormattedCookieContent(ReadOnlySpan<char> content)
    {
        if (content.IsEmpty) return new Error();

        var delimiterIndex = content.LastIndexOf(' ');
        if (delimiterIndex <= 0 || content.Length - 1 == delimiterIndex)
        {
            return new Error();
        }

        var username = content[..delimiterIndex];
        var token = content[(delimiterIndex + 1)..];

        return (username.ToString(), token.ToString());
    }

    public async Task<OneOf<Success<(User, RefreshToken)>,NotFound, Unauthorized>> LoginAsync(UserCredentialsDto userCredentialsDto)
    {
        var user = await CompiledQueries.UserByNameWithRefreshTokensAsync(_dbContext, userCredentialsDto.Username);

        if (user is null)
        {
            return new NotFound();
        }

        var hash = await _hashService.HashAsync(userCredentialsDto, user.Salt);

        if (hash != user.PasswordHash)
        {
            return new Unauthorized();
        }

        var refreshToken = _tokenService.CreateRefreshToken();
        await AddRefreshToken(user, refreshToken);

        return new Success<(User, RefreshToken)>((user, refreshToken));
    }

    public async Task<OneOf<Success<(User, RefreshToken)>, Conflict, Error<string>>> RegisterAsync(UserCredentialsDto userCredentialsDto)
    {
        if (await CompiledQueries.UsernameExistsAsync(_dbContext, userCredentialsDto.Username))
        {
            return new Conflict();
        }

        var createResult = await CreateUser(userCredentialsDto);

        if (createResult.TryPickT0(out var user, out var remainder))
        {
            var refreshToken = _tokenService.CreateRefreshToken();
            await AddRefreshToken(user, refreshToken);

            return new Success<(User, RefreshToken)>((user, refreshToken));
        }

        return remainder.Match<OneOf<Success<(User, RefreshToken)>, Conflict, Error<string>>>(
            conflict => conflict,
            error => error);
    }
    
    public async Task<OneOf<User, NotFound, Error>> GetUserAsync(ClaimsPrincipal claimsPrincipal, bool 
            includeRefreshTokens = false)
    {
        var idClaim = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == JwtClaims.Uid);
        if (idClaim is null) return new NotFound();

        var usernameClaim = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == JwtClaims.Username);
        if (usernameClaim is null) return new NotFound();
        
        var parsed = Guid.TryParse(idClaim.Value, out var guid);

        if (!parsed) return new Error();
        
        var user = includeRefreshTokens
            ? await CompiledQueries.UserByGuidWithRefreshTokensAsync(_dbContext, guid)
            : await CompiledQueries.UserByGuidAsync(_dbContext, guid);

        if (user is null || user.Username != usernameClaim.Value)
        {
            return new NotFound();
        }

        return user;
    }

    private async Task<OneOf<User, Conflict, Error<string>>> CreateUser(UserCredentialsDto userCredentialsDto)
    {
        var salt = _hashService.GetSalt(16);
        var hash = await _hashService.HashAsync(userCredentialsDto, salt);

        var user = new User
        {
            Username = userCredentialsDto.Username,
            PasswordHash = hash,
            Salt = salt,
            Id = Guid.NewGuid(),
            RefreshTokens = new List<RefreshToken>()
        };

        await _dbContext.Users.AddAsync(user);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            if (await _dbContext.Users.AnyAsync(x => x.Id == user.Id))
            {
                return new Conflict();
            }

            return new Error<string>(e.Message);
        }

        return user;
    }

    private async Task AddRefreshToken(User user, RefreshToken refreshToken)
    {
        var dbToken = refreshToken.CloneAndHashData();

        user.RefreshTokens.Add(dbToken);
        _dbContext.Update(user);

        await _dbContext.SaveChangesAsync();
    }
}

public struct Unauthorized
{
}

public struct Conflict
{
}