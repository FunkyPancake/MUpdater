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

        var zipStream = new FileStream(Path.Combine(outputDir, $"{ecuName}_{swVersion}.{FwPackExtension}"),
            FileMode.CreateNew);
        var zip = new ZipArchive(zipStream, ZipArchiveMode.Create);
        zip.CreateEntryFromFile(hexPath, FwFileName);
        var manifestEntry = zip.CreateEntry(ManifestFileName);
        var sigEntry = zip.CreateEntry(SignatureFileName);
        await GenerateMetadata(manifestEntry);
        await GenerateSignature(sigEntry);
        zipStream.Close();
    }


    private async Task GenerateMetadata(ZipArchiveEntry zipArchiveEntry) {
        var xml = new XmlDocument();

        xml.Save(zipArchiveEntry.Open());
    }

    private async Task GenerateSignature(ZipArchiveEntry zipArchiveEntry) {
    }
}