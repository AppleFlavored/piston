using Piston.Networking.Buffers;

namespace Piston.Networking.Packets.Clientbound;

public struct LoginSuccessPacket(Guid uniqueId, string username) : IClientboundPacket
{
    public int PacketId => 0x02;

    public void Write(PacketBuffer buffer)
    {
        Span<byte> uuidBytes = stackalloc byte[16];
        _ = uniqueId.TryWriteBytes(uuidBytes);
        buffer.Write(uuidBytes);
        buffer.WriteString(username);
        buffer.WriteVarInt(0);
    }
}