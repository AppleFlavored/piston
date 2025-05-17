using Piston.Networking.Buffers;

namespace Piston.Networking.Packets.Serverbound;

public struct ClientInformationPacket : IServerboundPacket<ClientInformationPacket>
{
    public string Locale { get; }
    public byte ViewDistance { get; }
    public int ChatMode { get; }
    public bool ChatColors { get; }
    public byte DisplayedSkinParts { get; }
    public int MainHand { get; }
    public bool EnableTextFiltering { get; }
    public bool AllowServerListing { get; }
    
    private ClientInformationPacket(PacketBuffer buffer)
    {
        Locale = buffer.ReadString();
        ViewDistance = buffer.ReadByte();
        ChatMode = buffer.ReadVarInt();
        ChatColors = buffer.ReadByte() == 1;
        DisplayedSkinParts = buffer.ReadByte();
        MainHand = buffer.ReadVarInt();
        EnableTextFiltering = buffer.ReadByte() == 1;
        AllowServerListing = buffer.ReadByte() == 1;
    }
    
    public static ClientInformationPacket Read(PacketBuffer buffer)
    {
        return new ClientInformationPacket(buffer);
    }
}