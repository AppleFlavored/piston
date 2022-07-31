using System.Net.Sockets;
using Piston.Networking.Handlers;

namespace Piston.Networking;

public class GameSession
{
    private readonly Socket _socket;
    private readonly MinecraftStream _stream;
    private ClientState _state = ClientState.Handshaking;
    private IPacketHandler _handler = new HandshakingHandler();

    public GameSession(Socket socket)
    {
        _socket = socket;
        _stream = new MinecraftStream(new NetworkStream(_socket));
    }

    public void HandleConnection()
    {
        while (_socket.Connected)
        {
            var length = _stream.ReadVarInt();
            var id = _stream.ReadVarInt();
            Console.WriteLine($"Length: {length}\nID: {id}\n======");

            _handler.Read(id, _stream, this);
        }
    }

    public void SendPacket()
    {
    }

    public void ChangeState(ClientState state)
    {
        _state = state;
        switch (_state)
        {
            case ClientState.Status:
                _handler = new StatusHandler();
                break;
            case ClientState.Login:
                _handler = new LoginHandler();
                break;
            default:
                throw new Exception($"A handler does not exist for {_state} state.");
        }
    }

    public void Disconnect()
    {
        if ((int)_state <= 1)
            goto disconnect;
        
        disconnect:
        _socket.Disconnect(false);
    }

    public enum ClientState
    {
        Handshaking,
        Status,
        Login,
        Play
    }
}