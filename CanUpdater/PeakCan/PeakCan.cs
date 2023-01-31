using System.Text;
using Serilog;
using Peak.Can.Basic;

namespace CanUpdater;

public class PeakCan : ICanDevice
{
    private readonly ILogger _logger;
    private const string DeviceType = $"{PCANBasic.LOOKUP_DEVICE_TYPE}=PCAN_USB";
    private ushort _handle;
    private const ushort English = 0x09;

    public PeakCan(ILogger logger)
    {
        _logger = logger;
        CheckForLibrary();
    }

    public bool Connect()
    {
        var status = PCANBasic.LookUpChannel(DeviceType, out var handle);
        if (status != TPCANStatus.PCAN_ERROR_OK)
        {
            _logger.Error("Hardware not detected");
            return false;
        }

        _handle = handle;
        return true;
    }

    public void Configure(CanDeviceConfig config)
    {
        Connect();
        PCANBasic.Initialize(_handle, TPCANBaudrate.PCAN_BAUD_500K);
    }

    public void Disconnect()
    {
        PCANBasic.Uninitialize(_handle);
    }

    public void SendFrame(CanFrame frame)
    {
        var msg = GetPcanMessage(frame);
        var result = PCANBasic.Write(_handle, ref msg);
        if (result != TPCANStatus.PCAN_ERROR_OK)
        {
            LogPeakcanError(result);
        }
    }

    private TPCANMsg GetPcanMessage(CanFrame frame)
    {
        var msg = new TPCANMsg
        {
            ID = frame.Id,
            MSGTYPE = frame.IdType == IdType.Extended
                ? TPCANMessageType.PCAN_MESSAGE_EXTENDED
                : TPCANMessageType.PCAN_MESSAGE_STANDARD,
            LEN = Convert.ToByte(frame.Dlc),
            DATA = new byte[8]
        };
        frame.Payload.CopyTo(msg.DATA,0);
        return msg;
    }

    public void SendFrameCyclic(CanFrame frame, int cycleTime)
    {
        // var msg = GetPcanMessage(frame);
        // var broadcast = new Broadcast(msg, cycleTime);
        // _worker.AddBroadcast(ref broadcast);
    }

    public void SubscribeFrame()
    {
        throw new NotImplementedException();
    }

    public void UnsubscribeFrame()
    {
        throw new NotImplementedException();
    }

    public void GetFrame()
    {
        throw new NotImplementedException();
    }

    public List<string> GetAvailableChannels()
    {
        throw new NotImplementedException();
    }

    private void LogPeakcanError(TPCANStatus status)
    {
        var stringBuffer = new StringBuilder();
        PCANBasic.GetErrorText(status, English, stringBuffer);
        _logger.Error("Error message : {Message}", stringBuffer.ToString());
    }

    public bool Open()
    {
        // AssertHandleValid();
        var status = PCANBasic.Initialize(_handle, TPCANBaudrate.PCAN_BAUD_1M);
        if (status != TPCANStatus.PCAN_ERROR_OK)
        {
            LogPeakcanError(status);
            return false;
        }

        _logger.Information("Opened channel {Channel}, baudrate {Baudrate}", _handle,
            TPCANBaudrate.PCAN_BAUD_1M.ToString());
        return true;
    }


    private void CheckForLibrary()
    {
        try
        {
            PCANBasic.Uninitialize(PCANBasic.PCAN_NONEBUS);
        }
        catch (DllNotFoundException)
        {
            _logger.Fatal("Unable to find the library: PCANBasic.dll");
            throw;
        }
    }

    // private PcanMessage GetPcanMessage(CanFrame frame)
    // {
    //     if (_handle != 0)
    //     {
    //         return;
    //     }
    //
    //     const string msg = "Channel handle value invalid";
    //     _logger.Fatal(msg);
    //     throw new InvalidDataException(msg);
    // }

    public void Close()
    {
        // AssertHandleValid();
        _logger.Information("Close channel");
        PCANBasic.Uninitialize(_handle);
    }
}