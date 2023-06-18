using System.IO.Compression;
using System.Security;
using System.Xml;
using Serilog;


namespace FirmwarePack;

public class FirmwarePackWriter : Base {
    private readonly ILogger _logger;

    public FirmwarePackWriter(ILogger logger) {
        _logger = logger;
    }

    public async Task Save(string outputDir, string hexPath, Version swVersion, string ecuName,
        List<Version> hwCompatibility,
        string privateKey) {
        // Guard.Against.FileNotFound(hexPath, nameof(hexPath));

        await using var zipStream = new FileStream(Path.Combine(outputDir, $"{ecuName}_{swVersion}.{FwPackExtension}"),
            FileMode.Create);
        using var zip = new ZipArchive(zipStream, ZipArchiveMode.Create);
        // zip.CreateEntryFromFile(hexPath, FwFileName);

        var manifestEntry = zip.CreateEntry(ManifestFileName);
        await GenerateMetadata(manifestEntry);

        // var sigEntry = zip.CreateEntry(SignatureFileName);
        // await GenerateSignature(sigEntry);
    }


    private async Task GenerateMetadata(ZipArchiveEntry zipArchiveEntry) {
        var xml = new XmlDocument();
        await using var writer = new StreamWriter(zipArchiveEntry.Open());
        // var xd =new XmlTextWriter(writer);
        xml.AppendChild(xml.CreateElement("item", "b", null));
        await writer.WriteLineAsync("abc");
        // await writer.WriteAsync(xml.OuterXml);
        // xml.Save(xd);
    }

    private async Task GenerateSignature(ZipArchiveEntry zipArchiveEntry) {
    }
}