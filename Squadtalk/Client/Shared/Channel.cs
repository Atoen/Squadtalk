namespace Squadtalk.Client.Shared;

public record struct Channel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}