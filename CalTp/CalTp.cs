namespace CalTp;

public class CalTp {

    public CalTp((CanFrame tx,CanFrame rx)) {
        throw new NotImplementedException();
    }

    public bool Connect() {
        return true;
    }

    public object GetEcuIdent() {
        throw new NotImplementedException();
    }

    public (object hwVersion, object swVersion) GetSwVersion() {
        throw new NotImplementedException();
    }

    public void Program(string swPackage) {
        throw new NotImplementedException();
    }

    public void Disconnect() {
        throw new NotImplementedException();
    }
}

public class CanFrame {
}