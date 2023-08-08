using System.Net;
using Piston.Networking;
using Serilog;

namespace Piston;

public class PistonServer
{
    private readonly PistonServerOptions _options;
    private readonly SocketListener _socketServer;
    private volatile bool _running = true;

    internal bool Running => _running;

    public PistonServer(PistonServerOptions options)
    {
        _options = options;
        _socketServer = new SocketListener(this);
    }

    public void Start()
    {
        var endpoint = new IPEndPoint(_options.Host, _options.Port);
        _socketServer.StartListening(endpoint).GetAwaiter().GetResult();
    }

    public void Stop()
    {
        if (!_running)
            return;

        _running = false;
        Log.Information("The server is shutting down...");
    }
}