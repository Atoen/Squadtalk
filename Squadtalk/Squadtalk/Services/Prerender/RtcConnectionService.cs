using Shared.Models;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

public class RtcConnectionService : IRtcConnectionService
{
    public bool ConnectedToCall => false;

    public Task StartCallAsync(ChatModel chat) => Task.CompletedTask;

    public Task AcceptCallAsync(ChatModel chat) => Task.CompletedTask;

    public Task DeclineCallAsync(ChatModel chat) => Task.CompletedTask;

    public Task LeaveCallAsync(ChatModel chat) => Task.CompletedTask;

    public Task<bool> GroupHasActiveCallAsync(ChatModel chat) => Task.FromResult(false);
}
