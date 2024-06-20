using MotionInterface.Lib.Model;
using MotionInterface.Lib.Util;
using static MotionInterface.Lib.Model.ProtocolCommand;
using static MotionInterface.Lib.Model.ProtocolConfig;

namespace MotionInterface.Lib.Service;

public class ProtocolParserService
{
    public readonly ProtocolFrame ProtocolFrame = new ();

    public readonly byte[] RecursiveBuffer = new byte[ProtocolRecursiveBufferSize];
    public ushort ReadOffset;
    public ushort WriteOffset;
    public ushort FrameLen;
    public bool FoundFrameHead;

    public ProtocolParserService()
    {
        
    }

    /// <summary>
    /// write the data into the ring buffer
    /// </summary>
    /// <param name="data">target data to receive</param>
    /// <param name="receiveLen">receive data length</param>
    public void ProtocolDataReceive(ref byte[] data, ushort receiveLen)
    {
        if (WriteOffset + receiveLen > ProtocolRecursiveBufferSize)
        {
            var dataLenPart = (ProtocolRecursiveBufferSize - WriteOffset);
            Buffer.BlockCopy(data, 0, RecursiveBuffer, WriteOffset, dataLenPart);
            Buffer.BlockCopy(data, dataLenPart, RecursiveBuffer, 0, receiveLen - dataLenPart);
        }else {
            Buffer.BlockCopy(data, 0, RecursiveBuffer, WriteOffset, receiveLen);
        }
        WriteOffset = (ushort)((WriteOffset + receiveLen) % ProtocolRecursiveBufferSize);
    }

    public int ProtocolDataHandler()
    {
        var frameData = new byte[ProtocolRecursiveBufferSize];
        ushort frameLen = 0;
        var cmdType = NullCmd;

        cmdType = ProtocolFrameParse(ref frameData, ref frameLen);
        switch (cmdType)
        {
            case NullCmd: 
                Console.WriteLine("NullCmd\n");
                break;
            case SendVelPidCmd:
                Console.WriteLine("SendVelPidCmd\n");
                break;
            case SendPosPidCmd:
                break;
            case SendStateIdCmd:
                break;
            case SetVelPidCmd:
                break;
            case SetPosPidCmd:
                break;
            case StartSystemCmd:
                break;
            case StopSystemCmd:
                break;
            case ResetSystemCmd:
                break;
            default:
                Console.WriteLine("NotMatchCommand\n");
                return -1;
        }
        return (int)cmdType;
    }


    private ProtocolCommand ProtocolFrameParse(ref byte[] data, ref ushort dataLen)
    {
        var cmd = NullCmd;
        ProtocolFrame.Command = cmd;
        ushort unParsedDataLen = 0;
        byte checksum = 0;

        unParsedDataLen = GetUnparsedFrameLen(
            FrameLen,
            ProtocolRecursiveBufferSize,
            ReadOffset,
            WriteOffset);
        if (unParsedDataLen < ProtocolFrameHeaderSize)
        {
            return cmd;
        }
        
        // no frame header founded, continue found
        if (FoundFrameHead is false)
        {
            var headerOffset = FindFrameHeader(RecursiveBuffer, ReadOffset, unParsedDataLen);

            if (0<= headerOffset)
            {
                // frame header founded
                FoundFrameHead = true;
                ReadOffset = (ushort)headerOffset;
                if (GetUnparsedFrameLen(FrameLen, 
                        ProtocolRecursiveBufferSize,
                        ReadOffset,
                        WriteOffset) < ProtocolFrameHeaderSize)
                {
                    return cmd;
                }
            }
            else
            {
                // no valid frame header in unparsed data, remove all data in this parse
                ReadOffset = (ushort)((ReadOffset + unParsedDataLen - 3) % ProtocolRecursiveBufferSize);
                return cmd;
            }
        }
        
        // check frame length valid
        if (0 == FrameLen)
        {
            FrameLen = GetFrameLen(RecursiveBuffer, ReadOffset);
            if (unParsedDataLen < FrameLen)
            {
                return cmd;
            }
        }
        
        // frame header valid, do checksum
        if (FrameLen + ReadOffset - ProtocolFrameChecksumSize > ProtocolRecursiveBufferSize) {
            checksum = ByteOperator.CalculateChecksum(
                RecursiveBuffer,
                ReadOffset,
                ProtocolRecursiveBufferSize - ReadOffset,
                checksum);
            checksum = ByteOperator.CalculateChecksum(
                RecursiveBuffer,
                0,
                FrameLen - ProtocolFrameChecksumSize + ReadOffset - ProtocolRecursiveBufferSize,
                checksum);
        } else {
            checksum = ByteOperator.CalculateChecksum(
                RecursiveBuffer,
                ReadOffset,
                FrameLen - ProtocolFrameChecksumSize,
                checksum);
        }

        byte tmpChecksum = 0;
        tmpChecksum = GetFrameCheckSum(RecursiveBuffer, ReadOffset, FrameLen);

        if (tmpChecksum == checksum)
        {
            // all data valid
            if ((ReadOffset + FrameLen) > ProtocolRecursiveBufferSize) {
                var dataLenPart = ProtocolRecursiveBufferSize - ReadOffset;
                Buffer.BlockCopy(RecursiveBuffer, 
                    ReadOffset,
                    data,
                    0,
                    dataLenPart);
                Buffer.BlockCopy(RecursiveBuffer, 
                    0,
                    data,
                    dataLenPart,
                    FrameLen - dataLenPart);
            } else {
                Buffer.BlockCopy(RecursiveBuffer, 
                    ReadOffset,
                    data,
                    0,
                    FrameLen);
            }

            dataLen = FrameLen;
            cmd = (ProtocolCommand)GetFrameCmd(data, 0);
            ProtocolFrame.DeserializeFrameData(ref data, 0);
            ReadOffset = (ushort)((ReadOffset + FrameLen) % ProtocolRecursiveBufferSize);
        }
        else
        {
            // check error, update read_offset
            ReadOffset = (ushort)((ReadOffset + 1) % ProtocolRecursiveBufferSize);
        }
        
        FrameLen        = 0;
        FoundFrameHead = false;
        
        return cmd;
    }
    
    
    private static byte GetFrameData(byte[] buf, ushort readOffset, int index)
    {
        return (byte)(buf[(readOffset + index) % ProtocolRecursiveBufferSize] & 0xFF);
    }

    private static uint GetFrameHeader(byte[] buf, ushort readOffset)
    {
        return (uint)(GetFrameData(buf, readOffset, ProtocolFrameHeaderOffset + 0) << 0 |
                      GetFrameData(buf, readOffset, ProtocolFrameHeaderOffset + 1) << 8 |
                      GetFrameData(buf, readOffset, ProtocolFrameHeaderOffset + 2) << 16 |
                      GetFrameData(buf, readOffset, ProtocolFrameHeaderOffset + 3) << 24); 
    }

    private static ushort GetFrameCmd(byte[] buf, ushort readOffset)
    {
        return (ushort)(GetFrameData(buf, readOffset, ProtocolFrameCommandOffset) << 0 |
                        GetFrameData(buf, readOffset, ProtocolFrameCommandOffset + 1) << 8);
    }
    
    private static ushort GetFrameLen(byte[] buf, ushort readOffset)
    {
        return (ushort)(GetFrameData(buf, readOffset, ProtocolFrameLengthOffset) << 0 |
                        GetFrameData(buf, readOffset, ProtocolFrameLengthOffset + 1) << 8);
    }
    
    private static byte GetFrameMotorId(byte[] buf, ushort readOffset)
    {
        return GetFrameData(buf, readOffset, ProtocolFrameMotorIdOffset);
    }

    private static byte GetFrameCheckSum(byte[] buf, ushort readOffset, ushort frameLen)
    {
        return GetFrameData(buf, readOffset, frameLen - 1);
    }
    
  
    /// <summary>
    /// Get the unparsed frame length
    /// </summary>
    /// <param name="frameLen">ideal frame length</param>
    /// <param name="bufferLen">ring buffer length</param>
    /// <param name="readOffset">start parse index in ring buffer</param>
    /// <param name="writeOffset">end parse index in ring buffer</param>
    /// <returns>unparsed frame length, 0: error</returns>
    private static ushort GetUnparsedFrameLen(ushort frameLen, ushort bufferLen, ushort readOffset, ushort writeOffset)
    {
        ushort unparsedFrameLen = 0;
        if (writeOffset >= readOffset)
        {
            unparsedFrameLen = (ushort)(writeOffset - readOffset);
        }
        else
        {
            unparsedFrameLen = (ushort)(bufferLen - readOffset + writeOffset);
        }

        return frameLen > unparsedFrameLen ? (ushort)0 : unparsedFrameLen;
    }
    
    
    /// <summary>
    /// find frame header in the buffer
    /// </summary>
    /// <param name="buffer">unparsed data buffer</param>
    /// <param name="start">start index of the buffer</param>
    /// <param name="targetFrameLen">search length</param>
    /// <returns>-1: not found, other: found index</returns>
    private static int FindFrameHeader(byte[] buffer, ushort start, ushort targetFrameLen)
    {
        for (var i = 0; i < (targetFrameLen-3); i++)
        {
            if (((buffer[(start + i + 0) % buffer.Length] << 0) |
                 (buffer[(start + i + 1) % buffer.Length] << 8) |
                 (buffer[(start + i + 2) % buffer.Length] << 16) |
                 (buffer[(start + i + 3) % buffer.Length] << 24)) == (int)ProtocolFrameHeader.NormalHeader) 
            {
                return ((start + i) % buffer.Length);
            }
        }

        return -1;
    }
}
    