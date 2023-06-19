namespace FirmwarePack; 

public struct Version {
    public uint Major, Minor, Patch;
    public override string ToString() {
        return $"{Major}.{Minor}.{Patch}";
    }

    public Version(string str) {
        var split = str.Split('.');
        Major = uint.Parse(split[0]);
        Minor = uint.Parse(split[1]);
        Patch = uint.Parse(split[2]);
    }
}