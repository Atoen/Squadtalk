using Shared.Data.TypedIds;
using Shared.Models;
using Shared.Reactive;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal class RtcConnectionService(IContactManager contactManager) : IRtcConnectionService
{
    event Action<ChatModel>? IRtcConnectionService.IncomingCall { add { } remove { } }

    public bool ConnectedToCall => false;

    public ChatModel? CallChat => null;

    public CallParticipantModel LocalParticipant { get; } = new(contactManager.LocalUserModel);

    public IObservableCollection<CallParticipantModel> CallParticipants => ObservableCollection<CallParticipantModel>.Empty;

    public Task StartCallAsync(ChatModel chat) => Task.CompletedTask;

    public Task AcceptCallAsync(ChatModel chat) => Task.CompletedTask;

    public Task DeclineCallAsync(ChatModel chat) => Task.CompletedTask;

    public Task LeaveCallAsync() => Task.CompletedTask;

    public CallParticipantModel? GetParticipantById(UserId userId) => null;

    public Task<bool> GroupHasActiveCallAsync(ChatModel chat) => Task.FromResult(false);

    public Task ChangeUserVolumeAsync(CallParticipantModel participant, Volume volume, AudioSource audioSource = AudioSource.Microphone) => Task.CompletedTask;

    public Task<Volume> GetSavedUserVolumeAsync(CallParticipantModel participant) => Task.FromResult(Volume.Full);
}
