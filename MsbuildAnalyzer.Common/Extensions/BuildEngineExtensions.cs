namespace MsbuildAnalyzer.Common.Extensions {
    using Microsoft.Build.Execution;
    using Microsoft.Build.Framework;
    using System;
    using System.IO;
    using System.Reflection;

    public static class BuildEngineExtensions {
        const BindingFlags bindingFlags = BindingFlags.NonPublic |
            BindingFlags.FlattenHierarchy |
            BindingFlags.Instance |
            BindingFlags.Public;

        public static ProjectInstance GetProjectInstance(this IBuildEngine buildEngine) {
            var buildEngineType = buildEngine.GetType();
            var callbackField = buildEngineType.GetField("targetBuilderCallback", bindingFlags);
            if (callbackField == null) {
                throw new Exception("Could not extract targetBuilderCallback from " + buildEngineType.FullName);
            }
            var callback = callbackField.GetValue(buildEngine);
            var targetCallbackType = callback.GetType();
            var instanceField = targetCallbackType.GetField("projectInstance", bindingFlags);
            if (instanceField == null) {
                throw new Exception("Could not extract projectInstance from " + targetCallbackType.FullName);
            }
            return (ProjectInstance)instanceField.GetValue(callback);
        }

        public static string GetProjectPath(this IBuildEngine buildEngine) {
            var projectFilePath = buildEngine.ProjectFileOfTaskNode;
            if (File.Exists(projectFilePath)) {
                return projectFilePath;
            }
            return buildEngine.GetProjectInstance().FullPath;
        }
    }

}
