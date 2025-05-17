using Piston.Networking.Buffers;
using SharpNBT;

namespace Piston.Networking.Packets.Clientbound;

public readonly struct RegistryDataPacket(CompoundTag registryData) : IClientboundPacket
{
    public int PacketId => 0x05;
    
    public void Write(PacketBuffer buffer)
    {
        var writer = BufferedTagWriter.Create(CompressionType.GZip, FormatOptions.BigEndian);
        writer.WriteCompound(registryData);
        buffer.Write(writer.ToArray());
    }
}