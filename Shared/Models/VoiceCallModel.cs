namespace Shared.Models;

public class VoiceCallModel
{
    public List<UserModel> Connected { get; set; } = default!;

    public string Id { get; set; } = default!;
}