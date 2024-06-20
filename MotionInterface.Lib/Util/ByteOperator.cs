
using System.Text;

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
    
    public static string ToHexString(this byte[] data)
    {
        var sb = new StringBuilder();
        foreach (var b in data)
        {
            sb.Append(b.ToString("X2"));
        }
        return sb.ToString();
    }

    public static float[] ToFloatArray(this byte[] data)
    {
        // 4bit - float
        var floatArray = new float[data.Length / 4];
        for (var i = 0; i < data.Length; i += 4)
        {
            floatArray[i / 4] = BitConverter.ToSingle(data, i);
        }
        return floatArray;
    }
    
    public static byte[] ToByteArray(this float[] data)
    {
        var byteArray = new byte[data.Length * 4];
        for (var i = 0; i < data.Length; i++)
        {
            BitConverter.GetBytes(data[i]).CopyTo(byteArray, i * 4);
        }
        return byteArray;
    }
}
