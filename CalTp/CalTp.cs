namespace CalTp;

public class CalTp {

    public CalTp((int tx,int rx) tp) {
        throw new NotImplementedException();
    }

    public bool Connect() {
        return true;
    }

    public CommonTypes.EcuIdent GetEcuIdent() {
        throw new NotImplementedException();
    }

    public CommonTypes.Version GetSwVersion() {
        throw new NotImplementedException();
    }

    public void Program(IntelHex.Hex swPackage) {
        throw new NotImplementedException();
    }

    public void Disconnect() {
        throw new NotImplementedException();
    }
}

public struct EcuIdent {
    public string EcuName { get; }
    public Version hwVersion { get; }
}

public class CanFrame {
}