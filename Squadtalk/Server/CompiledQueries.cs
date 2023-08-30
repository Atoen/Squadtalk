using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Squadtalk.Server.Models;

namespace Squadtalk.Server;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal static class CompiledQueries
{
    public static readonly Func<AppDbContext, string, Task<bool>> UsernameExistsAsync = EF.CompileAsyncQuery(
        (AppDbContext context, string username) => context
            .Users.Any(x => x.Username == username));

    public static readonly Func<AppDbContext, string, Task<User?>> UserByNameAsync = EF.CompileAsyncQuery(
        (AppDbContext context, string username) => context
            .Users.FirstOrDefault(x => x.Username == username));

    public static readonly Func<AppDbContext, string, Task<User?>> UserByNameWithRefreshTokensAsync = EF.CompileAsyncQuery(
            (AppDbContext context, string username) => context
                .Users.Include(x => x.RefreshTokens)
                .FirstOrDefault(x => x.Username == username));

    public static readonly Func<AppDbContext, Guid, Task<User?>> UserByGuidAsync = EF.CompileAsyncQuery(
        (AppDbContext context, Guid id) => context
            .Users.FirstOrDefault(x => x.Id == id));

    public static readonly Func<AppDbContext, Guid, Task<User?>> UserByGuidWithRefreshTokensAsync = EF.CompileAsyncQuery(
            (AppDbContext context, Guid id) => context
                .Users.Include(x => x.RefreshTokens)
                .FirstOrDefault(x => x.Id == id));

    public static readonly Func<AppDbContext, int, IAsyncEnumerable<Message>> MessagePageAsync = EF.CompileAsyncQuery(
        (AppDbContext context, int offset) => context
            .Messages.OrderByDescending(m => m.Timestamp)
            .Skip(offset)
            .Take(20)
            .Include(m => m.Author));
}