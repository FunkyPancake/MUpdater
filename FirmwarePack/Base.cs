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
    

}