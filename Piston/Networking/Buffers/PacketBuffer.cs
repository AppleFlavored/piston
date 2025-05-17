using System.Buffers;
using System.Buffers.Binary;
using System.Text;
using Serilog;

namespace Piston.Networking.Buffers;

public sealed class PacketBuffer : IDisposable
{
    private const int MaxPacketLength = 2_097_151;
    private static readonly ArrayPool<byte> BufferPool = ArrayPool<byte>.Create(MaxPacketLength, 24);

    public int ReadPosition { get; private set; }
    public int WritePosition { get; private set; }

    private byte[] _buffer = BufferPool.Rent(1024);

    public void SetByte(int offset, byte value)
    {
        _buffer[offset] = value;
    }
    
    public int Skip(int offset)
    {
        var oldPosition = WritePosition;
        WritePosition += offset;
        return oldPosition;
    }

    public async Task<int> ReadFromStreamAsync(Stream stream, int remaining)
    {
        var bytesRead = await stream.ReadAsync(_buffer.AsMemory(WritePosition, remaining));
        WritePosition += bytesRead;
        return bytesRead;
    }
    
    public async Task WriteToStreamAsync(Stream stream)
    {
        await stream.WriteAsync(_buffer.AsMemory(0, WritePosition));
    }

    public void Read(Span<byte> destination)
    {
        var source = _buffer.AsSpan(ReadPosition, destination.Length);
        source.CopyTo(destination);
        ReadPosition += destination.Length;
    }
    
    public void Write(ReadOnlySpan<byte> span)
    {
        var target = _buffer.AsSpan(WritePosition);
        span.CopyTo(target);
        WritePosition += span.Length;
    }
    
    public byte ReadByte()
    {
        return _buffer[ReadPosition++];
    }
    
    public ushort ReadUnsignedShort()
    {
        var value = BinaryPrimitives.ReadUInt16BigEndian(_buffer.AsSpan(ReadPosition));
        ReadPosition += sizeof(ushort);
        return value;
    }
    
    public int ReadInt()
    {
        var value = BinaryPrimitives.ReadInt32BigEndian(_buffer.AsSpan(ReadPosition));
        ReadPosition += sizeof(int);
        return value;
    }
    
    public long ReadLong()
    {
        var value = BinaryPrimitives.ReadInt64BigEndian(_buffer.AsSpan(ReadPosition));
        ReadPosition += sizeof(long);
        return value;
    }
    
    public float ReadFloat()
    {
        var value = BinaryPrimitives.ReadSingleBigEndian(_buffer.AsSpan(ReadPosition));
        ReadPosition += sizeof(float);
        return value;
    }
    
    public double ReadDouble()
    {
        var value = BinaryPrimitives.ReadDoubleBigEndian(_buffer.AsSpan(ReadPosition));
        ReadPosition += sizeof(double);
        return value;
    }

    public int ReadVarInt()
    {
        var value = 0;
        var position = 0;
        while (true)
        {
            var b = ReadByte();
            value |= (b & 0x7F) << (position++ * 7);
            // Return if we have read the entire value.
            if ((b & 0x80) == 0) return value;
            // Throw if the value is too large.
            if (position >= 7) throw new Exception("VarInt is too large");
        }
    }
    
    public string ReadString()
    {
        var length = ReadVarInt();
        var bytes = _buffer.AsSpan(ReadPosition, length);
        ReadPosition += length;
        return Encoding.UTF8.GetString(bytes);
    }

    public void WriteByte(byte value)
    {
        ResizeBufferIfNecessary(sizeof(byte));
        _buffer[WritePosition++] = value;
    }
    
    public void WriteInt(int value)
    {
        ResizeBufferIfNecessary(sizeof(int));
        var target = _buffer.AsSpan(WritePosition, sizeof(int));
        BinaryPrimitives.WriteInt32BigEndian(target, value);
        WritePosition += sizeof(int);
    }
    
    public void WriteLong(long value)
    {
        ResizeBufferIfNecessary(sizeof(long));
        var target = _buffer.AsSpan(WritePosition, sizeof(long));
        BinaryPrimitives.WriteInt64BigEndian(target, value);
        WritePosition += sizeof(long);
    }
    
    public void WriteFloat(float value)
    {
        ResizeBufferIfNecessary(sizeof(float));
        var target = _buffer.AsSpan(WritePosition, sizeof(float));
        BinaryPrimitives.WriteSingleBigEndian(target, value);
        WritePosition += sizeof(float);
    }
    
    public void WriteDouble(double value)
    {
        ResizeBufferIfNecessary(sizeof(double));
        var target = _buffer.AsSpan(WritePosition, sizeof(double));
        BinaryPrimitives.WriteDoubleBigEndian(target, value);
        WritePosition += sizeof(double);
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
            WriteByte((byte)((value & 0x80) | 0x7F));
            value >>= 7;
        }
    }

    public void WriteString(string value)
    {
        Span<byte> bytes = stackalloc byte[value.Length];
        if (!Encoding.UTF8.TryGetBytes(value, bytes, out _))
            throw new Exception("Failed to encode string");
        WriteVarInt(bytes.Length);
        Write(bytes);
    }

    private void ResizeBufferIfNecessary(int sizeToWrite)
    {
        // NOTE: If we reach the point where a resize is necessary, we should consider changing the additional
        //       buffer size to a more reasonable value.
        
        if (WritePosition + sizeToWrite >= _buffer.Length)
        {
            var newBuffer = BufferPool.Rent(_buffer.Length + 1024);
            Log.Information("PacketBuffer: Buffer was resized from {0} to {1}", _buffer.Length, newBuffer.Length);
            
            // Copy the contents to the new buffer, and return the old buffer to the pool.
            _buffer.CopyTo(newBuffer.AsMemory());
            BufferPool.Return(_buffer, true);
            
            _buffer = newBuffer;
        }
    }
    
    public void Dispose()
    {
        BufferPool.Return(_buffer, true);
    }
}