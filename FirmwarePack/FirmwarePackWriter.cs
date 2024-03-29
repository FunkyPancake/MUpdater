using System.IO.Compression;
using System.Net;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Serilog;


namespace FirmwarePack;

public class FirmwarePackWriter : Base {
    private readonly ILogger _logger;

    public FirmwarePackWriter(ILogger logger) {
        _logger = logger;
    }

    public async Task<bool> Save(string outputDir, string hexPath, CommonTypes.Version swVersion, string ecuName,
        List<CommonTypes.Version> hwCompatibility,
        SecureString privateKey) {
        if (!File.Exists(hexPath)) {
            _logger.Error("Path to Intel-hex file is incorrect.");
            return false;
        }

        if (hwCompatibility.Count == 0) {
            _logger.Error("Software has to be compatible with at least one hw version");
            return false;
        }

        if (ecuName == string.Empty) {
            _logger.Error("Empty ecu name.");
            return false;
        }

        if (privateKey.Length == 0) {
            _logger.Error("Empty private key.");
            return false;
        }

        var outFileName = Path.Combine(outputDir, $"{ecuName}-{swVersion}.{FwPackExtension}");
        await using (var zipStream = new FileStream(outFileName, FileMode.Create)) {
            using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create)) {
                zip.CreateEntryFromFile(hexPath, FwFileName);

                var manifestEntry = zip.CreateEntry(ManifestFileName);
                var metadata = await GenerateMetadata(manifestEntry, swVersion, ecuName, hwCompatibility);

                var sigEntry = zip.CreateEntry(SignatureFileName);
                await GenerateSignature(sigEntry, metadata, FwFileName, privateKey);
                await zipStream.FlushAsync();
            }
        }

        EncryptFile(outFileName, AesKey);
        _logger.Information("Software pack for {0} generated successfully. Version {1}.", ecuName, swVersion);
        return true;
    }

    private async Task GenerateSignature(ZipArchiveEntry signatureEntry, byte[] metadata,
        string hexEntry, SecureString key) {
        await using var writer = new StreamWriter(signatureEntry.Open());

        var hex = await File.ReadAllBytesAsync(hexEntry);
        var data = new byte[hex.Length + metadata.Length];
        metadata.CopyTo(data, 0);
        hex.CopyTo(data, metadata.Length);
        var rsa = RSA.Create();

        rsa.ImportFromPem(new NetworkCredential("", key).Password);

        var signData = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var signature = Convert.ToBase64String(signData);
        await writer.WriteLineAsync(signature);
        await writer.FlushAsync();
        _logger.Debug("Signature: {0}.", signature);
    }

    private  async Task<byte[]> GenerateMetadata(ZipArchiveEntry zipArchiveEntry, CommonTypes.Version swVersion, string ecuName,
        List<CommonTypes.Version> hwCompatibility) {
        var xml = new XmlDocument();
        await using var writer = new StreamWriter(zipArchiveEntry.Open());
        var root = xml.DocumentElement;
        var xmlDeclaration = xml.CreateXmlDeclaration("1.0", "UTF8", null);
        xml.InsertBefore(xmlDeclaration, root);
        var metadata = xml.CreateElement(string.Empty, "manifest", string.Empty);
        xml.AppendChild(metadata);
        _logger.Debug("");

        AddStringAttribute(xml, metadata, EcuNameNodeName, ecuName);

        AddStringAttribute(xml, metadata, VersionNodeName, swVersion.ToString());


        AddStringAttribute(xml, metadata, ReleaseDateNodeName, DateTime.Now.ToShortDateString());

        var hwElement = xml.CreateElement(string.Empty, HwCompatibilityNodeName, string.Empty);
        foreach (var version in hwCompatibility) {
            AddStringAttribute(xml, hwElement, VersionNodeName, version.ToString());
        }

        metadata.AppendChild(hwElement);
        await writer.WriteAsync(xml.OuterXml);
        await writer.FlushAsync();
        return Encoding.UTF8.GetBytes(xml.OuterXml);
    }

    private static void AddStringAttribute(XmlDocument doc, XmlNode parent, string name, string value) {
        var element = doc.CreateElement(string.Empty, name, string.Empty);
        var textNode = doc.CreateTextNode(value);
        element.AppendChild(textNode);
        parent.AppendChild(element);
    }
}