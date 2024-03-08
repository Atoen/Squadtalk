using Shared.Data;
using Squadtalk.Data;

namespace Squadtalk.Services;

public class VoiceCallManager
{
    private readonly List<VoiceCall> _activeVoiceCalls = [];
    private readonly List<VoiceCallOffer> _callOffers = [];

    public VoiceCall? GetCall(SignalrConnectionId connectionId)
    {
        var call = _activeVoiceCalls.FirstOrDefault(x =>
            x.Users.FirstOrDefault(y => y.ConnectionId == connectionId) is not null);

        return call;
    }

    public VoiceCall? GetCall(CallId id)
    {
        return _activeVoiceCalls.FirstOrDefault(x => x.Id == id);
    }

    public VoiceCallOffer? GetVoiceCallOffer(CallOfferId id)
    {
        return _callOffers.FirstOrDefault(x => x.Id == id);
    }

    public void AddCallOffer(VoiceCallOffer callOffer)
    {
        _callOffers.Add(callOffer);
    }

    public void RemoveCallOffer(CallOfferId id)
    {
        _callOffers.RemoveAll(x => x.Id == id);
    }

    public void RemoveCallOffersFromUser(ApplicationUser user)
    {
        _callOffers.RemoveAll(x => x.Caller.User.Id == user.Id);
    }

    public void AddCall(VoiceCall call)
    {
        _activeVoiceCalls.Add(call);
    }
    
    public void RemoveCall(VoiceCall call)
    {
        _activeVoiceCalls.Add(call);
    }
}