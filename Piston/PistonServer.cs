using System.Net;
using Piston.Networking;

namespace Piston;

public class PistonServer
{
    private readonly IPAddress _address;
    private readonly ushort _port;
    
    public PistonServer(IPAddress address, ushort port)
    {
        _address = address;
        _port = port;
    }

    public void Start()
    {
        var server = new SocketServer(_address, _port);
        server.Listen();
    }
}