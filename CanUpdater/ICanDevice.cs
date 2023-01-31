namespace CanUpdater;

public interface ICanDevice
{
    public bool Connect();
    public void Configure(CanDeviceConfig config);
    public void Disconnect();
    public void SendFrame(CanFrame frame);
    public void SendFrameCyclic(CanFrame frame, int cycleTime);
    public void SubscribeFrame();
    public void UnsubscribeFrame();
    public void GetFrame();
    public List<string> GetAvailableChannels();
}