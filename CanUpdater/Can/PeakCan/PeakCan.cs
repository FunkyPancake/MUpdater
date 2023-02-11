using System.ComponentModel;
using System.Text;
using Serilog;

namespace CanUpdater.Can.PeakCan;

public class PeakCan : ICanDevice
{
    private const string PCAN_DEVICE_TYPE = $"{PCANBasic.LOOKUP_DEVICE_TYPE}=PCAN_USB";
    private const ushort PCAN_ENGLISH = 0x09;
    private readonly ILogger _logger;
    private CanDeviceConfig _config = null!;
    private ushort _handle;
    private readonly Thread _thread;
    private bool _threadRun;
    private EventHandlerList _eventHandlers = new();

    public PeakCan(ILogger logger)
    {
        _logger = logger;
        CheckForLibrary();
        _thread = new Thread(ReadThread);
    }

    public bool Connect()
    {
        var status = PCANBasic.Initialize(_handle, ConvertBaudrate(_config.Baudrate));
        if (status != TPCANStatus.PCAN_ERROR_OK)
        {
            _logger.Error("Hardware not detected");
            return false;
        }

        _logger.Information("Opened channel {Channel}, baudrate {Baudrate}", _handle,
            TPCANBaudrate.PCAN_BAUD_1M.ToString());
        _threadRun = true;
        _thread.Start();
        return true;
    }

    public void Configure(CanDeviceConfig config)
    {
        _config = config;
        var status = PCANBasic.LookUpChannel(PCAN_DEVICE_TYPE, out var handle);
        if (status != TPCANStatus.PCAN_ERROR_OK)
        {
            _logger.Error("Hardware not detected");
            return;
        }

        _handle = handle;
    }

    public void Disconnect()
    {
        _logger.Information($"Close channel {_handle}");
        PCANBasic.Uninitialize(_handle);
        _thread.Join();
    }

    public void SendFrame(CanFrame frame)
    {
        var msg = GetPcanMessage(frame);
        var result = PCANBasic.Write(_handle, ref msg);
        if (result != TPCANStatus.PCAN_ERROR_OK)
        {
            LogError(result);
        }
    }

    public void StartSendFrameCyclic(CanFrame frame, int cycleTime)
    {
        throw new NotImplementedException();
    }

    public void StopSendFrameCyclic(CanFrame frame)
    {
        throw new NotImplementedException();
    }

    public void SubscribeFrame(CanFrame frame, ICanDevice.NewFrameRecievedEventHandler handler)
    {
        _eventHandlers.AddHandler(frame.Id, handler);
    }

    public void UnsubscribeFrame(CanFrame frame)
    {
        var e = (ICanDevice.NewFrameRecievedEventHandler?) _eventHandlers[frame.Id];
        if (e is null)
        {
            _logger.Error("Frame ID:{} not subscribed",frame.Id);
            return;
        }
        _eventHandlers.RemoveHandler(frame.Id, e);
    }

    public void GetFrame(out CanFrame frame)
    {
        frame = new CanFrame();
        throw new NotImplementedException();
    }

    public List<string> GetAvailableChannels()
    {
        throw new NotImplementedException();
    }

    private void ReadThread()
    {
        var evtReceiveEvent = new AutoResetEvent(false);
        var iBuffer = Convert.ToUInt32(evtReceiveEvent.SafeWaitHandle.DangerousGetHandle().ToInt32());
        var stsResult = PCANBasic.SetValue(_handle, TPCANParameter.PCAN_RECEIVE_EVENT, ref iBuffer, sizeof(uint));
        if (stsResult != TPCANStatus.PCAN_ERROR_OK)
        {
            LogError(stsResult);
            return;
        }

        while (_threadRun)
            // Checks for messages when an event is received
            if (evtReceiveEvent.WaitOne(50))
                OnFrameReceived();

        // Removes the Receive-Event again.
        iBuffer = 0;
        stsResult = PCANBasic.SetValue(_handle, TPCANParameter.PCAN_RECEIVE_EVENT, ref iBuffer, sizeof(uint));

        if (stsResult != TPCANStatus.PCAN_ERROR_OK)
            LogError(stsResult);
        evtReceiveEvent.Dispose();
    }

    private void OnFrameReceived()
    {
        TPCANStatus stsResult;
        do
        {
            stsResult = TPCANStatus.PCAN_ERROR_OK;
            if (ReadMessage(out var frame))
            {
                var e = (ICanDevice.NewFrameRecievedEventHandler?) _eventHandlers[frame.Id];
                e?.Invoke(this, new NewFrameRecievedEventArgs {Frame = frame});
            }
        } while (!Convert.ToBoolean(stsResult & TPCANStatus.PCAN_ERROR_QRCVEMPTY));
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
        frame.Payload.CopyTo(msg.DATA, 0);
        return msg;
    }

    private void LogError(TPCANStatus status)
    {
        var stringBuffer = new StringBuilder();
        PCANBasic.GetErrorText(status, PCAN_ENGLISH, stringBuffer);
        _logger.Error("Error message : {Message}", stringBuffer.ToString());
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

    private static TPCANBaudrate ConvertBaudrate(Baudrate baudrate)
    {
        var value = baudrate switch
        {
            Baudrate.Baud250k => TPCANBaudrate.PCAN_BAUD_250K,
            Baudrate.Baud500k => TPCANBaudrate.PCAN_BAUD_500K,
            Baudrate.Baud1000k => TPCANBaudrate.PCAN_BAUD_1M,
            _ => throw new ArgumentOutOfRangeException(nameof(baudrate), baudrate, null)
        };

        return value;
    }

    private bool ReadMessage(out CanFrame frame)
    {
        // We execute the "Read" function of the PCANBasic     
        TPCANStatus stsResult = PCANBasic.Read(_handle, out TPCANMsg CANMsg, out TPCANTimestamp CANTimeStamp);
        // if (stsResult != TPCANStatus.PCAN_ERROR_QRCVEMPTY)
        // We process the received message
        // ProcessMessageCan(CANMsg, CANTimeStamp);
        // ulong microsTimestamp = Convert.ToUInt64(itsTimeStamp.micros + 1000 * itsTimeStamp.millis + 0x100000000 * 1000 * itsTimeStamp.millis_overflow);
        frame = new CanFrame();
        return true;
        // return stsResult;
    }
}