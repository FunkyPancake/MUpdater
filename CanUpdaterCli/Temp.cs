namespace CanUpdaterCli; 



public class DbcReader {
    public DbcReader(string filePathDbc) {
    }

    public object GetFrameId(string ecuId, string caltx) {
        throw new NotImplementedException();
    }

    public string GetCalFrames(string ecuId) {
        throw new NotImplementedException();
    }
}

public class SoftwarePackage {
    public SoftwarePackage(string filePathSwPack) {
        throw new NotImplementedException();
    }

    public bool CheckCompatibility(object ecuData, object swVersion) {
        throw new NotImplementedException();
    }

    public void ProcessFile() {
        throw new NotImplementedException();
    }
}

public class CalibrationTp {

    public CalibrationTp(string getCalFrames) {
        throw new NotImplementedException();
    }

    public bool Connect() {
    }

    public object GetEcuIdent() {
        throw new NotImplementedException();
    }

    public (object hwVersion, object swVersion) GetSwVersion() {
        throw new NotImplementedException();
    }

    public void Program(SoftwarePackage swPackage) {
        throw new NotImplementedException();
    }

    public void Disconnect() {
        throw new NotImplementedException();
    }
}