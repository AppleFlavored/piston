using System.Text.Json.Serialization;

namespace Piston.Generators.Entities;

public record PacketList
{
    public int ProtocolVersion { get; set; }
    [JsonPropertyName("handshake")] public List<PacketDefinition>? HandshakePackets { get; set; }
    [JsonPropertyName("status")] public List<PacketDefinition>? StatusPackets { get; set; }
    [JsonPropertyName("login")] public List<PacketDefinition>? LoginPackets { get; set; }
    [JsonPropertyName("play")] public List<PacketDefinition>? PlayPackets { get; set; }
}