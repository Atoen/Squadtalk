using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal class ChatGroupManager : LazyModelCreator, IChatGroupManager
{
    private static readonly GroupChatModel GlobalGroupModel = GroupChatModel.CreateGlobalChat();
    private static readonly Dictionary<GroupId, ChatModel> EmptyChannels = [];

    private readonly IContactManager _contactManager;
    private readonly IUserAuthenticationService _authenticationService;
    private Dictionary<GroupId, ChatModel>? _channels;

    public ChatGroupManager(
        PrerenderPersistantState prerenderPersistantState,
        IContactManager contactManager,
        IUserAuthenticationService authenticationService) : base(prerenderPersistantState)
    {
        _contactManager = contactManager;
        _authenticationService = authenticationService;

        GroupParticipantProvider = GetOrCreateGroupParticipantModel;
    }

    private Dictionary<GroupId, ChatModel> LazyChannels => TryCreateModels(ref _channels, EmptyChannels);

    public IReadOnlyCollection<ChatModel> Channels => LazyChannels.Values;

    public Func<IGroupParticipant, GroupParticipantModel> GroupParticipantProvider { get; }

    public GroupChatModel GlobalGroup => GlobalGroupModel;
    public ChatModel? CurrentChannel { get; private set; }

    event Action? IChatGroupManager.ChannelsListChanged { add { } remove { } }
    event Action? IChatGroupManager.ChannelChanged { add { } remove { } }
    event Func<Task>? IChatGroupManager.ChannelChangedAsync { add { } remove { } }

    protected override void CreateModels(PrerenderPersistantState prerenderPersistantState)
    {
        if (prerenderPersistantState.Channels is not { Count: > 0 } channels)
        {
            _channels = EmptyChannels;
            return;
        }

        var currentUserId = _authenticationService.UserId;

        var channelModels = channels
            .Select(x => ChatModel.Create(x, GroupParticipantProvider))
            .ToDictionary(x => x.Id, x => x);

        _channels = channelModels;
    }

    private GroupParticipantModel GetOrCreateGroupParticipantModel(IGroupParticipant groupParticipant)
    {
        return GroupParticipantModel.Create(groupParticipant, _contactManager.UserModelProvider);
    }

    public Task OpenChannelAsync(ChatModel chatModel, bool navigate = true)
    {
        CurrentChannel = chatModel;
        return Task.CompletedTask;
    }

    public ChatModel? GetChannel(GroupId groupId) => LazyChannels.GetValueOrDefault(groupId);

    public ChatModel GetRequiredChannel(GroupId groupId) => LazyChannels[groupId];

    public Task OpenOrCreateTemporaryDirectMessageChannelAsync(UserModel model) => Task.CompletedTask;

    public Task UpgradeToPersistentChannelAsync(ChatModel chatModel) => Task.CompletedTask;

    public Task<GroupId?> CreateNewChannelAsync(params IEnumerable<UserModel> others) => Task.FromResult<GroupId?>(null);

    public Task CreateAndOpenNewChannelAsync(params IEnumerable<UserModel> others) => Task.CompletedTask;

    public Task AddFriendsToGroupAsync(GroupId groupId, params IEnumerable<UserModel> friends) => Task.CompletedTask;

    public Task ClearChannelSelectionAsync() => Task.CompletedTask;

    public Task<bool> ChangeGroupChatNameAsync(GroupChatModel groupChat, string? newName) => Task.FromResult(false);
}
