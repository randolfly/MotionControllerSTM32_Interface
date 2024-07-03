using System.IO.Ports;
using MotionInterface.Lib.Model;
using MotionInterface.Lib.Util;
using System.Management;
using System.Text.RegularExpressions;

namespace MotionInterface.Lib.Service;

/// <summary>
/// command uart port protocol send and receive service
/// </summary>
public class CommandCommunicationService
{
    public readonly SerialPort SerialPort = new();
    public readonly List<ProtocolFrame> ReceivedProtocolFrameList = new ();
    public readonly List<ProtocolFrame> SendProtocolFrameList = new ();

    /// <summary>
    /// current protocol frame(for send and receive)
    /// </summary>
    public ProtocolFrame ProtocolFrame = new();
    
    private readonly ProtocolParserService _protocolParserService = new();
    private readonly PeriodicActionTimer _periodicActionTimer;
    
    public List<string> AvailableSymbolName { get; set; } = new();
    public List<string> RecordSymbolName { get; set; } = new();
    public List<string> EchoRecordSymbolName { get; set; } = new();
    public List<string> GraphSymbolName { get; set; } = new();
    
    public CommandCommunicationService()
    {
        SerialPort.BaudRate = 115200;
        SerialPort.ReadBufferSize = 4096;
        SerialPort.DataReceived += PortDataReceived;
        _periodicActionTimer = new PeriodicActionTimer(ParseReceivedFrames, 100);
    }

    public bool IsPortOpen => SerialPort.IsOpen;

    /// <summary>
    /// extended action for the parsed frame data event
    /// </summary>
    public Action<ProtocolFrame>? OnParseFrameDataAction { get; set; }

    /// <summary>
    /// invokes when available symbol name changed
    /// </summary>
    public Action OnAvailableSymbolNameChanged { get; set; }

    #region SendFrameData

    public void SendFrameData(ProtocolFrame protocolFrame)
    {
        var data = new byte[ProtocolConfig.ProtocolFrameMaxSize];
        protocolFrame.SerializeFrameData(ref data);
        SerialPort.Write(data, 0, protocolFrame.Length);
        SendProtocolFrameList.Add(protocolFrame.DeepClone());
    }
    
    #endregion
    
    #region ReceiveFrameData

    /// <summary>
    /// serial port DataReceived event handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void PortDataReceived(object sender, SerialDataReceivedEventArgs args)
    {
        var dataReceived = ((SerialPort)sender).BytesToRead;
        if (dataReceived <= 0) return;
        var data = new byte[dataReceived];
        SerialPort.Read(data, 0, dataReceived);
        _protocolParserService.ProtocolDataReceive(ref data, (ushort)dataReceived);
    }
    
    /// <summary>
    /// parse the first received frame data.
    /// periodically execution, default period is 10Hz
    /// </summary>
    private void ParseReceivedFrames()
    {
        _protocolParserService.ProtocolDataHandler();
        if (_protocolParserService.ProtocolFrame == null) return;
        if (_protocolParserService.ProtocolFrame.Command == ProtocolCommand.NullCmd) return;
        ProtocolFrame = _protocolParserService.ProtocolFrame;
        ReceivedProtocolFrameList.Add(ProtocolFrame.DeepClone());
        switch (ProtocolFrame.Command)
        {
            case ProtocolCommand.DataLogSendAvailableDataCmd:
            {
                AvailableSymbolName = ProtocolFrame.ParamData.ByteArrayToNameStringList();
                OnAvailableSymbolNameChanged?.Invoke();
                break;
            }
            case ProtocolCommand.DataLogEchoLogDataCmd:
            {
                EchoRecordSymbolName = ProtocolFrame.ParamData.ByteArrayToNameStringList();
                break;
            }
        }

        OnParseFrameDataAction?.Invoke(ProtocolFrame);
    }

    #endregion

    #region util functions

    public void GetAvailableRecordDataNames()
    {
        var checkAvailableDataFrame = new ProtocolFrame
        {
            Command = ProtocolCommand.DataLogCheckAvailableDataCmd,
        };
        SendFrameData(checkAvailableDataFrame);
    }

    public void SetRecordDataNames()
    {
        var setRecordDataFrame = new ProtocolFrame
        {
            Command = ProtocolCommand.DataLogSetLogDataCmd,
            ParamData = RecordSymbolName.NameStringListToByteArray()
        };
        SendFrameData(setRecordDataFrame);
    }

    public void StartRecordData()
    {
        var startRecordDataFrame = new ProtocolFrame
        {
            Command = ProtocolCommand.DataLogStartLogCmd,
        };
        SendFrameData(startRecordDataFrame);
    }
    
    public void StopRecordData()
    {
        var stopRecordDataFrame = new ProtocolFrame
        {
            Command = ProtocolCommand.DataLogStopLogCmd,
        };
        SendFrameData(stopRecordDataFrame);
    }
   
    public void OpenPort()
    {
        try
        {
            if (IsPortOpen) return;
            SerialPort.Open();
            _periodicActionTimer.StartTimer();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    public void ClosePort()
    {
        try
        {
            if (!IsPortOpen) return;
            SerialPort.Close();
            _periodicActionTimer.StopTimer();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public static Dictionary<string, string> GetPortDetailsDictionary()
    {
        Dictionary<string, string> comDictionary = new();
        //note: only capture com00-99
        var pattern = @"(?<info>.*)\((?<com>COM\d\d?)\)$";
        var coms = GetPortDetailsList();
        foreach (var com in coms)
        {
            var match = Regex.Match(com, pattern);
            if (match.Success)
            {
                var info = match.Groups["info"].ToString();
                var comName = match.Groups["com"].ToString();
                comDictionary.Add($"{info}: {comName}", comName);
            }
        }

        return comDictionary;
    }

    /// <summary>
    /// ONLY support WINDOWS!!!
    /// </summary>
    private static List<string> GetPortDetailsList()
    {
        var infos = new List<string>();
        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher
                   ("select * from Win32_PnPEntity where Name like '%(COM%'"))
        {
            var hardInfos = searcher.Get();
            foreach (var hardInfo in hardInfos)
            {
                if (hardInfo.Properties["Name"].Value == null) continue;
                var deviceName = hardInfo.Properties["Name"].Value.ToString()!;
                infos.Add(deviceName);
            }
        }
        return infos;
    }
    
    #endregion
    
    
}