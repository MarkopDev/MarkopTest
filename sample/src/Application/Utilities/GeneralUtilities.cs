using System;
using System.Linq;

namespace Application.Utilities
{
    public static class GeneralUtilities
    {
        public static bool IsUnitTestRunning => AppDomain.CurrentDomain.GetAssemblies().Any(a =>
            a.FullName != null && a.FullName.ToLowerInvariant().StartsWith("xunit.execution.dotnet"));
    }
}