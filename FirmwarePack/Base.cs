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
public abstract class Base {
    protected const string FwPackExtension = "mfw";
    protected const string FwFileName = "fw.hex";
    protected const string ManifestFileName = "manifest.xml";
    protected const string SignatureFileName = "signature.sig";

    protected static void EncryptFile(string filePath, string keyString) {
        var tempFileName = Path.GetTempFileName();
        //TODO fix key lenght issue
        var key = Convert.FromBase64String(keyString)[..32];
        using (SymmetricAlgorithm cipher = Aes.Create())
        using (var fileStream = File.OpenRead(filePath))
        using (var tempFile = File.Create(tempFileName)) {
            cipher.Key = key;
            // aes.IV will be automatically populated with a secure random value
            var iv = cipher.IV;

            // Write a marker header so we can identify how to read this file in the future
            tempFile.Write(CryptoData.FileHeader);

            tempFile.Write(iv, 0, iv.Length);

            using (var cryptoStream =
                   new CryptoStream(tempFile, cipher.CreateEncryptor(), CryptoStreamMode.Write)) {
                fileStream.CopyTo(cryptoStream);
            }
        }

        File.Delete(filePath);
        File.Move(tempFileName, filePath);
    }

    protected static void DecryptFile(string filePath, string keyString) {
        var tempFileName = Path.GetTempFileName();
        //TODO fix key lenght issue
        var key = Convert.FromBase64String(keyString)[..32];

        using (SymmetricAlgorithm cipher = Aes.Create())
        using (var fileStream = File.OpenRead(filePath))
        using (var tempFile = File.Create(tempFileName)) {
            cipher.Key = key;
            var iv = new byte[cipher.BlockSize / 8];
            var headerBytes = new byte[CryptoData.FileHeader.Length];
            var remain = headerBytes.Length;

            while (remain != 0) {
                var read = fileStream.Read(headerBytes, headerBytes.Length - remain, remain);

                if (read == 0) {
                    throw new EndOfStreamException();
                }

                remain -= read;
            }

            if (!headerBytes.SequenceEqual(CryptoData.FileHeader)) {
                throw new InvalidOperationException();
            }

            remain = iv.Length;

            while (remain != 0) {
                var read = fileStream.Read(iv, iv.Length - remain, remain);

                if (read == 0) {
                    throw new EndOfStreamException();
                }

                remain -= read;
            }

            cipher.IV = iv;

            using (var cryptoStream =
                   new CryptoStream(tempFile, cipher.CreateDecryptor(), CryptoStreamMode.Write)) {
                fileStream.CopyTo(cryptoStream);
            }
        }

        File.Delete(filePath);
        File.Move(tempFileName, filePath);
    }
}