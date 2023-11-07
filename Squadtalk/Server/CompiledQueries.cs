using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Squadtalk.Server.Models;

namespace Squadtalk.Server;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal static class CompiledQueries
{
    public static readonly Func<AppDbContext, string, Task<bool>> UsernameExistsAsync =
        EF.CompileAsyncQuery(
            (AppDbContext context, string username) => context
                .Users.Any(x => x.Username == username));

    public static readonly Func<AppDbContext, string, Task<User?>> UserByNameAsync =
        EF.CompileAsyncQuery(
            (AppDbContext context, string username) => context
                .Users.FirstOrDefault(x => x.Username == username));

    public static readonly Func<AppDbContext, string, Task<User?>> UserByNameWithRefreshTokensAsync =
        EF.CompileAsyncQuery(
            (AppDbContext context, string username) => context
                .Users.Include(x => x.RefreshTokens)
                .FirstOrDefault(x => x.Username == username));

    public static readonly Func<AppDbContext, Guid, Task<User?>> UserByGuidAsync =
        EF.CompileAsyncQuery(
            (AppDbContext context, Guid id) => context
                .Users.FirstOrDefault(x => x.Id == id));

    public static readonly Func<AppDbContext, Guid, Task<User?>> UserByGuidWithRefreshTokensAsync =
        EF.CompileAsyncQuery(
            (AppDbContext context, Guid id) => context
                .Users.Include(x => x.RefreshTokens)
                .FirstOrDefault(x => x.Id == id));

    private const int MessagePageCount = 20;

    public static readonly Func<AppDbContext, Guid, DateTimeOffset, IAsyncEnumerable<Message>>
        MessagePageByCursorAsync =
            EF.CompileAsyncQuery(
                (AppDbContext context, Guid channelId, DateTimeOffset cursor) => context
                    .Messages.OrderByDescending(m => m.Timestamp)
                    .Where(m => m.ChannelId == channelId)
                    .Where(m => m.Timestamp < cursor)
                    .Take(MessagePageCount)
                    .Include(m => m.Author)
                    .Reverse());

    public static readonly Func<AppDbContext, Guid, IAsyncEnumerable<Message>>
        MessageFirstPageAsync =
            EF.CompileAsyncQuery(
                (AppDbContext context, Guid channelId) => context
                    .Messages.OrderByDescending(m => m.Timestamp)
                    .Where(m => m.ChannelId == channelId)
                    .Take(MessagePageCount)
                    .Include(m => m.Author)
                    .Reverse());

    public static readonly Func<AppDbContext, Guid, Task<Channel?>> ChannelByIdAsync =
        EF.CompileAsyncQuery(
            (AppDbContext context, Guid channelId) => context
                .Channels.Include(x => x.Participants)
                .FirstOrDefault());
}