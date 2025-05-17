using Piston.Networking.Buffers;

namespace Piston.Networking.Packets;

public interface IServerboundPacket<out T>
{
    static abstract T Read(PacketBuffer buffer);
}