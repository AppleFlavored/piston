using System.Net;
using System.Net.Sockets;
using System.Resources;
using Piston.Networking;
using Serilog;
using SharpNBT;
using SharpNBT.SNBT;

namespace Piston;

public class PistonServer
{
    private readonly PistonServerOptions _options;
    private readonly Socket _serverSocket = new(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
    private readonly Thread _schedulerThread;
    private readonly List<PlayerConnection> _connectionMap = [];
    
    public static bool IsAlive { get; private set; } = true;
    
    public PistonServer(PistonServerOptions options)
    {
        _options = options;
        _schedulerThread = new Thread(ProcessTicks) { Name = "Tick Scheduler" };
    }
    
    public void Start()
    {
        // Start the tick scheduler.
        _schedulerThread.Start();
        
        var endpoint = new IPEndPoint(_options.Host, _options.Port);
        _serverSocket.Bind(endpoint);
        _serverSocket.Listen();
        
        // Start the accept loop.
        Log.Information("Starting server on {0}", endpoint);
        _serverSocket.BeginAccept(HandleClientConnection, _serverSocket);
    }

    public void Shutdown()
    {
        Log.Information("Shutting down...");
        IsAlive = false;
        _schedulerThread.Join();

        foreach (var connection in _connectionMap) { connection.Disconnect(); }
        _serverSocket.Close();
    }

    private void HandleClientConnection(IAsyncResult result)
    {
        if (result.AsyncState is not Socket serverSocket)
            return;
        
        var incoming = serverSocket.EndAccept(result);
        var playerConnection = new PlayerConnection(incoming);
        Task.Run(playerConnection.HandleConnection);
        _connectionMap.Add(playerConnection);
        
        serverSocket.BeginAccept(HandleClientConnection, serverSocket);
    }
    
    private void ProcessTicks()
    {
        // TODO: Implement a tick scheduler.
        // This method should probably go into a separate class.
        while (IsAlive) { }
    }
}