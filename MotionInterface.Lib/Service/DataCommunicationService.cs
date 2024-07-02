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
        // 1ms read once, and clear all data
        _periodicActionTimer = new PeriodicActionTimer(ParseReceivedFrames, 1);
    }

    public bool IsPortOpen => SerialPort.IsOpen;

    /// <summary>
    /// extended action for the parsed frame data event
    /// </summary>
    public Action? OnParseFrameDataAction { get; set; }

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
    /// PeriodicActionTimer callback, default 1kHz execution
    /// </summary>
    private void ParseReceivedFrames()
    {
        while (_protocolParserService.GetRemainLength() >= ProtocolConfig.ProtocolFrameMaxSize)
        {
            _protocolParserService.ProtocolDataHandler();
            if (_protocolParserService.ProtocolFrame.Command != ProtocolCommand.DataLogRunningCmd) continue;
            _protocolFrame = _protocolParserService.ProtocolFrame;  // here we dont use deepclone because the target param data is already been cloned inside the handler()
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
    

    #endregion
    
}