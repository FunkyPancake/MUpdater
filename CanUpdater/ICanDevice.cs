namespace CanUpdater;

public interface ICanDevice
{
    public bool Connect();
    public void Configure(CanDeviceConfig config);
    public void Disconnect();
    public void SendFrame();
    public void SendFrameCyclic();
    public void SubscribeFrame();
    public void UnsubscribeFrame();
    public void GetFrame();
    public List<uint> GetAvailableChannels();
}