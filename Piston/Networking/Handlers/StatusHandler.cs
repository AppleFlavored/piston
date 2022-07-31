using System.Text.Json.Nodes;

namespace Piston.Networking.Handlers;

public class StatusHandler : IPacketHandler
{
    public void Read(int id, MinecraftStream stream, GameSession session)
    {
        switch (id)
        {
            // Status Request
            case 0x00:
                var json = new JsonObject
                {
                    ["version"] = new JsonObject 
                    {
                        ["name"] = "1.19",
                        ["protocol"] = 759    
                    },
                    ["players"] = new JsonObject
                    {
                        ["max"] = 10,
                        ["online"] = 0
                    },
                    ["description"] = new JsonObject
                    {
                        ["text"] = "test"
                    }
                };
                var jsonString = json.ToJsonString();
                
                stream.WriteVarInt(2 + jsonString.Length);
                stream.WriteVarInt(0);
                stream.WriteString(jsonString);
                break;
            
            // Ping Request
            case 0x01:
                var payload = stream.ReadLong();
                stream.WriteVarInt(1 + sizeof(long));
                stream.WriteVarInt(1);
                stream.WriteLong(payload);
                            
                session.Disconnect();
                break;
            
            default:
                session.Disconnect();
                break;
        }
    }
}