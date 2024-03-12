using Shared.Data;

namespace Shared.Models;

public class VoiceCallModel
{
    public List<UserModel> Connected { get; set; } = default!;

    public CallId Id { get; set; } = default!;
}