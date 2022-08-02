using System;
using System.Diagnostics;
using System.Linq;
using MarkopTest.FunctionalTest;

namespace MarkopTest.Handler
{
    internal class TestHandlerHelper
    {
        internal static TestHandler GetTestHandler(Type type)
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

            // TODO multiple handler because the attribute can use multiple time
            return (TestHandler) Attribute.GetCustomAttribute(methodBase, typeof(TestHandler));
        }
    }
}