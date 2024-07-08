namespace MotionInterface.Lib.Model;

public static class ProtocolConfig
{
    public const ushort ProtocolFrameMaxSize = 500;
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
    
    #region CONTROL

    

    #endregion

    #region GET

    GetSymbolDataCmd = 0x1001, 
    GetEchoSymbolDataCmd = 0x1101,

    #endregion

    #region SET

    SetSymbolDataCmd = 0x2001, 
    SetEchoSymbolDataCmd = 0x2101,

    #endregion
    

    #region DATALOG
    
    DataLogGetAvailableDataCmd = 0x3001,
    DataLogEchoGetAvailableDataCmd = 0x3101,
    
    DataLogSetLogDataCmd = 0x3002,
    DataLogEchoSetLogDataCmd = 0x3102,
    
    DataLogStartLogCmd = 0x3003,
    DataLogStopLogCmd = 0x3004,
    
    //! only this cmd is sent in high-speed uart port
    DataLogRunningCmd = 0x3103,
    #endregion

    #region OTHER

    NullCmd = 0xFFFF,

    #endregion
}