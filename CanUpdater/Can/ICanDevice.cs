namespace CanUpdater.Can;

public interface ICanDevice
{
    public delegate void NewFrameRecievedEventHandler(object sender, NewFrameRecievedEventArgs e);

    public bool Connect();
    public void Configure(CanDeviceConfig config);
    public void Disconnect();
    public void SendFrame(CanFrame frame);
    public void StartSendFrameCyclic(CanFrame frame, int cycleTime);
    public void StopSendFrameCyclic(CanFrame frame);
    public void SubscribeFrame(CanFrame frame, NewFrameRecievedEventHandler handler);
    public void UnsubscribeFrame(CanFrame frame);
    public void GetFrame(out CanFrame frame);
    public List<string> GetAvailableChannels();
}

public class NewFrameRecievedEventArgs : EventArgs
{
    public CanFrame Frame { get; set; }
}