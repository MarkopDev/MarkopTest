using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Features.Account.Commands.SignIn;

namespace IntegrationTest.Utilities;

public static class Extensions
{
    public static async Task<bool> HasErrorCode(this HttpResponseMessage response, ErrorCode? errorCode = null)
    {
        var errorProperty = (await response.GetJson())?.EnumerateObject()
            .FirstOrDefault(e => e.Name == "errors").Value;
        var errors = errorProperty?.ValueKind == JsonValueKind.Undefined
            ? null
            : errorProperty?.EnumerateArray().ToArray();
        if (errorCode == null)
            return errors == null || errors.ToArray().Length == 0;
        return errors != null && errors.ToArray().Select(error =>
                (ErrorCode) error.EnumerateObject().FirstOrDefault(e => e.Name == "code").Value.GetInt32())
            .Any(error => error == errorCode);
    }

    public static async Task<JsonElement?> GetJson(this HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(content) ? null : JsonSerializer.Deserialize<JsonElement>(content);
    }

    public static async Task<HttpResponseMessage> PostAsync<T>(this HttpClient client, string url, T data)
    {
        return await client.PostAsync(url, JsonContent.Create(data));
    }

    public static async Task<HttpClient> User(this HttpClient client)
    {
        var data = new SignInCommand
        {
            Login = "TestUser@Markop.com",
            Type = LoginType.Email,
            Password = "TestPassword"
        };

        var response = await client.PostAsync("/api/Account/SignIn", data);

        foreach (var cookie in response.Headers.GetValues("Set-Cookie").ToArray())
            client.DefaultRequestHeaders.Add("Cookie", cookie);

        return client;
    }

    public static async Task<HttpClient> User2(this HttpClient client)
    {
        var data = new SignInCommand
        {
            Login = "TestUser2@Markop.com",
            Type = LoginType.Email,
            Password = "TestPassword"
        };

        var response = await client.PostAsync("api/Account/SignIn", data);

        foreach (var cookie in response.Headers.GetValues("Set-Cookie").ToArray())
            client.DefaultRequestHeaders.Add("Cookie", cookie);

        return client;
    }

    public static async Task<HttpClient> Owner(this HttpClient client)
    {
        var data = new SignInCommand
        {
            Login = "TestOwner@Markop.com",
            Type = LoginType.Email,
            Password = "OwnerPassword"
        };

        var response = await client.PostAsync("api/Account/SignIn", data);

        foreach (var cookie in response.Headers.GetValues("Set-Cookie").ToArray())
            client.DefaultRequestHeaders.Add("Cookie", cookie);

        return client;
    }
}