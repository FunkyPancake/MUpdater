// See https://aka.ms/new-console-template for more information

using FirmwarePack;
using Serilog;
using Version = FirmwarePack.Version;

var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
var ecuName = "CanGps";
var writer = new FirmwarePack.FirmwarePackWriter(logger);
await writer.Save("./", "./temp.hex", new Version() {Major = 1, Minor = 2, Patch = 3}, ecuName,new List<Version>(){},"./key.rsa");
logger.Information("test");