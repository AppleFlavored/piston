using System.Net;
using System.Net.Sockets;

namespace Piston.Networking;

public class SocketServer
{
    private readonly IPAddress _address;
    private readonly ushort _port;
    private readonly ManualResetEventSlim _acceptResetEvent = new(false);
    private bool _shouldRun = true;

    public SocketServer(IPAddress address, ushort port)
    {
        _address = address;
        _port = port;
    }
    
    public void Listen()
    {
        var listener = new Socket(_address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        var endpoint = new IPEndPoint(_address, _port);
        
        listener.Bind(endpoint);
        listener.Listen();

        while (_shouldRun)
        {
            _acceptResetEvent.Reset();
            listener.BeginAccept(AcceptCallback, listener);
            _acceptResetEvent.Wait();
        }
        
        listener.Close();
    }

    public void Close()
    {
        _shouldRun = false;
    }

    private void AcceptCallback(IAsyncResult result)
    {
        _acceptResetEvent.Set();
        
        var listener = (Socket)result.AsyncState!;
        var socket = listener.EndAccept(result);

        var session = new GameSession(socket);
        try
        {
            session.HandleConnection();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
            
        // var connection = new Connection(socket);
        // _ = Task.Run(connection.HandleConnection);
    }
}