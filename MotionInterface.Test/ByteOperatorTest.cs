using MotionInterface.Lib.Util;
using Xunit.Abstractions;

namespace MotionInterface.Test;

public class ByteOperatorTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ByteOperatorTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void ByteArrayAndDoubleArrayConversionTest()
    {
        // the latter test can be carried with random double array
        var doubleArray = new double[]
        {
            1.0f,1e-8f,0.1f,0.001f, 1.0f,0.0f,22222.29f,1990129012121.4f,211212e10f
        };
        var byteArray = doubleArray.DoubleArrayToByteArray();
        var newDoubleArray = byteArray.ByteArrayToDoubleArray();
        for (var i = 0; i < doubleArray.Length; i++)
        {
            Assert.Equal(doubleArray[i], newDoubleArray[i]);
        }
    }

    [Fact]
    public void DoubleStringAndByteArrayConversionTest()
    {
        // the latter test can be carried with random double array
        var doubleArray = new double[]
        {
            1.0f,2.0f,3.0f,1e-8f,0.1f,0.001f, 1.0f,0.0f,22222.29f,1990129012121.4f,211212e10f
        };
        var validateDoubleString = string.Join(',',
            doubleArray.Select(f => f.ToString()).ToArray());
        _testOutputHelper.WriteLine(validateDoubleString);
        var testDoubleString = doubleArray.DoubleArrayToDoubleString();
        Assert.Equal(validateDoubleString, testDoubleString);

        // convert back
        var testDoubleArray = testDoubleString.DoubleStringToDoubleArray();
        for (var i = 0; i < doubleArray.Length; i++)
        {
            Assert.Equal(testDoubleArray[i], doubleArray[i]);
        }
    }

    [Fact]
    public void NameStringAndByteArrayConversionTest()
    {
        var nameStringList = new List<string>
        {
            "kp", "ki", "kd", "state_id", "test_name_hello"
        };
        var byteArray = nameStringList.NameStringListToByteArray();
        var testNameStringList = byteArray.ByteArrayToNameStringList();
        Assert.Equal(nameStringList.Count, testNameStringList.Count);
        for (var i = 0; i < nameStringList.Count; i++)
        {
            Assert.Equal(nameStringList[i], testNameStringList[i]);
        }
    }
}