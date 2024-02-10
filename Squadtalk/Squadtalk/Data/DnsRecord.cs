namespace Squadtalk.Data;

public class DnsRecord
{
    public required string Name { get; set; }
    public required string Id { get; set; }

    public void Deconstruct(out string name, out string id)
    {
        name = Name;
        id = Id;
    }
}