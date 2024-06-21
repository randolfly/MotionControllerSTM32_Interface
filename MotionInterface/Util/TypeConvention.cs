using BlazorComponent;

namespace MotionInterface.Util;

public static class TypeConvention
{
    public static StringNumber ToStringNumber(this string data)
    {
        return new StringNumber(data);
    }
    
    public static List<StringNumber> ToStringNumber(this List<string> data)
    {
        return data.Select(ToStringNumber).ToList();
    }
}