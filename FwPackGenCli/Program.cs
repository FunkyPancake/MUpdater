// See https://aka.ms/new-console-template for more information

using FirmwarePack;
using Serilog;
using Version = FirmwarePack.Version;

var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
var ecuName = "CanGps";
var writer = new FirmwarePack.FirmwarePackWriter(logger);
return await writer.Save("./", "./fw.hex", new Version("1.0.0"), ecuName,
    new List<Version> {new("1.2.3"),new ("2.0.1") }, "./key.rsa")
    ? 0
    : -1;
