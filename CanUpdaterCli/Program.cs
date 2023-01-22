// See https://aka.ms/new-console-template for more information

using CanUpdater;
using Serilog;

Console.WriteLine("Hello, World!");
new Mono.Options.OptionSet().Add("",v=>{});
var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

var x = new Class1(logger);
x.Connect();
x.Open();
x.Close();