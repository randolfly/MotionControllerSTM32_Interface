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
        InitProtocolFrame();
    }
    
    [Fact]
    public void SerializeAndDeserializeFrameDataTest()
    {
        var testFrame = new ProtocolFrame();
        InitProtocolFrame();
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
        InitProtocolFrame();

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
    public void SendVelPidCommadTest()
    {
        _testOutputHelper.WriteLine("start protocol_data_receive_test\n");
        InitProtocolFrame();
        ProtocolFrame testFrame = new();
        
        WriteFrameBuffer(ProtocolFrame);
        _frameParseResult = ProtocolParser.ProtocolDataHandler();
        Assert.True(_frameParseResult == (int)ProtocolCommand.SendVelPidCmd);
        Assert.False(ProtocolParser.FoundFrameHead);
        Assert.True(ProtocolParser.ReadOffset == ProtocolFrame.Length);

        testFrame = ProtocolParser.ProtocolFrame;
        
        Assert.True(ProtocolFrame.Header == testFrame.Header);
        Assert.True(ProtocolFrame.MotorId == testFrame.MotorId);
        Assert.True(ProtocolFrame.Command == testFrame.Command);
        Assert.True(ProtocolFrame.Length == testFrame.Length);
        Assert.True(ByteOperator.SequenceCompare(ProtocolFrame.ParamData, testFrame.ParamData));
        Assert.True(ProtocolFrame.Checksum == testFrame.Checksum);
    }
    private void InitProtocolFrame()
    {
        ProtocolFrame.Header = ProtocolFrameHeader.NormalHeader;
        ProtocolFrame.MotorId = MotorId.MotorId1;
        ProtocolFrame.Command = ProtocolCommand.SendVelPidCmd;
        ProtocolFrame.Length = (ushort)(ProtocolConfig.ProtocolFrameHeaderSize +
                                        ProtocolConfig.ProtocolFrameChecksumSize + 
                                        ProtocolFrame.ParamData.Length);

        ProtocolFrame.ParamData = [];
    }
    
    private void WriteFrameBuffer(ProtocolFrame protocolFrame)
    {
        protocolFrame.SerializeFrameData(ref _protocolFrameDataPool);
        ProtocolParser.ProtocolDataReceive(ref _protocolFrameDataPool, 
            protocolFrame.Length);
    }
}