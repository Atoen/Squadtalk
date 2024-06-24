using Shared.Data.TypedIds;

namespace Shared.Services;

public interface ICreateTextChannelRequestHandler
{
    Task<ChannelId?> CreateTextChannelAsync(List<UserId> participants);
}