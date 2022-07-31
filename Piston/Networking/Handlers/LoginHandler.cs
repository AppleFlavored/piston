namespace Piston.Networking.Handlers;

public class LoginHandler : IPacketHandler
{
    public void Read(int id, MinecraftStream stream, GameSession session)
    {
        switch (id)
        {
            case 0x00:
                var username = stream.ReadString();
                var hasSigData = stream.ReadByte() == 1;

                if (hasSigData)
                {
                    _ = stream.ReadLong();
                    _ = stream.ReadByteArray();
                    _ = stream.ReadByteArray();
                }

                // Login Success
                // Length (still need a better way to do this)
                stream.WriteVarInt(3 + 16 + username.Length);
                // ID
                stream.WriteVarInt(2);
                // UUID
                Span<byte> buffer = Guid.Empty.ToByteArray();
                stream.Write(buffer);
                // Username
                stream.WriteString(username);
                // Number of Properties
                stream.WriteVarInt(0);

                session.ChangeState(GameSession.ClientState.Play);
                break;
            default:
                session.Disconnect();
                break;
        }
    }
}