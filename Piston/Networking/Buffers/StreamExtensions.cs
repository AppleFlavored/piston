namespace Piston.Networking.Buffers;

public static class StreamExtensions
{
    public static async Task<int> ReadVarInt(this Stream stream)
    {
        var value = 0;
        var position = 0;
        while (true)
        {
            var b = await stream.ReadByteAsync();
            value |= (b & 0x7F) << (position++ * 7);
            // Return if we have read the entire value.
            if ((b & 0x80) == 0) return value;
            // Throw if the value is too large.
            if (position >= 7) throw new Exception("VarInt is too large");
        }
    }

    private static async Task<int> ReadByteAsync(this Stream stream)
    {
        var buffer = new byte[1];
        _ = await stream.ReadAsync(buffer.AsMemory(0, 1));
        return buffer[0];
    }
}