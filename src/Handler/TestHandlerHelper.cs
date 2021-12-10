using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MarkopTest.FunctionalTest;

namespace MarkopTest.Handler
{
    internal class TestHandlerHelper
    {
        private static TestHandler GetTestHandler(Type type)
        {
            if (new StackTrace().GetFrames().Any(frame =>
                    frame.GetMethod()?.DeclaringType?.DeclaringType?.BaseType?.BaseType?.Namespace ==
                    typeof(FunctionalTestFactory<>).Namespace))
                return null;

            var stackFrame = new StackTrace().GetFrames().LastOrDefault(frame =>
                frame.GetMethod()?.DeclaringType?.BaseType?.BaseType?.Namespace == type.Namespace);

            var methodBase = stackFrame?.GetMethod();

            if (methodBase == null)
                return null;

            return (TestHandler) Attribute.GetCustomAttribute(methodBase, typeof(TestHandler));
        }

        internal static async Task BeforeTest(HttpClient httpClient, Type type)
        {
            var handler = GetTestHandler(type);
            if (handler != null)
                await handler.Before(httpClient);
        }

        internal static async Task AfterTest(HttpClient httpClient, Type type)
        {
            var handler = GetTestHandler(type);
            if (handler != null)
                await handler.After(httpClient);
        }
    }
}