namespace MsbuildAnalyzer.Common {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Build.Evaluation;

    public interface IAnalyzer {
        void Analyze(Project project);
    }
}
