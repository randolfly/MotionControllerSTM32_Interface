namespace MotionInterface.Util;

public static class StringHelper
{
    public static List<int> FindSubStringListIndex(this List<string> srcStringList, List<string> subStringList)
    {
        var indexList = subStringList
            .Select(subString => srcStringList.IndexOf(subString)).ToList();
        return indexList;
    }
}