using System.Net;
using System.Net.Sockets;
using Serilog;

namespace Piston.Networking;

public class SocketListener : IDisposable
{
    private readonly PistonServer _server;
    private readonly Socket _listener = new(SocketType.Stream, ProtocolType.Tcp);

    public SocketListener(PistonServer server)
    {
        _server = server;
    }
    
    public async Task StartListening(IPEndPoint endpoint)
    {
        _listener.Bind(endpoint);
        _listener.Listen();

        while (_server.Running)
        {
            var incomingSocket = await _listener.AcceptAsync();
            HandleIncomingClient(incomingSocket);
        }
        
        _listener.Close();
    }

    public void Dispose()
    {
        _listener.Close();
    }

    private void HandleIncomingClient(Socket socket)
    {
        Log.Information($"Connection from {socket.RemoteEndPoint}");
    }
}