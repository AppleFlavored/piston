namespace Piston.Networking.Handlers;

public interface IPacketHandler
{
    void Read(int id, MinecraftStream stream, GameSession session);
}