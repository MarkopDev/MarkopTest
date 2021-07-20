using System.Net.Http;
using MarkopTest.Handler;
using System.Threading.Tasks;
using IntegrationTest.Utilities;

namespace IntegrationTest.Handlers
{
    public class OwnerHandler : TestHandler
    {
        public override async Task Before(HttpClient client)
        {
            await client.Owner();
        }
    }
}