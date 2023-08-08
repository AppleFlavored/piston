using System.Net;

namespace Piston;

public record PistonServerOptions
{
    public IPAddress Host { get; init; } = IPAddress.Any;
    public ushort Port { get; init; }
}