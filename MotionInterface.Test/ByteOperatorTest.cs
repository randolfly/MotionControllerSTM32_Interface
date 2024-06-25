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
    public void ByteArrayAndFloatArrayConversionTest()
    {
        // the latter test can be carried with random float array
        var floatArray = new float[]
        {
            1.0f,1e-8f,0.1f,0.001f, 1.0f,0.0f,22222.29f,1990129012121.4f,211212e10f
        };
        var byteArray = floatArray.FloatArrayToByteArray();
        var newFloatArray = byteArray.ByteArrayToFloatArray();
        for (var i = 0; i < floatArray.Length; i++)
        {
            Assert.Equal(floatArray[i], newFloatArray[i]);
        }
    }

    [Fact]
    public void FloatStringAndByteArrayConversionTest() {
        // the latter test can be carried with random float array
        var floatArray = new float[]
        {
            1.0f,2.0f,3.0f,1e-8f,0.1f,0.001f, 1.0f,0.0f,22222.29f,1990129012121.4f,211212e10f
        };
        var validateFloatString = string.Join(',',
            floatArray.Select(f => f.ToString()).ToArray());
        _testOutputHelper.WriteLine(validateFloatString);
        var testFloatString = floatArray.FloatArrayToFloatString();
        Assert.Equal(validateFloatString, testFloatString);

        // convert back
        var testFloatArray = testFloatString.FloatStringToFloatArray();
        for(var i=0; i<floatArray.Length; i++)
        {
            Assert.Equal(testFloatArray[i], floatArray[i]);
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
        for (var i = 0; i<nameStringList.Count; i++)
        {
            Assert.Equal(nameStringList[i], testNameStringList[i]);
        }
    }
}