namespace MotionInterface.Lib.Protocol;
using Util;

public class ProtocolFrame
{
    public ProtocolFrameHeader Header = ProtocolFrameHeader.NormalHeader;
    public MotorId MotorId = MotorId.MotorId1;
    public ushort Length = ProtocolConfig.ProtocolFrameHeaderSize + ProtocolConfig.ProtocolFrameChecksumSize;
    public ProtocolCommand Command = ProtocolCommand.NullCmd;
    public byte[] ParamData = Array.Empty<byte>();
    public byte Checksum;

    /// <summary>
    /// serialize frame struct to uint8 data array, the checksum will be calculated automatically
    /// </summary>
    /// <param name="dataDest">target data array, only store 1 frame data, no need to protect the data array</param>
    public void SerializeFrameData(ref byte[] dataDest)
    {
        ((uint)Header).ExtractTypeToBytes(ref dataDest,
            ProtocolConfig.ProtocolFrameHeaderOffset);
        ((byte)MotorId).ExtractTypeToBytes(ref dataDest,
            ProtocolConfig.ProtocolFrameMotorIdOffset
        );
        ((ushort)Length).ExtractTypeToBytes(ref dataDest,
            ProtocolConfig.ProtocolFrameLengthOffset
        );
        ((ushort)Command).ExtractTypeToBytes(ref dataDest,
            ProtocolConfig.ProtocolFrameCommandOffset
        );

        Buffer.BlockCopy(ParamData,
            0,
             dataDest,
             ProtocolConfig.ProtocolFrameHeaderSize,
            ParamData.Length);

        Checksum = ByteOperator.CalculateChecksum(dataDest, 
            0,
            Length-1);
        dataDest[Length - 1] = Checksum;
    }

    /// <summary>
    /// eserialize frame struct from specified data array, the checksum will not be calculated
    /// </summary>
    /// <param name="data">data array destination</param>
    /// <param name="offset">data array start offset</param> 
    public void DeserializeFrameData(ref byte[] data, int offset=0)
    {
        var targetFrameData = data
            .Skip(offset).ToArray();
        Header = (ProtocolFrameHeader)BitConverter.ToUInt32(targetFrameData, ProtocolConfig.ProtocolFrameHeaderOffset);
        MotorId = (MotorId)targetFrameData[ProtocolConfig.ProtocolFrameMotorIdOffset];
        Length = BitConverter.ToUInt16(targetFrameData, ProtocolConfig.ProtocolFrameLengthOffset);
        Command = (ProtocolCommand)BitConverter.ToUInt16(targetFrameData, ProtocolConfig.ProtocolFrameCommandOffset);

        ParamData = new byte[Length - ProtocolConfig.ProtocolFrameHeaderSize - ProtocolConfig.ProtocolFrameChecksumSize];
        Buffer.BlockCopy(targetFrameData,
            ProtocolConfig.ProtocolFrameHeaderSize,
            ParamData,
            0,
            ParamData.Length);

        Checksum = targetFrameData[Length - 1];
    }

}

