// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Security;
using FirmwarePack;
using Mono.Options;
using Serilog;

var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
var ecuName = string.Empty;
var swVersion = new CommonTypes.Version();
var key = new SecureString();
var writer = new FirmwarePackWriter(logger);
/*TODO
    -> write help and option comments
    -> add rest of the checks for parameters
    -> exception handling with logger and returns
*/

var options = new OptionSet {
    {"e|ecu=", "", s => ecuName = s}, {
        "s|sw=", "", s => {
            try {
                swVersion = new CommonTypes.Version(s);
            }
            catch {
                logger.Error("Version string in incorrect format. It has to be 'Major.Minor.Patch' format.");
                throw new OptionException(
                    "Version string in incorrect format. It has to be 'Major.Minor.Patch' format.", "sw");
            }
        }
    }, {
        "k|key=", "", s => {
            key = new NetworkCredential("", s).SecurePassword;
            key.MakeReadOnly();
        }
    }, {
        "p|pem=", "", s => {
            if (!File.Exists(s)) return;
            var streamReader = new StreamReader(s);
            while (streamReader.Peek() >= 0) {
                key.AppendChar((char) streamReader.Read());
            }

            key.MakeReadOnly();
        }
    },
    {"h|help=", "", s => { }}
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


return await writer.Save("./", "./fw.hex", swVersion, ecuName,
    new List<CommonTypes.Version> {new("1.2.3"), new("2.0.1")}, key)
    ? 0
    : -1;