using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MotionInterface.Lib.Model;
using System.IO;
using System.Text.Json;

namespace MotionInterface.Lib.Service;


public class AppConfigService
{
    public AppConfigService() {
        UpdateConfiguration();
    }

    public AppConfig AppConfig { get; private set; } = new();

    public static void SaveConfiguration(AppConfig appConfig) {
        var jsonString = JsonSerializer.Serialize(appConfig, new JsonSerializerOptions { WriteIndented = true });
        if (!Directory.Exists(AppConfig.ConfigurationFolder))
            Directory.CreateDirectory(AppConfig.ConfigurationFolder);
        File.WriteAllText(AppConfig.ConfigurationFileFullName, jsonString);
    }

    public void UpdateConfiguration() {
        LoadConfiguration(AppConfig.ConfigurationFileFullName);
    }

    private void LoadConfiguration(string configPath) {
        if (!File.Exists(configPath)) return;
        var configString = File.ReadAllText(configPath);
        AppConfig = JsonSerializer.Deserialize<AppConfig>(configString)!;
    }
}
