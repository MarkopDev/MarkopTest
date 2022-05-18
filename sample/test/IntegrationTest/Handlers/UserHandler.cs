using System.Net.Http;
using MarkopTest.Handler;
using System.Threading.Tasks;
using IntegrationTest.Utilities;

namespace IntegrationTest.Handlers;

public class UserHandler : TestHandler
{
    public override async Task BeforeRequest(HttpClient client)
    {
        await client.User();
    }
}