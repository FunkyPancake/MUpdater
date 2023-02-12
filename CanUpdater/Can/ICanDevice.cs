namespace CanUpdater.Can;

public interface ICanDevice
{
    public delegate void NewFrameReceivedEventHandler(object sender, NewFrameRecievedEventArgs e);

    public bool Connect();
    public void Configure(CanDeviceConfig config);
    public void Disconnect();
    public void SendFrame(CanFrame frame);
    public void SendFrames(IReadOnlyList<CanFrame> frames);
    public void StartSendFrameCyclic(CanFrame frame, int cycleTime);
    public void StopSendFrameCyclic(CanFrame frame);
    public void SubscribeFrame(CanFrame frame, NewFrameReceivedEventHandler handler);
    public void UnsubscribeFrame(CanFrame frame);
    public void GetFrame(out CanFrame frame);
}

public class NewFrameRecievedEventArgs : EventArgs
{
    public CanFrame Frame { get; set; }
}