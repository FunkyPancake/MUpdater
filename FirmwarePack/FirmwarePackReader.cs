using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using CommonTypes;
using IntelHex;
using Serilog;

namespace FirmwarePack;

public class FirmwarePackReader : Base {
    private readonly ILogger _logger;
    private readonly List<CommonTypes.Version> _hwCompatibility = new();

    public FirmwarePackReader(ILogger logger) {
        _logger = logger;
    }

    public Hex Hex { get; private set; } = new Hex();
    public string TargetEcu { get; private set; } = string.Empty;
    public DateTime ReleaseDate { get; private set; }
    public CommonTypes.Version SwVersion { get; private set; }


    public async Task Load(string filePath) {
        using var decryptedStream = await DecryptFileToStream(filePath, AesKey);
        using var zip = new ZipArchive(decryptedStream, ZipArchiveMode.Read, false);
        using var manifestMemoryStream = new MemoryStream();
        using var hexMemoryStream = new MemoryStream();

        var signature = zip.GetEntry(SignatureFileName)!.Open();

        await zip.GetEntry(ManifestFileName)!.Open().CopyToAsync(manifestMemoryStream);
        await zip.GetEntry(FwFileName)!.Open().CopyToAsync(hexMemoryStream);

        //check signature
        if (await CheckSignature(signature, manifestMemoryStream, hexMemoryStream) == false) {
            _logger.Error("Signature incorrect. Abort");
            throw new ApplicationException("");
        }

        //
        // //read metadata
        GetMetadata(manifestMemoryStream);
        await GetHex(hexMemoryStream);
    }

    private void GetMetadata(MemoryStream manifest) {
        var str = Encoding.UTF8.GetString(manifest.GetBuffer()[..(int) manifest.Length]);
        var xml = XElement.Parse(str);
        TargetEcu = xml.Element(EcuNameNodeName)!.Value;
        SwVersion = new CommonTypes.Version(xml.Element(VersionNodeName)!.Value);
        ReleaseDate = DateTime.Parse(xml.Element(ReleaseDateNodeName)!.Value);
        foreach (var element in xml.Element(HwCompatibilityNodeName)!.Elements()) {
            _hwCompatibility.Add(new CommonTypes.Version(element.Value));
        }
    }

    private async Task<bool> CheckSignature(Stream sigEntry, MemoryStream manifest, MemoryStream hex) {
        using var rsa = RSA.Create();

        rsa.ImportFromPem(RsaKey);
        string sig;
        using (var sigStream = new StreamReader(sigEntry)) {
            sig = await sigStream.ReadToEndAsync();
        }

        _logger.Debug("Read signature: {0}", sig);
        var dataBuffer = new byte[manifest.Length + hex.Length];
        manifest.GetBuffer()[..(int) manifest.Length].CopyTo(dataBuffer, 0);
        hex.GetBuffer()[..(int) hex.Length].CopyTo(dataBuffer, manifest.Length);

        return rsa.VerifyData(dataBuffer, Convert.FromBase64String(sig), HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
    }

    private async Task GetHex(Stream memoryStream) {
        Hex = await HexIo.ReadAsync(memoryStream);
    }


    public bool CheckCompatibility(EcuIdent ecuIdent,CommonTypes.Version version) {
        return _hwCompatibility.Contains(ecuIdent.HwVersion) && ecuIdent.EcuName.Equals(TargetEcu);
    }
}