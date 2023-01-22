using System.Text;
using Peak.Can.Basic;
using Serilog;

namespace CanUpdater;

public class Class1
{
    private readonly ILogger _logger;
    private const string DeviceType = $"{PCANBasic.LOOKUP_DEVICE_TYPE}=PCAN_USB";
    private ushort _handle;
    private UInt16 English = 0x09;

    public Class1(ILogger logger)
    {
        _logger = logger;
        CheckForLibrary();
    }

    public bool Connect()
    {
        var status = PCANBasic.LookUpChannel(DeviceType, out var handle);
        if (status != TPCANStatus.PCAN_ERROR_OK)
        {
            _logger.Error("Hardware not detected");
            return false;
        }

        _handle = handle;
        return true;
    }

    private void LogPeakcanError(TPCANStatus status)
    {
        var stringBuffer = new StringBuilder();
        PCANBasic.GetErrorText(status, English, stringBuffer);
        _logger.Error("Error message : {Message}", stringBuffer.ToString());
    }

    public bool Open()
    {
        AssertHandleValid();
        var status = PCANBasic.Initialize(_handle, TPCANBaudrate.PCAN_BAUD_1M);
        if (status != TPCANStatus.PCAN_ERROR_OK)
        {
            LogPeakcanError(status);
            return false;
        }

        _logger.Information("Opened channel {Channel}, baudrate {Baudrate}", _handle,
            TPCANBaudrate.PCAN_BAUD_1M.ToString());
        return true;
    }


    private void CheckForLibrary()
    {
        // Check for dll file
        try
        {
            PCANBasic.Uninitialize(PCANBasic.PCAN_NONEBUS);
        }
        catch (DllNotFoundException)
        {
            _logger.Fatal("Unable to find the library: PCANBasic.dll");
            throw;
        }
    }

    private void AssertHandleValid()
    {
        if (_handle != 0)
        {
            return;
        }

        const string msg = "Channel handle value invalid";
        _logger.Fatal(msg);
        throw new InvalidDataException(msg);
    }

    public void Close()
    {
        AssertHandleValid();
        _logger.Information("Close channel");
        PCANBasic.Uninitialize(_handle);
    }
}