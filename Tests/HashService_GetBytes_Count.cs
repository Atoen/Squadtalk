using Squadtalk.Server.Services;
using Squadtalk.Shared;

namespace Tests;

public class HashService_GetBytes
{
    [Fact]
    public async Task HashAsync_GeneratesHash()
    {
        var credentialsDto = new UserCredentialsDto
        {
            Username = "testuser",
            PasswordHash = "password123"
        };

        var hashService = new Argon2HashService();
        var salt = hashService.GetSalt(16);

        var hash = await hashService.HashAsync(credentialsDto, salt);

        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(12)]
    [InlineData(16)]
    public void GetSalt_Count(int count)
    {
        var hashService = new Argon2HashService();
        var salt = hashService.GetSalt(count);

        Assert.Equal(count, salt.Length);
    }

    [Theory]
    [InlineData(32)]
    [InlineData(48)]
    [InlineData(64)]
    public async Task GetBytes_Count(int count)
    {
        var credentials = new UserCredentialsDto
        {
            Username = "MockUser",
            PasswordHash = "24b602f63fc85e67778f6599b1353e50ccf2a09bcc5d3ee29487b9e39979b975"
        };

        var hashService = new Argon2HashService
        {
            HashBytes = count
        };

        var salt = hashService.GetSalt(16);
        var hash = await hashService.HashAsync(credentials, salt);
        var bytes = Convert.FromBase64String(hash);

        Assert.Equal(hashService.HashBytes, bytes.Length);
    }


    [Fact]
    public async Task HashAsync_IdenticalCredentialsProduceDifferentHashes()
    {
        var credentials = new UserCredentialsDto
        {
            Username = "testuser",
            PasswordHash = "password123"
        };

        var hashService = new Argon2HashService();

        var hash1 = await hashService.HashAsync(credentials, hashService.GetSalt(16));
        var hash2 = await hashService.HashAsync(credentials, hashService.GetSalt(16));

        Assert.NotNull(hash1);
        Assert.NotEmpty(hash1);
        Assert.NotNull(hash2);
        Assert.NotEmpty(hash2);
        Assert.NotEqual(hash1, hash2);
    }
}