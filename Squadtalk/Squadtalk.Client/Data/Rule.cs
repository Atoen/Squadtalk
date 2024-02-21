using System.Text.Json.Serialization;

namespace Squadtalk.Client.Data;

public record Rule
{
    public string Attacker { get; }
    public string Attacked { get; }
    
    public Rule(Color attacker, Color attacked)
    {
        if (attacker == attacked)
        {
            throw new InvalidOperationException("Attacker cannot be the same as attacked.");
        }

        Attacker = attacker.ToString();
        Attacked = attacked.ToString();
    }
    
    [JsonConstructor]
    public Rule(string attacker, string attacked)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(attacker);
        ArgumentException.ThrowIfNullOrWhiteSpace(attacked);
        
        if (attacker == attacked)
        {
            throw new InvalidOperationException("Attacker cannot be the same as attacked.");
        }

        if (!AvailableColors.Any(x => x.Equals(attacker, StringComparison.InvariantCultureIgnoreCase)))
        {
            throw new InvalidOperationException($"Attacker has invalid color: {attacker}");
        }
        
        if (!AvailableColors.Any(x => x.Equals(attacked, StringComparison.InvariantCultureIgnoreCase)))
        {
            throw new InvalidOperationException($"Attacked has invalid color: {attacked}");
        }

        Attacker = attacker;
        Attacked = attacked;
    }

    public static string[] AvailableColors { get; } = Enum.GetNames(typeof(Color));

}