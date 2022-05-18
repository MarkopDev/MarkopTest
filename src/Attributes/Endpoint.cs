using System;

namespace MarkopTest.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class Endpoint : Attribute
    {
        internal readonly string Template;

        public Endpoint(string template)
        {
            Template = template;
        }
    }
}