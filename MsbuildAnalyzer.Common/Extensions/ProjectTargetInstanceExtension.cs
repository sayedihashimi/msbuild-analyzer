namespace MsbuildAnalyzer.Common.Extensions {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;

    public static class ProjectTargetInstanceExtension {
        public static int GetNumDependentTargets(this ProjectTargetInstance target, Project project) {
            if (target == null) { throw new ArgumentNullException("target"); }
            if (project == null) { throw new ArgumentNullException("project"); }

            string depndsOnTargets = target.DependsOnTargets;
            IList<string> targetsList = target.GetDependsOnTargetsAsList(project);

            return targetsList.Count();
        }

        public static IList<string> GetDependsOnTargetsAsList(this ProjectTargetInstance target,Project project) {
            if (target == null) { throw new ArgumentNullException("target"); }
            if (project == null) { throw new ArgumentNullException("project"); }

            List<string> targets = new List<string>();
            string depTargets = target.DependsOnTargets != null ? target.DependsOnTargets : string.Empty;
            string depTargetsEvaluated = project.ExpandString(depTargets);

            string[] dtArray = depTargetsEvaluated.Split(';');
            dtArray.ToList().ForEach(t => {
                if (!string.IsNullOrWhiteSpace(t)) {
                    string tName = t.Trim();
                    if (!string.IsNullOrWhiteSpace(tName) &&
                        string.Compare(";", tName, StringComparison.InvariantCultureIgnoreCase) != 0) {
                        targets.Add(tName);
                    }
                }
            });

            int numTarges = targets != null ? targets.Count() : 0;
            string tempDebug = null;
            if (numTarges >= 1) {
                tempDebug = targets[0];
            }

            return targets;
        }

    }
}
