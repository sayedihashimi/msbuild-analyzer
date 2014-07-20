namespace MsbuildAnalyzer.Common {
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Framework;
    using MsbuildAnalyzer.Common.Loggers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class DiagnosticBuilder : IBuildHelper {
        private ProjectCollection pc;
        private BuildManager buildManager;
        private BuildSubmission submission;
        private BuildResult buildResult;
        private ProjectInstance projInst;
        private string _projectFilepath;
        private string _logFilepath;
        private string[] _targets;

        public DiagnosticBuilder(string projectFilepath, string logFilepath,string []targets) {
            _projectFilepath = projectFilepath;
            _logFilepath = logFilepath;
            _targets = targets;
        }

        public void BuildAndAnalyze() {
            var globalProps = new Dictionary<string, string> {
                {"Configuration","Release"},
                {"DeployOnBuild","true"},
                {"PublishProfile","PSBuildTest"},
                {"Password","p3P3KLcwEFmvDyoMNlLhocPwzy4hr4heEwQzTQvYxm1B8sQirB9hbTYnfFMk"},
                {"VisualStudioVersion","12.0"}
            };
            pc = new ProjectCollection(globalProps);
            var diagLogger = new DiagnosticXmlLogger(this);
            diagLogger.LogFile = _logFilepath;           

            var proj = pc.LoadProject(_projectFilepath);
            projInst = proj.CreateProjectInstance();
            buildManager = new BuildManager();
            var buildParams = new BuildParameters();
            buildParams.Loggers = new ILogger[] { diagLogger };
            buildManager.BeginBuild(buildParams);

            var brd = new BuildRequestData(projInst, _targets, null, BuildRequestDataFlags.ReplaceExistingProjectInstance);
            submission = buildManager.PendBuildRequest(brd);
            buildResult = submission.Execute();

            buildManager.EndBuild();
        }

        public ProjectInstance GetProjectInstanceById(int projectInstanceId) {
            ProjectInstance result = null;
            
            // the ProjectInstance.DeepCopy() can throw InvalidOperationException sporadically
            int numRetries = 0;

            while (numRetries < 10) {
                numRetries++;
                try {
                    // TODO: how to know if we have the correct project here?
                    result = projInst.DeepCopy();
                    // if we get to this point no need to loop any more
                    break;
                }
                catch (InvalidOperationException) { }
            }

            return result;            
        }
    }

    public interface IBuildHelper {
        ProjectInstance GetProjectInstanceById(int projectInstanceId);
    }

}
