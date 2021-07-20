using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MarkopTest.Handler
{
    internal class TestHandlerHelper
    {
        private static TestHandler GetTestHandler(Type type)
        {
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