namespace MotionInterface.Lib.Protocol;

public static class ProtocolConfig
{
    public const ushort ProtocolRecursiveBufferSize = 32768;
    public const ushort ProtocolFrameChecksumSize = 1;
    public const ushort ProtocolFrameHeaderSize = 9;

    public const ushort ProtocolFrameHeaderOffset = 0;
    public const ushort ProtocolFrameMotorIdOffset = 4;
    public const ushort ProtocolFrameLengthOffset = 5;
    public const ushort ProtocolFrameCommandOffset = 7;
}

public enum ProtocolFrameHeader : uint
{
    NormalHeader = 0x0d000721u,
    TestInvalidHeader = 0x23332333
}

public enum MotorId : byte
{
    MotorId1 = 0x01,
    MotorId2 = 0x02
}

public enum ProtocolCommand : ushort
{
    // client to server commands
    SendVelPidCmd = 0x0001,
    SendPosPidCmd = 0x0002,
    SendStateIdCmd = 0x0003,

    // server to client commands
    SetVelPidCmd = 0x0004,
    SetPosPidCmd = 0x0005,
    StartSystemCmd = 0x0006,
    StopSystemCmd = 0x0007,
    ResetSystemCmd = 0x0008,
    NullCmd = 0xFFFF
}