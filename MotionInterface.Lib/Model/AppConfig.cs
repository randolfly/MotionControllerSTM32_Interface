using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MotionInterface.Lib.Model;

public class AppConfig {
    public SerialPortConfig CommandSerialPortConfig { get; set; } = new();
    public SerialPortConfig DatalogPortConfig { get; set; } = new SerialPortConfig
    {
        BaudRate = 921600
    };
    public DataLogConfig DataLogConfig { get; set; } = new();


    public static readonly string ConfigurationFolder;
    public static readonly string ConfigurationFileName;

    static AppConfig() {
        ConfigurationFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        ConfigurationFileName = Assembly.GetCallingAssembly().FullName!.Split(',')[0] + ".json";
    }

    [JsonIgnore]
    public static string ConfigurationFileFullName => Path.Combine(ConfigurationFolder, ConfigurationFileName);
}