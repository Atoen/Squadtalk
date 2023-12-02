namespace Squadtalk.Data;

public class Channel
{
    public string Id { get; set; } = string.Empty;
    
    public List<ApplicationUser> Participants { get; set; } = [];
}