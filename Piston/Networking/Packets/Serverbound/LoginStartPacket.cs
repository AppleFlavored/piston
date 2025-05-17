using Piston.Networking.Buffers;

namespace Piston.Networking.Packets.Serverbound;

public struct LoginStartPacket : IServerboundPacket<LoginStartPacket>
{
    public const int PacketId = 0x00;

    public string Username { get; }
    public Guid UniqueId { get; }

    private LoginStartPacket(PacketBuffer buffer)
    {
        Username = buffer.ReadString();

        Span<byte> uuidBytes = stackalloc byte[16];
        buffer.Read(uuidBytes);
        UniqueId = new Guid(uuidBytes);
    }
    
    public static LoginStartPacket Read(PacketBuffer buffer)
    {
        return new LoginStartPacket(buffer);
    }
}