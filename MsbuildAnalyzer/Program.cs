namespace MsbuildAnalyzer {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MsbuildAnalyzer.Common;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;
    using MsbuildAnalyzer.Common.Loggers;
    using Microsoft.Build.Framework;

    public class Program {
        static void Main(string[] args) {
            if (args != null && args.Length >= 2) {
                CommandLineOptionsBuilder builder = new CommandLineOptionsBuilder();
                CommandLineOptions options = builder.GetOptions(args);

                if (options.AreOptionsValid) {
                    TargetAnalyzer analyzer = new TargetAnalyzer();
                    analyzer.Analyze(options.ProjectFilePath);

                    var diagBuilder = new DiagnosticBuilder(options.ProjectFilePath, options.LogfilePath, new string[] { "Build" });
                    diagBuilder.BuildAndAnalyze();

                    Console.WriteLine(analyzer.GetReport());
                }
                else {
                    Console.WriteLine(options.ValidationErrorMessage);
                    PrintUsage();
                }
            }
            else {
                PrintUsage();
            }
        }

        // this method is not being called yet, this is just prototype. i'll refactor
        private static void BuildAndAnalyze() {
            new DiagnosticBuilder(@"C:\temp\msbuild\WebApplication1\WebApplication1.csproj", @"c:\temp\msbuild\log2.xml", new string[] { "Build" }).BuildAndAnalyze();            
        }

        static void PrintUsage() {
            StringBuilder usage = new StringBuilder();
            usage.AppendLine("usage: msbuidanalyzer.exe [project file] [logfile path]");
            usage.AppendLine();

            Console.WriteLine(usage.ToString());
        }
    }
}
