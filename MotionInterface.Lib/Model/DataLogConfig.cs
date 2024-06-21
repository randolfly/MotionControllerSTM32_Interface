using System.Text.Json.Serialization;

namespace MotionInterface.Lib.Model;

public class DataLogConfig
{
    public string DataLogFolderName { get; set; } =
        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    public string DataLogFileName { get; set; } = "helloworld";

    public List<DataExportTypes> DataExportTypes { get; set; } =
        Enum.GetValues<DataExportTypes>().ToList();
    public List<string> RecordSymbolName { get; set; } = new();
    public List<string> GraphSymbolName { get; set; } = new();

    [JsonIgnore]
    public string TempFileFullName => Path.Combine(DataLogFolderName, DataLogFileName);

    [JsonIgnore]
    public string RecordFileFullName
    {
        get
        {
            var datetime = DateTime.Now;
            var fileName = DataLogFileName + "_" + datetime.ToString("yyyyMMddHHmmss");
            return Path.Combine(DataLogFolderName, fileName);
        }
    }
}

public enum DataExportTypes
{
    Csv,
    Matlab
}
