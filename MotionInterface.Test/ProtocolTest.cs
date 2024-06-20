using MotionInterface.Lib.Util;
using Xunit.Abstractions;

namespace MotionInterface.Test;
using Lib.Protocol;

public class ProtocolTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    private ProtocolFrame ProtocolFrame { get; set; } = new ();
    private ProtocolParser ProtocolParser { get; set; } = new();
    private byte[] _protocolFrameDataPool = new byte[ProtocolConfig.ProtocolRecursiveBufferSize];
    private int _frameParseResult = (int)ProtocolCommand.NullCmd;
    
    public ProtocolTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        ProtocolFrame = InitProtocolFrame();
    }
    
    [Fact]
    public void SerializeAndDeserializeFrameDataTest()
    {
        var testFrame = new ProtocolFrame();
        ProtocolFrame = InitProtocolFrame();
        ProtocolFrame.SerializeFrameData(ref _protocolFrameDataPool);
        testFrame.DeserializeFrameData(ref _protocolFrameDataPool);
        
        Assert.Equal(testFrame.Header, ProtocolFrame.Header);
        Assert.Equal(testFrame.MotorId, ProtocolFrame.MotorId);
        Assert.Equal(testFrame.Command, ProtocolFrame.Command);
        Assert.Equal(testFrame.Length, ProtocolFrame.Length);
        Assert.Equal(testFrame.ParamData, ProtocolFrame.ParamData);
        Assert.Equal(testFrame.Checksum, ProtocolFrame.Checksum);
    }

    [Fact]
    public void ProtocolDataReceiveTest()
    {
        _testOutputHelper.WriteLine("start protocol_data_receive_test\n");
        ProtocolFrame = InitProtocolFrame();

        var checkReadOffset = ProtocolParser.WriteOffset;
        var checkWriteOffset = (ushort)(ProtocolParser.WriteOffset + ProtocolFrame.Length);

        WriteFrameBuffer(ProtocolFrame);

        for (var i = checkReadOffset; i < checkWriteOffset; i++)
        {
            var parserData = ProtocolParser.RecursiveBuffer[i % ProtocolConfig.ProtocolRecursiveBufferSize];
            Assert.True(parserData.Equals(_protocolFrameDataPool[i]));
        }
    }

    [Fact]
    public void SendVelPidCommandTest()
    {
        _testOutputHelper.WriteLine("start SendVelPidCommandTest\n");
        var protocolFrame = InitProtocolFrame();
        WriteFrameBuffer(protocolFrame);
        CheckParseResult(protocolFrame);
    }
    
    [Fact]
    public void SendMultipleCommandsTest()
    {
        _testOutputHelper.WriteLine("start SendMultipleCommandsTest\n");
        var protocolFrame1 = InitProtocolFrame();
        WriteFrameBuffer(protocolFrame1);
        var protocolFrame2 = InitProtocolFrame(ProtocolCommand.ResetSystemCmd);
        WriteFrameBuffer(protocolFrame2);
        var protocolFrame3 = InitProtocolFrame(ProtocolCommand.SetPosPidCmd);
        protocolFrame3.Header = ProtocolFrameHeader.TestInvalidHeader;
        WriteFrameBuffer(protocolFrame3);
        var protocolFrame4 = InitProtocolFrame(ProtocolCommand.SendStateIdCmd);
        WriteFrameBuffer(protocolFrame4);
        
        CheckParseResult(protocolFrame1);
        CheckParseResult(protocolFrame2);
        CheckParseResult(protocolFrame4, protocolFrame3.Length);
        _frameParseResult = ProtocolParser.ProtocolDataHandler();
        Assert.True(_frameParseResult == (int)ProtocolCommand.NullCmd);
        WriteFrameBuffer(protocolFrame1);
        WriteFrameBuffer(protocolFrame2);
        CheckParseResult(protocolFrame1);
        CheckParseResult(protocolFrame2);
    }
    
    [Fact]
    public void SendThousandsCommandsTest()
    {
        _testOutputHelper.WriteLine("start SendThousandsCommandsTest\n");
        var protocolFrame = InitProtocolFrame();
        for (var i = 0; i < 3000; i++)
        {
            WriteFrameBuffer(protocolFrame);
        }
        for (var i = 0; i < 3000; i++)
        {
            CheckParseResult(protocolFrame);
        }
        for (var i = 0; i < 3000; i++)
        {
            WriteFrameBuffer(protocolFrame);
        }
        for (var i = 0; i < 3000; i++)
        {
            CheckParseResult(protocolFrame);
        }
        for (var i = 0; i < 3000; i++)
        {
            WriteFrameBuffer(protocolFrame);
        }
        for (var i = 0; i < 3000; i++)
        {
            CheckParseResult(protocolFrame);
        }
    }
    private void CheckParseResult(ProtocolFrame protocolFrame, int preReadOffset = 0)
    {
        // test on ProtocolRecursiveBufferSize = 41;
        // note: do not set 40, the size should be prime to n*frameSize
        // or the readOffset may equal to writeOffset, the corner case is not easy to handle
        preReadOffset += ProtocolParser.ReadOffset;
        _frameParseResult = ProtocolParser.ProtocolDataHandler();
        Assert.True(_frameParseResult == (int)protocolFrame.Command);
        Assert.False(ProtocolParser.FoundFrameHead);
        Assert.True((ProtocolParser.ReadOffset%ProtocolConfig.ProtocolRecursiveBufferSize) == (ProtocolFrame.Length + preReadOffset)%ProtocolConfig.ProtocolRecursiveBufferSize);
        var testFrame = ProtocolParser.ProtocolFrame;
        Assert.True(protocolFrame.Header == testFrame.Header);
        Assert.True(protocolFrame.MotorId == testFrame.MotorId);
        Assert.True(protocolFrame.Command == testFrame.Command);
        Assert.True(protocolFrame.Length == testFrame.Length);
        Assert.True(ByteOperator.SequenceCompare(protocolFrame.ParamData, testFrame.ParamData));
        Assert.True(protocolFrame.Checksum == testFrame.Checksum);
    }
    
    private ProtocolFrame InitProtocolFrame(ProtocolCommand command = ProtocolCommand.SendVelPidCmd)
    {
        var protocolFrame = new ProtocolFrame
        {
            Header = ProtocolFrameHeader.NormalHeader,
            MotorId = MotorId.MotorId1,
            Command = command,
            Length = (ushort)(ProtocolConfig.ProtocolFrameHeaderSize +
                              ProtocolConfig.ProtocolFrameChecksumSize + 
                              ProtocolFrame.ParamData.Length),
            ParamData = []
        };
        return protocolFrame;
    }
    
    private void WriteFrameBuffer(ProtocolFrame protocolFrame)
    {
        protocolFrame.SerializeFrameData(ref _protocolFrameDataPool);
        ProtocolParser.ProtocolDataReceive(ref _protocolFrameDataPool, 
            protocolFrame.Length);
    }
}