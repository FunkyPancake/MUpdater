namespace FirmwarePack; 

public struct Version {
    public uint Major, Minor, Patch;
    public override string ToString() {
        return $"{Major}.{Minor}.{Patch}";
    }
}