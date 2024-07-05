namespace MotionInterface.Lib.Model;

public class ControlState
{
    public  MotorState MotorState { get; set; } = new ();}

public class MotorState
{
    public bool IsElectricallyPowerOn { get; set; } = false;
}