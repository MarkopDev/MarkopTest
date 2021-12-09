using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;

namespace MarkopTest.Handler;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public abstract class TestHandler : ValidationAttribute
{
    protected TestHandler()
    {
    }

    public virtual Task Before(HttpClient client) => Task.CompletedTask;

    public virtual Task After(HttpClient client) => Task.CompletedTask;
}