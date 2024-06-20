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
        var byteArray = floatArray.ToByteArray();
        var newFloatArray = byteArray.ToFloatArray();
        for (var i = 0; i < floatArray.Length; i++)
        {
            Assert.Equal(floatArray[i], newFloatArray[i]);
        }
    }
}