using System.IO.Ports;
using MotionInterface.Lib.Model;
using MotionInterface.Lib.Util;

namespace MotionInterface.Lib.Service;

public class DataCommunicationService
{
    public readonly SerialPort SerialPort = new();
    private ProtocolFrame _protocolFrame = new();
    
    private readonly ProtocolParserService _protocolParserService = new();
    private readonly PeriodicActionTimer _periodicActionTimer;
    public readonly List<ProtocolFrame> ReceivedDataFrameList = new ();
    
    public DataCommunicationService()
    {
        SerialPort.BaudRate = 921600;
        SerialPort.DataReceived += PortDataReceived;
        SerialPort.ReadBufferSize = ProtocolConfig.ProtocolRecursiveBufferSize;
        // 5ms read once, and clear all data
        _periodicActionTimer = new PeriodicActionTimer(ParseReceivedFrames, 2);
    }

    public bool IsPortOpen => SerialPort.IsOpen;
    
    // extended action for the parsed frame data event
    public Action? OnParseFrameDataAction { get; set; }
    
    #region SendFrameData

    public void SendFrameData(ProtocolFrame protocolFrame)
    {
        var data = new byte[ProtocolConfig.ProtocolFrameMaxSize];
        protocolFrame.SerializeFrameData(ref data);
        SerialPort.Write(data, 0, protocolFrame.Length);
    }
    
    #endregion
    
    #region ReceiveFrameData

    /// <summary>
    /// callback function of SerialPort receive data
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
    /// PeriodicActionTimer callback, default 10ms /execution
    /// </summary>
    private void ParseReceivedFrames()
    {
        while (_protocolParserService.GetRemainLength() >= ProtocolConfig.ProtocolFrameMaxSize)
        {
            _protocolParserService.ProtocolDataHandler();
            if (_protocolParserService.ProtocolFrame.Command != ProtocolCommand.DataLogRunningCmd) continue;
            _protocolFrame = _protocolParserService.ProtocolFrame;
            ReceivedDataFrameList.Add(_protocolFrame);
        }
        // extension action
        OnParseFrameDataAction?.Invoke();
    }

    #endregion

    #region util functions

   
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