namespace MsbuildAnalyzer {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MsbuildAnalyzer.Common;

    public class Program {
        static void Main(string[] args) {
            if (args != null && args.Length >= 1) {
                CommandLineOptionsBuilder builder = new CommandLineOptionsBuilder();
                CommandLineOptions options = builder.GetOptions(args);

                if (options.AreOptionsValid) {
                    TargetAnalyzer analyzer = new TargetAnalyzer();
                    analyzer.Analyze(options.ProjectFilePath);

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

        static void PrintUsage() {
            StringBuilder usage = new StringBuilder();
            usage.AppendLine("usage: msbuidanalyzer.exe [project file]");
            usage.AppendLine();

            Console.WriteLine(usage.ToString());
        }
    }
}
