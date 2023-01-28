namespace CanUpdater;


 
public interface ICanDevice
{
    public delegate void NewFrameHandler(CanFrame frame);
    public bool Connect();
    public void Configure(CanDeviceConfig config);
    public void Disconnect();
    public void SendFrame(CanFrame frame);
    public void StartSendFrameCyclic(CanFrame frame, int cycleTime);
    public void StopSendFrameCyclic(CanFrame frame);
    public void SubscribeFrame(CanFrame frame, NewFrameHandler handler);
    public void UnsubscribeFrame(CanFrame frame);
    public void GetFrame(ref CanFrame frame);
    public List<string> GetAvailableChannels();
}