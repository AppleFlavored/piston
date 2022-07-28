using System.Buffers.Binary;
using System.Text;

namespace Piston.Networking;

public class MinecraftStream : Stream
{
    private readonly Stream _baseStream;

    public override bool CanRead => _baseStream.CanRead;
    public override bool CanSeek => _baseStream.CanSeek;
    public override bool CanWrite => _baseStream.CanWrite;
    public override long Length => _baseStream.Length;
    public override long Position
    {
        get => _baseStream.Position;
        set => _baseStream.Position = value;
    }

    public MinecraftStream(Stream baseStream)
    {
        _baseStream = baseStream;
    }

    public int ReadVarInt()
    {
        var size = 0;
        var value = 0;
        while (true) {
            var b = (byte)ReadByte();
            value |= (b & 0x7F) << (size * 7);
            size++;

            if ((b & 0x80) == 0) break;
            if (size > 5) throw new Exception($"VarInt is too large!");
        }
        return value;
    }

    public void WriteVarInt(int value)
    {
        while (true)
        {
            if ((value & ~0x7F) == 0)
            {
                WriteByte((byte)value);
                return;
            }
            WriteByte((byte)((value & 0x7F) | 0x80));
            value >>= 7;
        }
    }

    public long ReadVarLong()
    {
        var value = 0L;
        var size = 0;
        while (true)
        {
            var b = (byte)ReadByte();
            value |= (long)(b * 0x7F) << (size * 7);
            
            if ((b & 0x80) == 0)
                break;

            size++;
            if (size > 10) throw new Exception("VarLong is too large!");
        }
        return value;
    }
    
    public void WriteVarLong(long value)
    {
        while (true)
        {
            if ((value & ~0x7F) == 0)
            {
                WriteByte((byte)value);
                return;
            }
            WriteByte((byte)((value & 0x7F) | 0x80));
            value >>= 7;
        }
    }

    public string ReadString()
    {
        var length = ReadVarInt();
        var buffer = new byte[length];
        _ = Read(buffer, 0, length);
        return Encoding.UTF8.GetString(buffer);
    }

    public void WriteString(string value)
    {
        WriteVarInt(value.Length);
        var buffer = Encoding.UTF8.GetBytes(value);
        Write(buffer);
    }

    public short ReadShort()
    {
        Span<byte> buffer = stackalloc byte[2];
        _ = Read(buffer);
        return BinaryPrimitives.ReadInt16BigEndian(buffer);
    }

    public void WriteShort(short value)
    {
        Span<byte> buffer = stackalloc byte[2];
        BinaryPrimitives.WriteInt16BigEndian(buffer, value);
        Write(buffer);
    }

    public ushort ReadUnsignedShort()
    {
        Span<byte> buffer = stackalloc byte[2];
        _ = Read(buffer);
        return BinaryPrimitives.ReadUInt16BigEndian(buffer);
    }

    public void WriteUnsignedShort(ushort value)
    {
        Span<byte> buffer = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
        Write(buffer);
    }

    public int ReadInt()
    {
        Span<byte> buffer = stackalloc byte[4];
        _ = Read(buffer);
        return BinaryPrimitives.ReadInt32BigEndian(buffer);
    }
    
    public void WriteInt(int value)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(buffer, value);
        Write(buffer);
    }

    public long ReadLong()
    {
        Span<byte> buffer = stackalloc byte[8];
        _ = Read(buffer);
        return BinaryPrimitives.ReadInt64BigEndian(buffer);
    }

    public void WriteLong(long value)
    {
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(buffer, value);
        Write(buffer);
    }

    public float ReadFloat()
    {
        Span<byte> buffer = stackalloc byte[4];
        _ = Read(buffer);
        return BinaryPrimitives.ReadSingleBigEndian(buffer);
    }

    public void WriteFloat(float value)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteSingleBigEndian(buffer, value);
        Write(buffer);
    }

    public double ReadDouble()
    {
        Span<byte> buffer = stackalloc byte[8];
        _ = Read(buffer);
        return BinaryPrimitives.ReadDoubleBigEndian(buffer);
    }

    public void WriteDouble(double value)
    {
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteDoubleBigEndian(buffer, value);
        Write(buffer);
    }

    public byte[] ReadByteArray()
    {
        var length = ReadVarInt();
        Span<byte> buffer = stackalloc byte[length];
        _ = Read(buffer);
        return buffer.ToArray();
    }
    
    public override void Flush()
        => _baseStream.Flush();

    public override int Read(byte[] buffer, int offset, int count)
        => _baseStream.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin)
        => _baseStream.Seek(offset, origin);

    public override void SetLength(long value)
        => _baseStream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count)
        => _baseStream.Write(buffer, offset, count);
}