using System.Net;
using System.Net.Sockets;

namespace Piston.Networking;

public class ConnectionManager
{
    private readonly IPAddress _address;
    private readonly int _port;
    private readonly ManualResetEventSlim _lock = new(false);

    public ConnectionManager(IPAddress address, int port)
    {
        _address = address;
        _port = port;
    }

    public void Start()
    {
        var listener = new Socket(_address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        listener.NoDelay = true;

        var endpoint = new IPEndPoint(_address, _port);
        listener.Bind(endpoint);
        listener.Listen();

        while (true)
        {
            _lock.Reset();
            listener.BeginAccept(AcceptCallback, listener);
            _lock.Wait();
        }
    }

    private void AcceptCallback(IAsyncResult result)
    {
        _lock.Set();

        var listener = (Socket)result.AsyncState!;
        var socket = listener.EndAccept(result);

        var connection = new Connection(socket);
        _ = Task.Run(connection.HandleConnection);
    }
}