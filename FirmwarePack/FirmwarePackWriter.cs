using System.IO.Compression;
using System.Security.Cryptography;
using System.Xml;
using Serilog;


namespace FirmwarePack;

public class FirmwarePackWriter : Base {
    private readonly ILogger _logger;

    public FirmwarePackWriter(ILogger logger) {
        _logger = logger;
    }

    public async Task<bool> Save(string outputDir, string hexPath, Version swVersion, string ecuName,
        List<Version> hwCompatibility,
        string privateKey) {
        // Guard.Against.FileNotFound(hexPath, nameof(hexPath));
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
        
        await using var zipStream = new FileStream(Path.Combine(outputDir, $"{ecuName}-{swVersion}.{FwPackExtension}"),
            FileMode.Create);
        using var zip = new ZipArchive(zipStream, ZipArchiveMode.Create);
        var hexEntry = zip.CreateEntryFromFile(hexPath, FwFileName);

        var manifestEntry = zip.CreateEntry(ManifestFileName);
        await GenerateMetadata(manifestEntry, swVersion, ecuName, hwCompatibility);

        var sigEntry = zip.CreateEntry(SignatureFileName);
        await GenerateSignature(sigEntry, manifestEntry, FwFileName, privateKey);
        _logger.Information("Software pack for {0} generated successfully. Version {1}.",ecuName,swVersion);
        return true;
    }

    private static async Task GenerateSignature(ZipArchiveEntry signatureEntry, ZipArchiveEntry manifestEntry,
        string hexEntry, string key) {
        await using var writer = new StreamWriter(signatureEntry.Open());
        var data = await File.ReadAllBytesAsync(hexEntry);
        using var rsa = RSA.Create();
        var signData = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        await writer.WriteLineAsync(Convert.ToBase64String(signData));
        await writer.FlushAsync();
    }

    private async Task GenerateMetadata(ZipArchiveEntry zipArchiveEntry, Version swVersion, string ecuName,
        List<Version> hwCompatibility) {
        var xml = new XmlDocument();
        await using var writer = new StreamWriter(zipArchiveEntry.Open());
        var root = xml.DocumentElement;
        var xmlDeclaration = xml.CreateXmlDeclaration("1.0", "UTF8", null);
        xml.InsertBefore(xmlDeclaration, root);
        var metadata = xml.CreateElement(string.Empty, "manifest", string.Empty);
        xml.AppendChild(metadata);

        AddStringAttribute(xml, metadata, "ecuName", ecuName);
        AddStringAttribute(xml, metadata, "version", swVersion.ToString());
        AddStringAttribute(xml, metadata, "releaseDate", DateTime.Now.ToShortDateString());

        var hwElement = xml.CreateElement(string.Empty, "hwCompatibility", string.Empty);
        foreach (var version in hwCompatibility) {
            AddStringAttribute(xml, hwElement, "version", version.ToString());
        }

        metadata.AppendChild(hwElement);
        await writer.WriteAsync(xml.OuterXml);
        await writer.FlushAsync();
    }

    private static void AddStringAttribute(XmlDocument doc, XmlNode parent, string name, string value) {
        var element = doc.CreateElement(string.Empty, name, string.Empty);
        var textNode = doc.CreateTextNode(value);
        element.AppendChild(textNode);
        parent.AppendChild(element);
    }
}