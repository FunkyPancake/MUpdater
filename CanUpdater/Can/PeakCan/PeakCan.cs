using System.ComponentModel;
using System.Text;
using Serilog;
using Peak.Can.Basic;


namespace CanUpdater.Can.PeakCan;

public class PeakCan : ICanDevice {
    private readonly Dictionary<uint, ICanDevice.NewFrameReceivedEventHandler> _eventHandlers = new();
    private readonly Dictionary<uint, int> _broadcastDictionary = new();
    private readonly ILogger _logger;
    private CanDeviceConfig _config = null!;
    private PcanChannel _handle = PcanChannel.None;
    private Worker _worker = new();

    public PeakCan(ILogger logger) {
        _logger = logger;
    }

    public bool Connect() {
        _worker = new Worker(_handle, ConvertBitrate(_config.Baudrate));
        _worker.MessageAvailable += OnMessageAvailable;
        _worker.Start();

        _logger.Information("Opened channel {Channel}, bitrate {Baudrate}", _handle, _config.Baudrate);
        return true;
    }

    public void Configure(CanDeviceConfig config) {
        _config = config;
        var parameter = ParameterValue.LookUp.GetCriteriaString(PcanDevice.PcanUsb, null, null, null);
        var status = Api.LookUpChannel(parameter, out var handle);
        if (status != PcanStatus.OK) {
            _logger.Error("Pcan Status = {status}", status);
            return;
        }

        _handle = handle;
    }

    public void Disconnect() {
        _logger.Information($"Close channel {_handle}");
        _worker.Stop(true, true, false);
        Api.Uninitialize(_handle);
    }

    public void SendFrame(CanFrame frame) {
        var msg = GetPcanMessage(frame);
        if (!_worker.Transmit(msg, out var status)) {
            _logger.Error("Transmit failed, reason: {status}", status);
        }
    }

    public void SendFrames(IReadOnlyList<CanFrame> frames) {
        foreach (var frame in frames) {
            SendFrame(frame);
        }
    }

    public void StartSendFrameCyclic(CanFrame frame, int cycleTime) {
        int broadcastId;
        if (_broadcastDictionary.ContainsKey(frame.Id)) {
            broadcastId = _broadcastDictionary[frame.Id];
            _worker.UpdateBroadcast(broadcastId, cycleTime, GetPcanMessage(frame));
            _logger.Information("Update cyclic transmit, frame ID:{id}, new cycle time: {cycleTime}", frame.Id,
                cycleTime);
        }
        else {
            broadcastId = _worker.AddBroadcast(GetPcanMessage(frame), cycleTime);
            _broadcastDictionary.Add(frame.Id, broadcastId);
            _logger.Information("Add new cyclic transmit, frame ID:{id}, cycle time: {cycleTime}", frame.Id, cycleTime);
        }

        _worker.ResumeBroadcast(broadcastId);
    }

    public void StopSendFrameCyclic(CanFrame frame) {
        if (_broadcastDictionary.ContainsKey(frame.Id)) {
            var broadcastId = _broadcastDictionary[frame.Id];
            _worker.PauseBroadcast(broadcastId);
            _logger.Information("Cyclic transmit stopped, frame ID:{id}", frame.Id);
        }
        else {
            _logger.Error("Frame ID:{id} is not cyclic TX", frame.Id);
        }
    }

    public void SubscribeFrame(CanFrame frame, ICanDevice.NewFrameReceivedEventHandler handler) {
        _eventHandlers.Add(GetId(frame.Id), handler);
        _logger.Information("Subscribe frame ID:{id}", GetId(frame.Id));
    }

    public void UnsubscribeFrame(CanFrame frame) {
        if (!_eventHandlers.ContainsKey(GetId(frame.Id))) {
            _logger.Error("Frame ID:{id} not subscribed", frame.Id);
            return;
        }

        _eventHandlers.Remove(GetId(frame.Id));
        _logger.Information("Unsubscribe frame ID:{id}", GetId(frame.Id));
    }

    public void GetFrame(out CanFrame frame) {
        frame = new CanFrame();
        throw new NotImplementedException();
    }

    private void OnMessageAvailable(object? sender, MessageAvailableEventArgs e) {
        if (_worker.Dequeue(e.QueueIndex, out var message, out var timestamp)) {
            var str = new StringBuilder(message.Data.MaxLength * 4 + 1);
            for (var i = 0; i < message.DLC; i++) {
                str.Append($"0x{message.Data[i]:x2} ");
            }

            _logger.Information("Rx Message Timestamp:{timestamp} ID:{id:X}, DLC:{dlc}, payload:{payload}", timestamp,
                message.ID, message.DLC, str);
            if (_eventHandlers.TryGetValue(message.ID, out var ev)) {
                ev.Invoke(this, new NewFrameRecievedEventArgs());
            }
        }
        else {
            _logger.Error("Cannot dequeue queue with Index:{idx}", e.QueueIndex);
        }
    }

    private static PcanMessage GetPcanMessage(CanFrame frame) {
        var msg = new PcanMessage {
            ID = frame.Id & 0x7fffffff,
            MsgType = GetMessageType(frame.Id),
            DLC = frame.Dlc,
            Data = frame.Payload
        };
        return msg;
    }

    private static Bitrate ConvertBitrate(Baudrate bitrate) {
        var value = bitrate switch {
            Baudrate.Baud125k => Bitrate.Pcan125,
            Baudrate.Baud250k => Bitrate.Pcan250,
            Baudrate.Baud500k => Bitrate.Pcan500,
            Baudrate.Baud1000k => Bitrate.Pcan1000,
            _ => throw new ArgumentOutOfRangeException(nameof(bitrate), bitrate, null)
        };

        return value;
    }

    private static MessageType GetMessageType(uint id) {
        const uint extIdFlag = 0x80000000;
        return (id & extIdFlag) == extIdFlag ? MessageType.Extended : MessageType.Standard;
    }

    private static uint GetId(uint id) => id & 0x7fffffff;

    // private bool ReadMessage(out CanFrame frame)
    // {
    //     // We execute the "Read" function of the PCANBasic     
    //     TPCANStatus stsResult = PCANBasic.Read(_handle, out TPCANMsg CANMsg, out TPCANTimestamp CANTimeStamp);
    //     // if (stsResult != TPCANStatus.PCAN_ERROR_QRCVEMPTY)
    //     // We process the received message
    //     // ProcessMessageCan(CANMsg, CANTimeStamp);
    //     // ulong microsTimestamp = Convert.ToUInt64(itsTimeStamp.micros + 1000 * itsTimeStamp.millis + 0x100000000 * 1000 * itsTimeStamp.millis_overflow);
    //     frame = new CanFrame();
    //     return true;
    //     // return stsResult;
    // }
}