﻿using System.IO.Ports;
using MotionInterface.Lib.Model;
using MotionInterface.Lib.Util;

namespace MotionInterface.Lib.Service;

public class CommandCommunicationService
{
    public SerialPort SerialPort = new();
    public readonly List<ProtocolFrame> ReceivedProtocolFrameList = new ();
    public readonly List<ProtocolFrame> SendProtocolFrameList = new ();
    public ProtocolFrame ProtocolFrame = new();
    
    private readonly ProtocolParserService ProtocolParserService = new();
    private readonly PeriodicActionTimer _periodicActionTimer;
    
    public CommandCommunicationService()
    {
        SerialPort.DataReceived += PortDataReceived;
        _periodicActionTimer = new PeriodicActionTimer(ParseReceivedFrames, 100);
    }

    public bool IsPortOpen => SerialPort.IsOpen;
    
    // extended action for the parsed frame data event
    public Action<ProtocolFrame>? OnParseFrameDataAction { get; set; }

    #region SendFrameData

    public void SendFrameData(ProtocolFrame protocolFrame)
    {
        var data = new byte[ProtocolConfig.ProtocolRecursiveBufferSize];
        protocolFrame.SerializeFrameData(ref data);
        SerialPort.Write(data, 0, protocolFrame.Length);
        SendProtocolFrameList.Add(protocolFrame);
    }
    
    #endregion
    
    #region ReceiveFrameData

    private void PortDataReceived(object sender, SerialDataReceivedEventArgs args)
    {
        var dataReceived = ((SerialPort)sender).BytesToRead;
        if (dataReceived <= 0) return;
        var data = new byte[dataReceived];
        SerialPort.Read(data, 0, dataReceived);
        ProtocolParserService.ProtocolDataReceive(ref data, (ushort)dataReceived);
    }
    private void ParseReceivedFrames()
    {
        ProtocolParserService.ProtocolDataHandler();
        if (ProtocolParserService.ProtocolFrame?.Command == ProtocolCommand.NullCmd) return;
        if (ProtocolParserService.ProtocolFrame == null) return;
        ProtocolFrame = ProtocolParserService.ProtocolFrame;
        ReceivedProtocolFrameList.Add(ProtocolParserService.ProtocolFrame);
        OnParseFrameDataAction?.Invoke(ProtocolFrame);
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