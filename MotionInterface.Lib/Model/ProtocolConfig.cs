namespace MotionInterface.Lib.Model;

public static class ProtocolConfig
{
    public const ushort ProtocolFrameMaxSize = 100;
    public const ushort ProtocolRecursiveBufferSize = 32768;
    // public const ushort ProtocolRecursiveBufferSize = 400;

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
    NullCmd = 0xFFFF,
    
    // client to server commands
    SendVelPidCmd = 0x0001,
    SendPosPidCmd = 0x0002,
    SendStateIdCmd = 0x0003,
    
    DataLogSendAvailableDataCmd = 0x2001,
    DataLogEchoLogDataCmd = 0x2002,
    DataLogEchoLogStartCmd = 0x2003,
    //! only this cmd is sent in high-speed uart port
    DataLogRunningCmd = 0x2004,

    // server to client commands
    SetVelPidCmd = 0x1001,
    SetPosPidCmd = 0x1002,
    StartSystemCmd = 0x1003,
    StopSystemCmd = 0x1004,
    EmergencySystemStopCmd = 0x1005,
    ResetSystemCmd = 0x1006,
    
    DataLogCheckAvailableDataCmd = 0x3001,
    DataLogSetLogDataCmd = 0x3002,
    DataLogStartLogCmd = 0x3003,
    DataLogStopLogCmd = 0x3004,
}