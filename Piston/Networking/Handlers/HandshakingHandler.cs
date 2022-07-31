namespace Piston.Networking.Handlers;

public class HandshakingHandler : IPacketHandler
{
    public void Read(int id, MinecraftStream stream, GameSession session)
    {
        switch (id)
        {
            case 0x00:
                _ = stream.ReadVarInt(); // protocol version
                _ = stream.ReadString(); // server address
                _ = stream.ReadUnsignedShort(); // server port
                session.ChangeState((GameSession.ClientState)stream.ReadVarInt());
                break;
            default:
                session.Disconnect();
                break;
        }
    }
}