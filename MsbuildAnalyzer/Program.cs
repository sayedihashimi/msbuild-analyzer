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

        // this method is not being called yet, this is just prototype. i'll refactor
        private static void BuildAndAnalyze() {
            // https://github.com/bdachev/CSTests/blob/4a3056f2a821fc47362ef33eab0d22249776f984/MSBuildTest/Program.cs#L37
            string projToBuild = @"C:\temp\msbuild\proj1.proj";

            var pc = new ProjectCollection();
            var diagLogger = new DiagnosticXmlLogger();
            pc.RegisterLogger(diagLogger);

            var proj = pc.LoadProject(projToBuild);
            var projInst = proj.CreateProjectInstance();
            var buildManager = new BuildManager();
            var buildParams = new BuildParameters();
            buildParams.Loggers = new ILogger[] { diagLogger };
            buildManager.BeginBuild(buildParams);

            var brd = new BuildRequestData(projInst, new string[] { "Demo" }, null, BuildRequestDataFlags.ReplaceExistingProjectInstance);
            var submission = buildManager.PendBuildRequest(brd);
            var buildResult = submission.Execute();

            buildManager.EndBuild();
        }

        static void PrintUsage() {
            StringBuilder usage = new StringBuilder();
            usage.AppendLine("usage: msbuidanalyzer.exe [project file]");
            usage.AppendLine();

            Console.WriteLine(usage.ToString());
        }
    }
}
