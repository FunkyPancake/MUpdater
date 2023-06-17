using System.Collections.ObjectModel;
using System.IO.Compression;

namespace FirmwarePack;

/* swPack structure
 * SwPack.msw
 *  -> Manifest.xml
 *      -> Target Ecu
 *      -> Compatible Hardware Versions
 *      -> Release date
 *      -> Sw Version
 *  -> Cert file
 *  -> Hex file
*/
public class FirmwarePack {
    public FirmwarePack() {
    }

    public async Task ProcessFile(string filePath) {
        //check file exists and extension matches
        
        //unpack
        var stream = await DecryptPackage(filePath);
        using var zip = ZipFile.Open(filePath, ZipArchiveMode.Read);
        var entries = zip.Entries;        
        //calc signature
        CheckSignature(entries);
        //read metadata
        ParseMetadata(entries);
        ExtractHex(entries);
    }

    private void ExtractHex(ReadOnlyCollection<ZipArchiveEntry> entries) {
        throw new NotImplementedException();
    }

    private void ParseMetadata(ReadOnlyCollection<ZipArchiveEntry> entries) {
        throw new NotImplementedException();
    }

    public byte[] Hex { get; private set; } = Array.Empty<byte>();
    public string TargetEcu { get; private set; } = string.Empty;
    public DateTime ReleaseDate { get; private set; }
    public Version SwVersion { get; } = new Version();

    public bool CheckEcuCompatibility() {
        return true;
    }


    private async Task<Stream> DecryptPackage() {
    }

    private async Task<bool> CheckSignature() {
    }
}

public struct Version {
    public uint Major, Minor, Patch;
}