namespace Piston.Generators.Entities;

public record PacketDefinition
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? BoundTo { get; set; }
    public Dictionary<string, string>? Fields { get; set; }
}