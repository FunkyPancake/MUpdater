﻿// See https://aka.ms/new-console-template for more information

using FirmwarePack;
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
    {"h|help=","", s => { }}
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

// if (!CheckOptions(extra)) {
//     return;
// }
var dbcReader = new DbcReader.DbcReader(filePathDbc);
var swPackage = new FirmwarePack.FirmwarePackReader(logger);
await swPackage.Load(filePathSwPack);
logger.Information("data from the package:{0},{1},{2}", swPackage.SwVersion,swPackage.TargetEcu,swPackage.ReleaseDate);

var cal = new CalTp.CalTp(dbcReader.GetCalFrames(swPackage.TargetEcu));
if (!cal.Connect()) {
    return;
}

var ecuData = cal.GetEcuIdent();
var swVersion = cal.GetSwVersion();
if (!swPackage.CheckCompatibility(ecuData, swVersion)) {
    cal.Disconnect();
    return;
}

cal.Program(swPackage.Hex);
cal.GetSwVersion();
cal.Disconnect();