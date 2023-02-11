namespace CanUpdater.Can;

public struct CanFrame
{
    public uint Id { get; init; }
    public IdType IdType;
    public byte Dlc { get; set; }
    public byte[] Payload { get; set; }
}

public enum IdType
{
    Normal,
    Extended
}

public enum Baudrate
{
    Baud250k,
    Baud500k,
    Baud1000k
}