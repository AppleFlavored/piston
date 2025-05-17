using Piston.Networking.Buffers;

namespace Piston.Networking.Packets;

public interface IClientboundPacket
{
    int PacketId { get; }
    
    void Write(PacketBuffer buffer);
}