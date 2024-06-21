using System.Globalization;
using System.Text;
using MathNet.Numerics.Data.Matlab;
using MathNet.Numerics.LinearAlgebra;
using MotionInterface.Lib.Model;
using MotionInterface.Lib.Util;

namespace MotionInterface.Lib.Service;

public class DataLogService
{
    private readonly DataCommunicationService _dataCommunicationService;
    private readonly CommandCommunicationService _commandCommunicationService;
    
    public DataLogService(DataCommunicationService dataCommunicationService, CommandCommunicationService commandCommunicationService)
    {
        _dataCommunicationService = dataCommunicationService;
        _commandCommunicationService = commandCommunicationService;
        _dataCommunicationService.OnParseFrameDataAction = UpdateRecordData;
        PeriodicActionTimer = new PeriodicActionTimer(BackupCsvLog);
    }

    public List<List<float>> RecordData { get; } = new();
    
    // todo: Add recurrent save file action
    public DataLogConfig DataLogConfig = new();
    public PeriodicActionTimer PeriodicActionTimer { get; set; }
    public bool ExportCsvTempWithHeader { get; set; } = true;
    public int BackupLogStartId { get; set; } = 0;
    public int BackupLogEndId { get; set; }
    
    // extended action for the parsed frame data event
    public Action<int>? OnParseFrameDataAction { get; set; }

    public void StartDataLog()
    {
        PeriodicActionTimer.StartTimer();
    }

    public void StopDataLog()
    {
        PeriodicActionTimer.StopTimer();
    }
    
    
    private void UpdateRecordData()
    {
        var startId = RecordData.Count;
        var receivedFrameData = _dataCommunicationService.ReceivedDataFrameList;
        var endId = receivedFrameData.Count;
        for (var i = startId; i < endId; i++)
        {
            RecordData.Add(receivedFrameData[i].ParamData.ToFloatArray().ToList());
        }
        OnParseFrameDataAction?.Invoke(startId);
    }
    
    private async void BackupCsvLog()
    {
        var backupFileName = DataLogConfig.TempFileFullName + ".tmp";
        BackupLogEndId = RecordData.Where(d => d.Count > 0).Select(d => d.Count).Min();
        await ExportCsvLog(backupFileName, BackupLogStartId, BackupLogEndId, ExportCsvTempWithHeader);
        BackupLogStartId = BackupLogEndId;
        ExportCsvTempWithHeader = false;
    }

    private async Task ExportLog(string logDataFullName, string exportType)
    {
        switch (exportType)
        {
            case "csv":
                await ExportCsvLog(logDataFullName);
                break;
            case "mat":
                await ExportMatLog(logDataFullName);
                break;
        }
    }

    private async Task ExportMatLog(string logDataFullName)
    {
        var exportMatDict = GetRecordMatDict();
        await Task.Run(() => MatlabWriter.Write(logDataFullName, exportMatDict));
    }

    private async Task ExportCsvLog(string logDataFullName)
    {
        var recordString = GetRecordString();
        await using var outputFile = new StreamWriter(logDataFullName);
        await outputFile.WriteAsync(recordString);
    }

    private async Task ExportCsvLog(string logDataFullName, int startId, int endId, bool withHeader = false)
    {
        var recordString = GetRecordString(startId, endId, withHeader);
        await using var outputFile = new StreamWriter(logDataFullName, append: true);
        await outputFile.WriteAsync(recordString);
    }

    private string GetRecordString(int startId, int endId, bool withHeader = false)
    {
        var sb = new StringBuilder();
        if (withHeader)
        {
            sb.AppendLine(string.Join(',', _commandCommunicationService.RecordSymbolName));
        }

        var maxLength = RecordData.Where(d => d.Count > 0).Select(d => d.Count).Min();
        endId = endId < maxLength ? endId : maxLength;
        for (var i = startId; i < endId; i++)
            sb.AppendLine($"{string.Join(',', RecordData.Select(d => d[i].ToString(CultureInfo.InvariantCulture)))}");
        return sb.ToString();
    }

    private string GetRecordString()
    {
        var sb = new StringBuilder();
        var maxLength = RecordData.Where(d => d.Count > 0).Select(d => d.Count).Min();
        sb.AppendLine(string.Join(',', _commandCommunicationService.RecordSymbolName));
        for (var i = 0; i < maxLength; i++)
            sb.AppendLine($"{string.Join(',', RecordData.Select(d => d[i].ToString(CultureInfo.InvariantCulture)))}");
        return sb.ToString();
    }

    private Dictionary<string, Matrix<float>> GetRecordMatDict()
    {
        var exportMatDict = new Dictionary<string, Matrix<float>>();
        var maxLength = RecordData.Where(d => d.Count > 0).Select(d => d.Count).Min();
        for (var i = 0; i < _commandCommunicationService.RecordSymbolName.Count; i++)
        {
            exportMatDict.Add(_commandCommunicationService.RecordSymbolName[i].Replace(".", "_"),
                Matrix<float>.Build.Dense(maxLength, 1, RecordData[i].Take(maxLength).ToArray()));
        }

        return exportMatDict;
    }

    
}