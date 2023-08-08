using Piston;
using Serilog;

const string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u4}] {Message:lj}{NewLine}{Exception}";
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(
        outputTemplate: outputTemplate)
#if !DEBUG
    .WriteTo.File("latest.log",
        outputTemplate: outputTemplate,
        rollingInterval: RollingInterval.Day,
        rollOnFileSizeLimit: true)
#endif
    .CreateLogger();

// TODO: Load server options from a configuration file.
var serverOptions = new PistonServerOptions { Port = 25565 };
var server = new PistonServer(serverOptions);

// Handle Ctrl+C key events by shutting down the server gracefully.
Console.CancelKeyPress += delegate { server.Stop(); };

server.Start();