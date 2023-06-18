using System.IO.Compression;
using System.Security;
using System.Security.Cryptography;
using IntelHex;
using Serilog;

namespace FirmwarePack;

/* fwPack structure
 * fwPack.msw
 *  -> Manifest.xml
 *      -> Target Ecu
 *      -> Compatible Hardware Versions
 *      -> Release date
 *      -> Sw Version
 *  -> Cert file
 *  -> Hex file
*/
public class FirmwarePack {
    private readonly ILogger _logger;

    public FirmwarePack(ILogger logger) {
        _logger = logger;
    }

    public async Task WritePack(string outputDir,string hexPath, Version swVersion, string ecuName, List<Version> hwCompatibility,
        SecureString privateKey) {
        Guard
    }

    public async Task ReadPack(string filePath) {
        //check file exists and extension matches

        //unpack
        var stream = await DecryptPackage(filePath);
        using var zip = new ZipArchive(stream, ZipArchiveMode.Read, false);
        var signature = zip.GetEntry("signature.sig");
        var manifest = zip.GetEntry("manifest.xml");
        var hex = zip.GetEntry("sw.hex");
        if (zip.Entries.Count != 3 || hex is null || manifest is null || signature is null) {
            throw new ApplicationException("");
        }

        //calc signature
        if (await CheckSignature(signature, manifest, hex) == false) {
            throw new ApplicationException("");
        }

        //read metadata
        await GetMetadata(manifest);
        await GetHex(hex);
    }

    private async Task<bool> CheckSignature(ZipArchiveEntry sigEntry, ZipArchiveEntry manifest, ZipArchiveEntry hex) {
        var sig = await new StreamReader(sigEntry.Open()).ReadToEndAsync();
        var memoryStream = new MemoryStream();
        await hex.Open().CopyToAsync(memoryStream);
        await manifest.Open().CopyToAsync(memoryStream);
        using var rsa = RSA.Create();
        return rsa.VerifyData(memoryStream.GetBuffer(), Convert.FromBase64String(sig), HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
    }

    private async Task GetHex(ZipArchiveEntry zipArchiveEntry) {
        Hex = await HexIo.ReadAsync(zipArchiveEntry.Open());
    }

    private async Task GetMetadata(ZipArchiveEntry zipArchiveEntry) {
    }

    public Hex Hex { get; private set; } = new Hex();
    public string TargetEcu { get; private set; } = string.Empty;
    public DateTime ReleaseDate { get; private set; }
    public Version SwVersion { get; } = new Version();

    public bool CheckEcuCompatibility() {
        return true;
    }


    private async Task<Stream> DecryptPackage(string fileName) {
        var stream = new FileStream(fileName, FileMode.Open);
        using var aes = Aes.Create();
        byte[] key;
        stream.Read();
        using var cryptoStream = new CryptoStream(stream, aes.CreateDecryptor(key), CryptoStreamMode.Read)
        using var streamReader = new StreamReader(cryptoStream);
        return streamReader.BaseStream;
    }

    private async Task<bool> CheckSignature() {
    }
}

public struct Version {
    public uint Major, Minor, Patch;
}