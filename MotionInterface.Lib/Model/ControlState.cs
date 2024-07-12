namespace MotionInterface.Lib.Model;

public class ControlState
{
    public  MotionState MotionState { get; set; } = new ();
    
}

public enum MotionState
{
    Init,
    PowerOn,
    Idle,
    PosMode,
    VelMode,
    TorqueMode,
    Exit,
    TestTorqueBs,
}