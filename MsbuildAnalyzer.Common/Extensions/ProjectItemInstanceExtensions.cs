namespace MsbuildAnalyzer.Common.Extensions {
    using Microsoft.Build.Execution;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class ProjectItemInstanceExtensions {
        public static string SafeGetMetadataValue(this ProjectItemInstance item, string name) {
            string result = null;
            try {
                result = item.GetMetadataValue(name);
            }
            catch (Exception) { }
            return result;
        }
    }
}
