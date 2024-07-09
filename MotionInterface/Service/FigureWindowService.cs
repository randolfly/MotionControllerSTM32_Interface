using MotionInterface.Shared;

namespace MotionInterface.Service;

public class FigureWindowService
{
    //todo: add service to manage figure view windows as persistent service
    public Dictionary<string, FigureViewWindow> FigureViewWindowDictionary { get; set; } = new();

}