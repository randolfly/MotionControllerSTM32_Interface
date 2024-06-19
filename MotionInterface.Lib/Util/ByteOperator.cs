
namespace MotionInterface.Lib.Util;
using System.Buffers.Binary;
public static class ByteOperator
{
    public static void ExtractTypeToBytes(this uint data, ref byte[] dataDest, uint offset = 0)
    {
        dataDest[0 + offset] = (byte)(data & 0xFF);
        dataDest[1 + offset] = (byte)((data >> 8) & 0xFF);
        dataDest[2 + offset] = (byte)((data >> 16) & 0xFF);
        dataDest[3 + offset] = (byte)((data >> 24) & 0xFF);
    }

    public static void ExtractTypeToBytes(this ushort data, ref byte[] dataDest, uint offset = 0)
    {
        dataDest[0 + offset] = (byte)(data & 0xFF);
        dataDest[1 + offset] = (byte)((data >> 8) & 0xFF);
    }

    public static void ExtractTypeToBytes(this byte data, ref byte[] dataDest, uint offset = 0)
    {
        dataDest[0 + offset] = (byte)(data & 0xFF);
    }

    public static byte CalculateChecksum(byte[] data, int startIndex, int length,  byte init = 0)
    {
        var sum = init;
        for (var i = 0; i < length; i++)
        {
            sum += data[i + startIndex];
        }

        return sum;
    }

    public static uint ReverseEndianness(this uint data)
    {
        return BinaryPrimitives.ReverseEndianness(data);
    }
    
    public static ushort ReverseEndianness(this ushort data)
    {
        return BinaryPrimitives.ReverseEndianness(data);
    }
    
    public static bool SequenceCompare(byte[]? x, byte[]? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return x.Length == y.Length && x.SequenceEqual(y);
    }
}
