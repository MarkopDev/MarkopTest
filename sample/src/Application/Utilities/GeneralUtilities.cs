using System;
using System.Linq;

namespace Application.Utilities
{
    public static class GeneralUtilities
    {
        public static bool IsTestRunning => AppDomain.CurrentDomain.GetAssemblies().Any(a =>
            a.FullName != null && a.FullName.ToLowerInvariant().StartsWith("xunit.execution.dotnet"));
    }
}