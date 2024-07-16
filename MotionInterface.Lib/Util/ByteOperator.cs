using System.Text;
using ProtocolConfig = MotionInterface.Lib.Model.ProtocolConfig;

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

    public static byte CalculateChecksum(byte[] data, int startIndex, int length, byte init = 0)
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

    public static double[] ByteArrayToDoubleArray(this byte[] data)
    {
        // 4bit - double
        var doubleArray = new double[data.Length / ProtocolConfig.ProtocolDatalogTypeSize];
        for (var i = 0; i < data.Length; i += ProtocolConfig.ProtocolDatalogTypeSize)
        {
            doubleArray[i / ProtocolConfig.ProtocolDatalogTypeSize] = BitConverter.ToDouble(data, i);
        }
        return doubleArray;
    }

    public static byte[] DoubleArrayToByteArray(this double[] data)
    {
        var byteArray = new byte[data.Length * ProtocolConfig.ProtocolDatalogTypeSize];
        for (var i = 0; i < data.Length; i++)
        {
            BitConverter.GetBytes(data[i]).CopyTo(byteArray, i * ProtocolConfig.ProtocolDatalogTypeSize);
        }
        return byteArray;
    }

    /// <summary>
    /// used for ui presentation, convert a byte[] to string as: "1.0,2.0,3.0"
    /// </summary>
    /// <param name="data"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static string ByteArrayToDoubleString(this byte[] data, char separator = ',')
    {
        var doubleArray = data.ByteArrayToDoubleArray();
        return doubleArray.DoubleArrayToDoubleString(separator);
    }

    public static byte[] DoubleStringToByteArray(this string rawCommand, char separator = ',')
    {
        var doubleArray = rawCommand.DoubleStringToDoubleArray(separator);
        return doubleArray.DoubleArrayToByteArray();
    }

    public static string DoubleArrayToDoubleString(this double[] data, char separator = ',')
    {
        return string.Join(separator, data);
    }

    public static double[] DoubleStringToDoubleArray(this string rawCommand, char separator = ',')
    {
        var stringArray = rawCommand.Trim().Split(separator);
        var doubleArray = stringArray.Where(s => s.Length > 0)
            .Select(double.Parse).ToArray();
        return doubleArray;
    }

    /// <summary>
    /// [UTF8] convert name string list to byte array, such as ["kp", "ki", "kd"] => "kp, ki, kd" => [0x11,0x22,...]
    /// </summary>
    /// <param name="nameString">string that contains param names</param>
    /// <param name="separator">string separator</param>
    /// <returns>byte array</returns>
    public static byte[] NameStringListToByteArray(this List<string> nameString, char separator = ',')
    {
        var midString = string.Join(separator, nameString);
        return midString.NameStringToByteArray(separator);
    }

    public static byte[] NameStringToByteArray(this string nameString, char separator = ',')
    {
        return Encoding.UTF8.GetBytes(nameString);
    }

    /// <summary>
    /// [UTF8] convert byte array to name string list, such as [0x11,0x22,...] => "kp, ki, kd" => ["kp", "ki", "kd"]
    /// </summary>
    /// <param name="byteArray">byte array that encodes param names</param>
    /// <param name="separator">string separator</param>
    /// <returns>name string</returns>
    public static List<string> ByteArrayToNameStringList(this byte[] byteArray, char separator = ',')
    {
        var midString = Encoding.UTF8.GetString(byteArray); ;
        return midString.Split(separator).ToList();
    }

    public static string ByteArrayToNameString(this byte[] byteArray, char separator = ',')
    {
        return Encoding.UTF8.GetString(byteArray);
    }

    /// <summary>
    /// used to convert string_double param data to string, for SET cmd
    /// </summary>
    /// <param name="data">src param data array</param>
    /// <returns>param name: param value</returns>
    public static string ByteArrayToNameStringAndDouble(this byte[] data)
    {
        var stringValue = data.Take(data.Length - ProtocolConfig.ProtocolDatalogTypeSize).ToArray().ByteArrayToNameString();
        var doubleValue = BitConverter.ToDouble(data, data.Length - ProtocolConfig.ProtocolDatalogTypeSize);
        return CombineNameStringAndDouble(stringValue, doubleValue);
    }

    public static string CombineNameStringAndDouble(string nameString, double doubleValue)
    {
        return $"{nameString}:{doubleValue}";
    }

    /// <summary>
    /// convert string_double param data to byte array, for SET cmd
    /// </summary>
    /// <param name="nameString"></param>
    /// <returns></returns>
    public static byte[] NameStringAndDoubleToByteArray(this string nameString)
    {
        var result = nameString.Split(':');
        var stringValue = result[0].NameStringToByteArray();
        var doubleValue = result[1].DoubleStringToByteArray();
        var targetByteArray = stringValue.Concat(doubleValue).ToArray();
        return targetByteArray;
    }
}
