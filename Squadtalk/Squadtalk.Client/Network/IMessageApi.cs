using Refit;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs;
using Shared.Routing;

namespace Squadtalk.Client.Network;

public interface IMessageApi
{
    [Post(Routes.Endpoints.GetMessages)]
    Task<List<MessageDto>> GetMessagePage(ChannelId channelId, [AliasAs("timestamp")] TextChannelCursor? cursor, CancellationToken cancellationToken = default);

    [Post(Routes.Endpoints.CreateChannel)]
    Task<ChannelId?> CreateChannel(IEnumerable<UserId> participants, CancellationToken cancellationToken = default);
}
