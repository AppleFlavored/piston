using System.Net.Sockets;
using System.Resources;
using Piston.Networking.Buffers;
using Piston.Networking.Packets;
using Piston.Networking.Packets.Clientbound;
using Piston.Networking.Packets.Serverbound;
using Serilog;
using SharpNBT;

namespace Piston.Networking;

public class PlayerConnection(Socket socket)
{
    private readonly NetworkStream _stream = new(socket);
    
    public ConnectionState State { get; private set; } = ConnectionState.Handshaking;
    
    public async Task HandleConnection()
    {
        try
        {
            while (true)
            {
                using var buffer = new PacketBuffer();
                if (!await GetNextPacketAsync(buffer)) break;
                
                var packetId = buffer.ReadVarInt();
                Log.Information("Received packet with length {0} and ID {1}", buffer.WritePosition, packetId);
                switch (State)
                {
                    case ConnectionState.Handshaking when packetId == 0:
                        HandleHandshakePacket(buffer);
                        break;
                    case ConnectionState.Login:
                        await HandleLoginPacket(packetId, buffer);
                        break;
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Connection was closed.");
        }
        finally
        {
            Disconnect();
        }
    }

    private async Task<bool> GetNextPacketAsync(PacketBuffer buffer)
    {
        var packetLength = await _stream.ReadVarInt();
        if (packetLength == 0) return false;
        
        var bytesReceived = 0;
        while (bytesReceived < packetLength)
        {
            var numBytes = await buffer.ReadFromStreamAsync(_stream, packetLength - bytesReceived);
            if (numBytes == 0) return false;
            bytesReceived += numBytes;
        }
        
        return true;
    }

    public async Task SendPacketAsync(IClientboundPacket packet)
    {
        using var packetBuffer = new PacketBuffer();
        _ = packetBuffer.Skip(3);
        packetBuffer.WriteVarInt(packet.PacketId);
        packet.Write(packetBuffer);
        
        var packetLength = packetBuffer.WritePosition - 3;
        packetBuffer.SetByte(0, (byte)(packetLength & 0x7F | 0x80));
        packetBuffer.SetByte(1, (byte)((packetLength >> 7) & 0x7F | 0x80));
        packetBuffer.SetByte(2, (byte)(packetLength >> 14));
        
        await packetBuffer.WriteToStreamAsync(_stream);
    }
    
    private void HandleHandshakePacket(PacketBuffer buffer)
    {
        _ = buffer.ReadVarInt();
        _ = buffer.ReadString();
        _ = buffer.ReadUnsignedShort();
        var nextState = buffer.ReadVarInt();
        
        State = (ConnectionState) nextState;
    }
    
    private async Task HandleLoginPacket(int packetId, PacketBuffer buffer)
    {
        switch (packetId)
        {
            case 0x00:
                var packet = LoginStartPacket.Read(buffer);
                await SendPacketAsync(new LoginSuccessPacket(packet.UniqueId, packet.Username));
                break;
            case 0x03:
                State = ConnectionState.Configuration;
                
                var registry = new CompoundTag(null)
                {
                    new CompoundTag("minecraft:trim_pattern")
                    {
                        new StringTag("type", "minecraft:trim_pattern"),
                        new ListTag("value", TagType.Compound)
                    },
                    new CompoundTag("minecraft:dimension_type")
                    {
                        new StringTag("type", "minecraft:dimension_type"),
                        new ListTag("value", TagType.Compound)
                    },
                    new CompoundTag("minecraft:chat_type")
                    {
                        new StringTag("type", "minecraft:chat_type"),
                        new ListTag("value", TagType.Compound)  
                    },
                    new CompoundTag("minecraft:damage_type")
                    {
                        new StringTag("type", "minecraft:damage_type"),
                        new ListTag("value", TagType.Compound)  
                    },
                    new CompoundTag("minecraft:worldgen/biome")
                    {
                        new StringTag("type", "minecraft:worldgen/biome"),
                        new ListTag("value", TagType.Compound)
                        {
                            new CompoundTag(null)
                            {
                                new StringTag("name", "minecraft:plains"),
                                new IntTag("id", 0),
                                new CompoundTag("element")
                                {
                                    new CompoundTag("effects")
                                    {
                                        new IntTag("sky_color", 7907327),
                                        new IntTag("water_fog_color", 329011),
                                        new IntTag("fog_color", 12638463),
                                        new IntTag("water_color", 4159204),
                                        new CompoundTag("mood_sound")
                                        {
                                            new IntTag("tick_delay", 6000),
                                            new FloatTag("offset", 2.0f),
                                            new StringTag("sound", "minecraft:ambient.cave"),
                                            new IntTag("block_search_extent", 8)
                                        }
                                    },
                                    new ByteTag("has_precipitation", true),
                                    new FloatTag("temperature", 0.8f),
                                    new FloatTag("downfall", 0.4f)
                                }
                            }
                        }
                    }
                };
                await SendPacketAsync(new RegistryDataPacket(registry));
                break;
        }
    }

    private async Task HandleConfigurationPacket(int packetId, PacketBuffer buffer)
    {
        switch (packetId)
        {
            case 0x00:
                var packet = ClientInformationPacket.Read(buffer);
                Log.Information("Received client information packet: {0}", packet);
                break;
        }
    }
    
    public void Disconnect()
    {
        socket.Close();
    }
}