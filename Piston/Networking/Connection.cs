using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Piston.Networking;

public class Connection
{
    private readonly Socket _socket;
    private readonly MinecraftStream _stream;
    private State _state = State.Handshaking;

    public Connection(Socket socket)
    {
        _socket = socket;
        _stream = new MinecraftStream(new NetworkStream(_socket));
    }

    public Task HandleConnection()
    {
        while (_socket.Connected)
        {
            var length = _stream.ReadVarInt();
            var id = _stream.ReadVarInt();
            Console.WriteLine($"Length: {length}\nID: {id}\n======");
            
            switch (_state)
            {
                case State.Handshaking:
                    if (id != 0)
                    {
                        _socket.Disconnect(false);
                        break;
                    }

                    _ = _stream.ReadVarInt(); // protocol version
                    _ = _stream.ReadString(); // server address
                    _ = _stream.ReadUnsignedShort(); // server port
                    var nextState = _stream.ReadVarInt();
                    _state = (State)nextState;
                    
                    Console.WriteLine($"Switched state to {_state}");
                    break;
                case State.Status:
                    switch (id)
                    {
                        case 0x00:
                            // Just a test object for serialization
                            // var response = new StatusPing
                            // {
                            //     Version = new ServerVersion { Name = "1.19", Protocol = 759 },
                            //     Players = new Players { Max = 10, Online = 0 },
                            //     Description = new TextComponent { Text = "§cstatus ping done :sunglasses:" }
                            // };
                            //
                            // var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                            // {
                            //     PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            //     DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                            // });
                            //
                            // _stream.WriteVarInt(3 + json.Length);
                            // _stream.WriteVarInt(0);
                            // _stream.WriteString(json);
                            
                            // TODO: server ping
                            _socket.Disconnect(false);
                            break;
                        case 0x01:
                            var payload = _stream.ReadLong();
                            _stream.WriteVarInt(1 + sizeof(long));
                            _stream.WriteVarInt(1);
                            _stream.WriteLong(payload);
                            
                            _socket.Disconnect(false);
                            break;
                        default:
                            _socket.Disconnect(false);
                            break;
                    }
                    break;
                case State.Login:
                    switch (id)
                    {
                        case 0x00:
                            var username = _stream.ReadString();
                            var hasSigData = _stream.ReadByte() == 1;

                            if (hasSigData)
                            {
                                _ = _stream.ReadLong();
                                _ = _stream.ReadByteArray();
                                _ = _stream.ReadByteArray();
                            }
                            
                            Console.WriteLine($"User '{username}' tried connecting to the server.");
                            _stream.WriteVarInt(16 + username.Length + 1);
                            _stream.WriteVarInt(2);
                            Span<byte> buffer = Guid.Empty.ToByteArray();
                            _stream.Write(buffer);
                            _stream.WriteVarInt(0);

                            _state = State.Play;
                            Console.WriteLine($"Switched state to {_state}");
                            
                            // TODO: send login (play) packet
                            break;
                        default:
                            Disconnect("Unhandled packet during login.");
                            break;
                    }
                    break;
                case State.Play:
                    break;
                default:
                    Console.WriteLine($"Unhandled state: {_state}");
                    _socket.Disconnect(false);
                    break;
            }
        }
        
        return Task.CompletedTask;
    }

    public void Disconnect(string reason)
    {
        if ((int)_state <= 1)
        {
            _socket.Disconnect(false);
            return;
        }
        
        var chatString = $"{{\"text\": \"{reason}\"}}";
        _stream.WriteVarInt(2 + chatString.Length);
        _stream.WriteVarInt(0);
        _stream.WriteString(chatString);
        _stream.Flush();
        _socket.Disconnect(false);
    }

    // private class StatusPing
    // {
    //     public ServerVersion Version { get; init; }
    //     public Players Players { get; init; }
    //     public TextComponent Description { get; init; }
    // }
    //
    // private class ServerVersion
    // {
    //     public string Name { get; init; }
    //     public int Protocol { get; init; }
    // }
    //
    // private class Players
    // {
    //     public int Max { get; init; }
    //     public int Online { get; init; }
    // }
    //
    // private class TextComponent
    // {
    //     public string Text { get; init; }
    //     public string Color { get; init; }
    // }

    private enum State
    {
        Handshaking,
        Status,
        Login,
        Play
    }
}