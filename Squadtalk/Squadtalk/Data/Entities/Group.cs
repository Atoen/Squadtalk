using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

    [ForeignKey(nameof(LastMessage))]
    public MessageId? LastMessageId { get; set; }
    public Message? LastMessage { get; set; }
}
