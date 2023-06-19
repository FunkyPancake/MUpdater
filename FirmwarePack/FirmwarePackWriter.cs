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

    public async Task<bool> Save(string outputDir, string hexPath, Version swVersion, string ecuName,
        List<Version> hwCompatibility,
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

        await using var zipStream = new FileStream(Path.Combine(outputDir, $"{ecuName}-{swVersion}.{FwPackExtension}"),
            FileMode.Create);
        using var zip = new ZipArchive(zipStream, ZipArchiveMode.Create);
        zip.CreateEntryFromFile(hexPath, FwFileName);

        var manifestEntry = zip.CreateEntry(ManifestFileName);
        var metadata = await GenerateMetadata(manifestEntry, swVersion, ecuName, hwCompatibility);

        var sigEntry = zip.CreateEntry(SignatureFileName);
        await GenerateSignature(sigEntry, metadata, FwFileName, privateKey);
        _logger.Information("Software pack for {0} generated successfully. Version {1}.", ecuName, swVersion);
        return true;
    }

    private static async Task GenerateSignature(ZipArchiveEntry signatureEntry, byte[] metadata,
        string hexEntry, SecureString key) {
        await using var writer = new StreamWriter(signatureEntry.Open());
        
        var hex = await File.ReadAllBytesAsync(hexEntry);
        var data = new byte[hex.Length + metadata.Length];
        hex.CopyTo(data,0);
        metadata.CopyTo(data,hex.Length);
        var rsa = RSA.Create();

        rsa.ImportFromPem(new NetworkCredential("", key).Password);

        var signData = rsa.SignData(data!, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        await writer.WriteLineAsync(Convert.ToBase64String(signData));
        await writer.FlushAsync();
    }

    private async Task<byte[]> GenerateMetadata(ZipArchiveEntry zipArchiveEntry, Version swVersion, string ecuName,
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
        return Encoding.UTF8.GetBytes(xml.OuterXml);
    }

    private static void AddStringAttribute(XmlDocument doc, XmlNode parent, string name, string value) {
        var element = doc.CreateElement(string.Empty, name, string.Empty);
        var textNode = doc.CreateTextNode(value);
        element.AppendChild(textNode);
        parent.AppendChild(element);
    }
}