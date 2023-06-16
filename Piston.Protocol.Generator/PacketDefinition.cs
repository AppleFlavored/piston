namespace Piston.Protocol.Generator;

public record PacketDefinition
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? State { get; set; }
    public string? BoundTo { get; set; }
    public Dictionary<string, string>? Fields { get; set; }
}