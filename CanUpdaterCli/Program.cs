// See https://aka.ms/new-console-template for more information

using CalTp;
using Mono.Options;
using Serilog;

bool CheckOptions(List<string> list) {
    throw new NotImplementedException();
}

var filePathDbc = string.Empty;
var filePathSwPack = string.Empty;
var ecuId = string.Empty;
var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
var options = new OptionSet {
    {"d|dbc=", "", s => filePathDbc = s},
    {"s|sw=", "", s => filePathSwPack = s},
    {"h|help=", "", s => { }}
};
List<string> extra;
try {
    // parse the command line
    extra = options.Parse(args);
}
catch (OptionException e) {
    // output some error message
    Console.Write("greet: ");
    Console.WriteLine(e.Message);
    Console.WriteLine("Try `greet --help' for more information.");
    return;
}

if (!CheckOptions(extra)) {
    return;
}

var dbcReader = new DbcReader.DbcReader(filePathDbc);
var swPackage = new FirmwarePack.FirmwarePackReader(logger);
await swPackage.Load(filePathSwPack);
logger.Information("data from the package:{0},{1},{2}", swPackage.SwVersion, swPackage.TargetEcu,
    swPackage.ReleaseDate);

var cal = new CalibrationProtocol(logger,dbcReader.GetCalFrames(swPackage.TargetEcu));
if (await cal.Connect() != CmdStatus.Ok) {
    return;
}

var ecuData = await cal.GetEcuIdent();
var swVersion = await cal.GetSwVersion();
if (!swPackage.CheckCompatibility(ecuData, swVersion)) {
    await cal.Disconnect();
    return;
}

await cal.Program(swPackage.Hex);
swVersion = await cal.GetSwVersion();
await cal.Disconnect();
if (swPackage.SwVersion == swVersion) {
    logger.Information("Software updated successfully to version {0}", swVersion);
}
else {
    logger.Error("Software update failed.");
}