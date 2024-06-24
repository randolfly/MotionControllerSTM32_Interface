using System.IO.Ports;
using MotionInterface.Lib.Model;
using MotionInterface.Lib.Util;

namespace MotionInterface.Lib.Service;

public class CommandCommunicationService
{
    public readonly SerialPort SerialPort = new();
    public readonly List<ProtocolFrame> ReceivedProtocolFrameList = new ();
    public readonly List<ProtocolFrame> SendProtocolFrameList = new ();
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
        SerialPort.DataReceived += PortDataReceived;
        _periodicActionTimer = new PeriodicActionTimer(ParseReceivedFrames, 100);
    }

    public bool IsPortOpen => SerialPort.IsOpen;
    
    // extended action for the parsed frame data event
    public Action<ProtocolFrame>? OnParseFrameDataAction { get; set; }

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

    private void PortDataReceived(object sender, SerialDataReceivedEventArgs args)
    {
        var dataReceived = ((SerialPort)sender).BytesToRead;
        if (dataReceived <= 0) return;
        var data = new byte[dataReceived];
        SerialPort.Read(data, 0, dataReceived);
        _protocolParserService.ProtocolDataReceive(ref data, (ushort)dataReceived);
    }
    private void ParseReceivedFrames()
    {
        _protocolParserService.ProtocolDataHandler();
        if (_protocolParserService.ProtocolFrame?.Command == ProtocolCommand.NullCmd) return;
        if (_protocolParserService.ProtocolFrame == null) return;
        ProtocolFrame = _protocolParserService.ProtocolFrame;
        ReceivedProtocolFrameList.Add(_protocolParserService.ProtocolFrame.DeepClone());
        switch (ProtocolFrame.Command)
        {
            case ProtocolCommand.DataLogSendAvailableDataCmd:
            {
                AvailableSymbolName = ProtocolFrame.ParamData.ByteArrayToNameString();
                break;
            }
            case ProtocolCommand.DataLogEchoLogDataCmd:
            {
                EchoRecordSymbolName = ProtocolFrame.ParamData.ByteArrayToNameString();
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
            ParamData = RecordSymbolName.NameStringToByteArray()
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
    
    public void SetReceiveBytesThreshold(int threshold)
    {
        // default is 1
        SerialPort.ReceivedBytesThreshold = threshold;
    }

    #endregion
    
    
}