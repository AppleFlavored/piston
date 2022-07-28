using System.Net;
using Piston.Networking;

namespace Piston;

public class PistonServer
{
    private readonly ConnectionManager _connectionManager;
    
    public PistonServer(IPAddress address, int port = 25565)
    {
        _connectionManager = new ConnectionManager(address, port);
    }
    
    public void Start()
    {
        _connectionManager.Start();
    }
}