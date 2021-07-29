using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MarkopTest.FunctionalTest;
using MarkopTest.IntegrationTest;
using MarkopTest.LoadTest;

namespace MarkopTest.Handler
{
    internal class TestHandlerHelper
    {
        private static TestHandler GetTestHandler()
        {
            var functionalFrame = new StackTrace().GetFrames().LastOrDefault(frame =>
                frame?.GetMethod()?.DeclaringType?.Namespace ==
                typeof(FunctionalTestFactory<>).Namespace);
            
            if (functionalFrame != null)
                return null;

            var stackFrame = new StackTrace().GetFrames().LastOrDefault(frame =>
                                 frame?.GetMethod()?.DeclaringType?.BaseType?.BaseType?.Namespace ==
                                 typeof(LoadTestFactory<>).Namespace) ??
                             new StackTrace().GetFrames().LastOrDefault(frame =>
                                 frame?.GetMethod()?.DeclaringType?.BaseType?.BaseType?.Namespace ==
                                 typeof(IntegrationTestFactory<>).Namespace);


            var methodBase = stackFrame?.GetMethod();

            if (methodBase == null)
                return null;

            return (TestHandler) Attribute.GetCustomAttribute(methodBase, typeof(TestHandler));
        }

        internal static async Task BeforeTest(HttpClient httpClient)
        {
            var handler = GetTestHandler();
            if (handler != null)
                await handler.Before(httpClient);
        }

        internal static async Task AfterTest(HttpClient httpClient)
        {
            var handler = GetTestHandler();
            if (handler != null)
                await handler.After(httpClient);
        }
    }
}