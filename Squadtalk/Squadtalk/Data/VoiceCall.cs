namespace Squadtalk.Data;

public class VoiceCall
{
    public required ApplicationUser Initiator { get; set; }

    public required List<ApplicationUser> Invited { get; set; }

    public required HashSet<Participant> ConnectedUsers { get; set; }
    
    public required string Id { get; set; }

    public record Participant(ApplicationUser User, string SignalrConnectionId);

    public VoiceCall AddParticipant(ApplicationUser user, string connectionId)
    {
        var participant = new Participant(user, connectionId);
        ConnectedUsers.Add(participant);

        return this;
    }
}