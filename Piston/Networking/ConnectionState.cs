namespace Piston.Networking;

public enum ConnectionState : byte
{
    Handshaking,
    Status,
    Login,
    Configuration,
    Play
}