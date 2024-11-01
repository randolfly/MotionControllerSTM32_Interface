﻿using System.Globalization;
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
    private readonly AppConfigService _appConfigService;

    public DataLogService(DataCommunicationService dataCommunicationService,
        CommandCommunicationService commandCommunicationService,
        AppConfigService appConfigService)
    {
        _dataCommunicationService = dataCommunicationService;
        _commandCommunicationService = commandCommunicationService;
        _appConfigService = appConfigService;
        _dataCommunicationService.OnParseFrameDataAction = UpdateRecordData;
        PeriodicActionTimer = new PeriodicActionTimer(BackupCsvLog);

        DataLogConfig = _appConfigService.AppConfig.DataLogConfig;
    }

    /// <summary>
    /// record data list, shape: [symbol count, data count], double data
    /// example: 3 symbol data, 1000 data count, shape: [1000, 3]
    /// </summary>
    public List<List<double>> RecordData { get; } = new();

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
        BackupLogStartId = 0;
        _dataCommunicationService.OpenPort();
        PeriodicActionTimer.StartTimer();
    }

    public void StopDataLog()
    {
        // stop tmp data export
        PeriodicActionTimer.StopTimer();
        _dataCommunicationService.ClosePort();
    }

    private void UpdateRecordData()
    {
        var startId = RecordData.Count;
        var receivedFrameData = _dataCommunicationService.ReceivedDataFrameList;
        var endId = receivedFrameData.Count;
        for (var i = startId; i < endId; i++)
        {
            RecordData.Add(receivedFrameData[i].ParamData.ByteArrayToDoubleArray().ToList());
        }
        OnParseFrameDataAction?.Invoke(startId);
    }

    private async void BackupCsvLog()
    {
        var backupFileName = _appConfigService.AppConfig.DataLogConfig.TempFileFullName + ".tmp";
        BackupLogEndId = RecordData.Count;
        await ExportCsvLog(backupFileName, BackupLogStartId, BackupLogEndId, ExportCsvTempWithHeader);
        BackupLogStartId = BackupLogEndId;
        ExportCsvTempWithHeader = false;
    }

    public async Task ExportLog(string logDataFullName, DataExportTypes exportType)
    {
        switch (exportType)
        {
            case DataExportTypes.csv:
                await ExportCsvLog(logDataFullName);
                break;
            case DataExportTypes.mat:
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

        for (var i = startId; i < endId; i++)
            sb.AppendLine(string.Join(',', RecordData[i].Select(s => s.ToString(CultureInfo.InvariantCulture))));
        return sb.ToString();
    }

    private string GetRecordString()
    {
        var sb = new StringBuilder();
        var maxLength = RecordData.Count;
        sb.AppendLine(string.Join(',', _commandCommunicationService.RecordSymbolName));
        for (var i = 0; i < maxLength; i++)
            sb.AppendLine(string.Join(',', RecordData[i].Select(s => s.ToString(CultureInfo.InvariantCulture))));
        return sb.ToString();
    }

    private Dictionary<string, Matrix<double>> GetRecordMatDict()
    {
        var exportMatDict = new Dictionary<string, Matrix<double>>();
        var maxLength = RecordData.Count;
        for (var i = 0; i < _commandCommunicationService.RecordSymbolName.Count; i++)
        {
            var recordSymbolData = Matrix<double>.Build
                .Dense(maxLength, 1, RecordData.Select(d => d[i]).ToArray());
            exportMatDict.Add(_commandCommunicationService.RecordSymbolName[i].Replace(".", "_"), recordSymbolData);
        }

        return exportMatDict;
    }


}