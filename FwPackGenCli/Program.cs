// See https://aka.ms/new-console-template for more information

using FirmwarePack;
using Mono.Options;
using Serilog;
using Version = FirmwarePack.Version;

var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
var ecuName = string.Empty;
var swVersion = new Version();
var options = new OptionSet {
    {"e|ecu=", "", s => ecuName = s},
    {"s|sw=", "", s => {
            try {
                swVersion = new Version(s);
            }
            catch {
                logger.Error("Version string in incorrect format. It has to be 'Major.Minor.Patch' format.");
                throw new OptionException("Version string in incorrect format. It has to be 'Major.Minor.Patch' format.","sw");
            }
        }
    },
    {"h|help=","", s => { }}
};
List<string> extra;
try {
    extra = options.Parse(args);
}
catch (OptionException e) {
    // output some error message
    Console.Write("greet: ");
    Console.WriteLine(e.Message);
    Console.WriteLine("Try `greet --help' for more information.");
    return -1;
}


var writer = new FirmwarePackWriter(logger);
return await writer.Save("./", "./fw.hex", swVersion, ecuName,
    new List<Version> {new("1.2.3"),new ("2.0.1") }, "./key.rsa")
    ? 0
    : -1;
