using Shared.Communication;
using Shared.Enums;

namespace Shared.Models;

public class UserModel : ICommunicationTab
{
    public string Username { get; set; } = default!;
    
    public string Color { get; set; } = default!;
    public string AvatarUrl { get; set; } = default!;

    public string Id { get; set; } = default!;
    public UserStatus Status { get; set; }
    
    public bool Selected { get; set; }
    public string Name => Username;
    
}