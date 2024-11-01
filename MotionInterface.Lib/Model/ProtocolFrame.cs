﻿using System.Text;
using MotionInterface.Lib.Util;

namespace MotionInterface.Lib.Model;

public class ProtocolFrame
{
    public ProtocolFrameHeader Header = ProtocolFrameHeader.NormalHeader;
    public MotorId MotorId = MotorId.MotorId1;
    public ushort Length = ProtocolConfig.ProtocolFrameHeaderSize + ProtocolConfig.ProtocolFrameChecksumSize;
    public ProtocolCommand Command = ProtocolCommand.NullCmd;
    public byte[] ParamData = Array.Empty<byte>();
    public byte Checksum;

    public string DateTime = System.DateTime.Now.ToString("HH:mm:ss:ms");

    /// <summary>
    /// serialize frame struct to uint8 data array, the length and checksum will be calculated automatically
    /// </summary>
    /// <param name="dataDest">target data array, only store 1 frame data, no need to protect the data array</param>
    public void SerializeFrameData(ref byte[] dataDest)
    {
        Length = (ushort)(ProtocolConfig.ProtocolFrameHeaderSize + ParamData.Length + ProtocolConfig.ProtocolFrameChecksumSize);
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
            Length - 1);
        dataDest[Length - 1] = Checksum;
    }

    /// <summary>
    /// serialize frame struct from specified data array, the checksum will not be calculated
    /// </summary>
    /// <param name="data">data array destination</param>
    /// <param name="offset">data array start offset</param> 
    public void DeserializeFrameData(ref byte[] data, int offset = 0)
    {
        var targetFrameData = data
            .Skip(offset).ToArray();
        Header = (ProtocolFrameHeader)BitConverter.ToUInt32(targetFrameData, ProtocolConfig.ProtocolFrameHeaderOffset);
        MotorId = (MotorId)targetFrameData[ProtocolConfig.ProtocolFrameMotorIdOffset];
        Command = (ProtocolCommand)BitConverter.ToUInt16(targetFrameData, ProtocolConfig.ProtocolFrameCommandOffset);

        Length = BitConverter.ToUInt16(targetFrameData, ProtocolConfig.ProtocolFrameLengthOffset);
        ushort totalRequiredLength = (ushort)(ProtocolConfig.ProtocolFrameHeaderSize + ProtocolConfig.ProtocolFrameChecksumSize);

        if (Length < totalRequiredLength)
        {
            throw new InvalidOperationException("Length must be larger than the sum of ProtocolFrameHeaderSize and ProtocolFrameChecksumSize.");
        }
        else
        {
            // Proceed with your subtraction and other logic, since it's safe to do so.
            ushort ParamDataSize = (ushort)(Length - totalRequiredLength);
            // Continue with the logic that uses ParamDataSize
        }

        ParamData = new byte[Length - ProtocolConfig.ProtocolFrameHeaderSize - ProtocolConfig.ProtocolFrameChecksumSize];
        Buffer.BlockCopy(targetFrameData,
            ProtocolConfig.ProtocolFrameHeaderSize,
            ParamData,
            0,
            ParamData.Length);

        Checksum = targetFrameData[Length - 1];
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"{MotorId}::{Command}_{ParamData.Length}");
        return sb.ToString();
    }

    public ProtocolFrame DeepClone()
    {
        var newProtocolFrame = new ProtocolFrame
        {
            Header = Header,
            MotorId = MotorId,
            Length = Length,
            Command = Command,
            ParamData = new byte[ParamData.Length],
        };
        for (var id = 0; id < ParamData.Length; id++)
        {
            newProtocolFrame.ParamData[id] = ParamData[id];
        }
        newProtocolFrame.Checksum = Checksum;
        return newProtocolFrame;
    }

    public void UpdateFrameDateTime()
    {
        DateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms");
    }




    /// <summary>
    /// FRAME DATA => UI 
    /// </summary>
    /// <returns></returns>
    public string SerializeParamData()
    {
        return Command switch
        {
            ProtocolCommand.GetSymbolDataCmd => ParamData.ByteArrayToNameString(),
            ProtocolCommand.GetEchoSymbolDataCmd => ParamData.ByteArrayToDoubleString(),
            ProtocolCommand.SetSymbolDataCmd => ParamData.ByteArrayToNameStringAndDouble(),
            ProtocolCommand.SetEchoSymbolDataCmd => ParamData.ByteArrayToNameStringAndDouble(),
            ProtocolCommand.DataLogEchoGetAvailableDataCmd => ParamData.ByteArrayToNameString(),
            ProtocolCommand.DataLogEchoSetLogDataCmd => ParamData.ByteArrayToNameString(),
            ProtocolCommand.DataLogSetLogDataCmd => ParamData.ByteArrayToNameString(),
            _ => ParamData.ByteArrayToDoubleString()
        };
    }

    /// <summary>
    /// UI=>FRAME DATA
    /// convert param data string to byte array for ui send frame
    /// </summary>
    /// <param name="paramDataString">ui data string</param>
    public void DeserializeParamData(string paramDataString)
    {
        ParamData = Command switch
        {
            ProtocolCommand.GetSymbolDataCmd => paramDataString.NameStringToByteArray(),
            ProtocolCommand.SetSymbolDataCmd => paramDataString.NameStringAndDoubleToByteArray(),
            ProtocolCommand.DataLogSetLogDataCmd => paramDataString.NameStringToByteArray(),
            _ => paramDataString.DoubleStringToByteArray()
        };
    }
}

