using Refit;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.DTOs.Chat.Results;
using Shared.Routing;

namespace Squadtalk.Client.Network;

public interface IChatApi
{
    [Get(Routes.Endpoints.ChatController + "{channelId}/{timestamp}")]
    Task<List<MessageDto>> GetMessagePage(ChannelId channelId, [AliasAs("timestamp")] TextChannelCursor cursor, CancellationToken cancellationToken = default);

    [Get(Routes.Endpoints.ChatController + "{channelId}")]
    Task<List<MessageDto>> GetMessagePage(ChannelId channelId, CancellationToken cancellationToken = default);

    [Post(Routes.Endpoints.CreateChannel)]
    Task<ChannelId?> CreateChannel(IEnumerable<UserId> participants, CancellationToken cancellationToken = default);

    [Post(Routes.Endpoints.SendFriendRequest)]
    Task<FriendRequestResultDto> SendFriendRequest(FriendRequestDto friendRequestDto);

    [Post(Routes.Endpoints.RespondFriendRequest)]
    Task<IApiResponse> RespondFriendRequest();
}
