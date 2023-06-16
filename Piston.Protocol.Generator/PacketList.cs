namespace Piston.Protocol.Generator;

public record PacketList
{
    public int ProtocolVersion { get; set; }
    public List<PacketDefinition>? Packets { get; set; }
}