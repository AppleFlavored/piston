using System.Net;
using Piston;
using Serilog;

const string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u4}] {Message:lj}{NewLine}{Exception}";
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: outputTemplate)
    .CreateLogger();

var options = new PistonServerOptions
{
    Host = IPAddress.Any,
    Port = 25565
};
var server = new PistonServer(options);
Console.CancelKeyPress += delegate { server.Shutdown(); };
server.Start();