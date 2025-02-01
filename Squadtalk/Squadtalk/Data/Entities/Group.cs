using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Services;

namespace Squadtalk.Data.Entities;

public class Group
{
    public GroupId Id { get; set; } = default!;

    [StringLength(IFormValidator.MaximumGroupNameLength)]
    public string? CustomName { get; set; }

    public ICollection<GroupParticipant> Participants { get; set; } = default!;

    public ChatUser GroupCreator { get; set; } = default!;

    public ChatType ChatType { get; set; }

    public Message? LastMessage { get; set; }

    // [Owned]
    // public class Message
    // {
    //     [StringLength(IFormValidator.MaximumTextMessageLength)]
    //     public required string Content { get; set; }
    //
    //     [StringLength(IFormValidator.MaximumUsernameLength)]
    //     public required string AuthorName { get; set; }
    //
    //     public required GroupId GroupId { get; set; }
    //
    //     public required UserId AuthorId { get; set; }
    //
    //     public Embed? Embed { get; set; }
    //
    //     public DateTimeOffset Timestamp { get; set; }
    // }
}
