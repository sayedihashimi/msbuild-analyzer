namespace MsbuildAnalyzer.Common {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;
    using QuickGraph;
    using MsbuildAnalyzer.Common.Extensions;

    // TODO: figure out which targets have no dependencies (i.e. not ever called, except maybe DefaultTarget)

    public class TargetAnalyzer : IAnalyzer {
        public TargetAnalyzer() {
            this.TargetReport = "Target analysis has not been called yet.";
        }

        private Project Project { get; set; }
        private string TargetReport { get; set; }
        private AdjacencyGraph<ProjectTargetInstance, Edge<ProjectTargetInstance>> TargetGraph { get; set; }
        private bool ReportGenerated { get; set; }

        public void Analyze(string projectFilePath) {
            if (string.IsNullOrEmpty(projectFilePath)) { throw new ArgumentNullException("projectFilePath"); }
            if (!File.Exists(projectFilePath)) {
                string message = string.Format("Project file not found at [{0}]", projectFilePath);
                throw new FileNotFoundException(message);
            }

            ProjectCollection pc = new ProjectCollection();
            Project project = pc.LoadProject(projectFilePath);
            this.Analyze(project);
        }

        public void Analyze(Project project) {
            if (project == null) { throw new ArgumentNullException("project"); }

            this.Project = project;
            this.TargetGraph = this.BuildTargetGraph(project);
        }

        protected internal AdjacencyGraph<ProjectTargetInstance, Edge<ProjectTargetInstance>> BuildTargetGraph(Project project) {
            AdjacencyGraph<ProjectTargetInstance, Edge<ProjectTargetInstance>> graph =
                new AdjacencyGraph<ProjectTargetInstance, Edge<ProjectTargetInstance>>();

            foreach (string key in project.Targets.Keys) {
                var target = project.Targets[key];
                if (!graph.ContainsVertex(target)) {
                    graph.AddVertex(target);
                }

                // TODO: DependsOnTargets needs to be evalueated
                string depTargetsString = target.DependsOnTargets != null ? target.DependsOnTargets : string.Empty;

                List<string> dependentTargets = target.GetDependsOnTargetsAsList(project).ToList();

                string depTargetsStringEvaluated = project.ExpandString(depTargetsString);

                dependentTargets.ToList().ForEach(depTarget => {
                    var dt = project.Targets[depTarget];
                    if (dt != null) {
                        if (!graph.ContainsVertex(dt)) {
                            graph.AddVertex(dt);
                        }

                        Edge<ProjectTargetInstance> e = new Edge<ProjectTargetInstance>(target, dt);
                        graph.AddEdge(e);
                    }
                });
            }

            return graph;
        }

        public string GetReport() {
            if (this.TargetGraph != null && !this.ReportGenerated) {

                var verticesOrderedByNumDepends = from ProjectTargetInstance v in this.TargetGraph.Vertices
                                                  orderby v.GetNumDependentTargets(this.Project)
                                                  select v;
                // create the report here
                StringBuilder sb = new StringBuilder();
                foreach (var v in verticesOrderedByNumDepends) {
                    sb.Append(v.Name);
                    sb.AppendFormat(@"{0} ({1}) -----------------------------{2}", v.Name, v.GetNumDependentTargets(this.Project), Environment.NewLine);
                    foreach (var e in this.TargetGraph.OutEdges(v)) {
                        sb.AppendFormat(". {0}{1}", e.Target.Name, Environment.NewLine);
                    }
                    sb.Append(Environment.NewLine);
                }
                sb.Append(Environment.NewLine);

                this.TargetReport = sb.ToString();
                this.ReportGenerated = true;
            }

            return this.TargetReport;
        }
    }
}
