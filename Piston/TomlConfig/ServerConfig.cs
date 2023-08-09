using Tomlyn.Model;

namespace Piston;

/// <summary>
/// Prime example of how configuration files are created.
/// </summary>
public sealed class ServerConfig
{
    // Default server config
    public static TomlTable Default = new TomlTable
    {
        ["title"] = "Piston",
        ["slots"] = 40,
        ["description"] = "I am MOTD",
        ["port"] = 25565,
    };
}