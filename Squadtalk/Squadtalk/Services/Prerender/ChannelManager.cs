using Shared.Data.TypedIds;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal class ChannelManager : LazyModelCreator, IChannelManager
{
    private static readonly GroupChatModel GlobalChatModel = GroupChatModel.CreateGlobalChat();
    private static readonly Dictionary<ChannelId, ChannelModel> EmptyChannels = [];

    private readonly IContactManager _contactManager;
    private readonly IUserAuthenticationService _authenticationService;
    private Dictionary<ChannelId, ChannelModel>? _channels;

    public ChannelManager(
        PrerenderPersistantState prerenderPersistantState,
        IContactManager contactManager,
        IUserAuthenticationService authenticationService) : base(prerenderPersistantState)
    {
        _contactManager = contactManager;
        _authenticationService = authenticationService;
    }

    private Dictionary<ChannelId, ChannelModel> LazyChannels => TryCreateModels(ref _channels, EmptyChannels);

    public IReadOnlyCollection<ChannelModel> Channels => LazyChannels.Values;

    public GroupChatModel GlobalChat => GlobalChatModel;
    public ChannelModel? CurrentChannel { get; private set; }

    event Action? IChannelManager.ChannelsListChanged { add { } remove { } }
    event Action? IChannelManager.ChannelChanged { add { } remove { } }
    event Func<Task>? IChannelManager.ChannelChangedAsync { add { } remove { } }

    protected override void CreateModels(PrerenderPersistantState prerenderPersistantState)
    {
        if (prerenderPersistantState.Channels is not { Count: > 0 } channels)
        {
            _channels = EmptyChannels;
            return;
        }

        var currentUserId = _authenticationService.UserId;

        var channelModels = channels
            .Select(x => ChannelModel.Create(x, currentUserId, _contactManager.UserModelProvider))
            .ToDictionary(x => x.Id, x => x);

        _channels = channelModels;
    }

    public Task OpenChannelAsync(ChannelModel channelModel, bool navigate = true)
    {
        CurrentChannel = channelModel;
        return Task.CompletedTask;
    }

    public ChannelModel? GetChannel(ChannelId channelId) => LazyChannels.GetValueOrDefault(channelId);

    public ChannelModel GetRequiredChannel(ChannelId channelId) => LazyChannels[channelId];

    public Task OpenOrCreateTemporaryDirectMessageChannelAsync(UserModel model) => Task.CompletedTask;

    public Task UpgradeToPersistentChannelAsync(ChannelModel channelModel) => Task.CompletedTask;

    public Task<ChannelId?> CreateNewChannelAsync(params IEnumerable<UserModel> others) => Task.FromResult<ChannelId?>(null);

    public Task AddFriendsToGroupAsync(ChannelId channelId, params IEnumerable<UserModel> friends) => Task.CompletedTask;

    public Task ClearChannelSelectionAsync() => Task.CompletedTask;

    public Task<bool> ChangeGroupChatNameAsync(GroupChatModel groupChat, string? newName) => Task.FromResult(false);
}
