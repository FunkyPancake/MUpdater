using System.Security.Cryptography;

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
public abstract partial class Base {
    protected const string FwPackExtension = "mfw";
    protected const string FwFileName = "fw.hex";
    protected const string ManifestFileName = "manifest.xml";
    protected const string SignatureFileName = "signature.sig";
    protected const string EcuNameNodeName = "ecuName";
    protected const string VersionNodeName = "version";
    protected const string ReleaseDateNodeName = "releaseDate";
    protected const string HwCompatibilityNodeName = "hwCompatibility";

    protected async void EncryptFile(string filePath, string keyString) {
        var tempFileName = Path.GetTempFileName();
        //this is correct for keys generated with openssl, last 16 bytes are IV
        var key = Convert.FromBase64String(keyString)[..32];
        using (SymmetricAlgorithm cipher = Aes.Create())
        await using (var fileStream = File.OpenRead(filePath))
        await using (var tempFile = File.Create(tempFileName)) {
            cipher.Key = key;
            // aes.IV will be automatically populated with a secure random value
            var iv = cipher.IV;

            // Write a marker header so we can identify how to read this file in the future
            await tempFile.WriteAsync(FileHeader);

            await tempFile.WriteAsync(iv);

            await using (var cryptoStream =
                         new CryptoStream(tempFile, cipher.CreateEncryptor(), CryptoStreamMode.Write)) {
                await fileStream.CopyToAsync(cryptoStream);
            }
        }

        File.Delete(filePath);
        File.Move(tempFileName, filePath);
    }

    protected async Task<MemoryStream> DecryptFileToStream(string filePath, string keyString) {
        //this is correct for keys generated with openssl, last 16 bytes are IV
        var key = Convert.FromBase64String(keyString)[..32];

        using SymmetricAlgorithm cipher = Aes.Create();
        await using var fileStream = File.OpenRead(filePath);
        cipher.Key = key;
        var iv = new byte[cipher.BlockSize / 8];
        var headerBytes = new byte[FileHeader.Length];
        var remain = headerBytes.Length;

        while (remain != 0) {
            var read = await fileStream.ReadAsync(headerBytes.AsMemory(headerBytes.Length - remain, remain));

            if (read == 0) {
                throw new EndOfStreamException();
            }

            remain -= read;
        }

        if (!headerBytes.SequenceEqual(FileHeader)) {
            throw new InvalidOperationException();
        }

        remain = iv.Length;

        while (remain != 0) {
            var read = await fileStream.ReadAsync(iv.AsMemory(iv.Length - remain, remain));

            if (read == 0) {
                throw new EndOfStreamException();
            }

            remain -= read;
        }

        cipher.IV = iv;

        var mem = new MemoryStream();
        await using var cryptoStream = new CryptoStream(fileStream, cipher.CreateDecryptor(), CryptoStreamMode.Read);
        await cryptoStream.CopyToAsync(mem);
        return mem;
    }
}