using System.Net;
using Newtonsoft.Json;

namespace MLZ2025.Core.Tests;

public class Address
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Id { get; set; }
}

[TestFixture]
public class HttpClientTests : TestsBase
{
    [Test]
    public async Task TestGetAddresses()
    {
        var serviceProvider = CreateServiceProvider();
        var httpClient = serviceProvider.GetRequiredService<HttpClient>();

        httpClient.BaseAddress = new Uri("http://localhost:3000");

        var result = await httpClient.GetAsync("/addresses/");

        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.IsSuccessStatusCode, Is.True);

        var stringResult = await result.Content.ReadAsStringAsync();

        var addresses = JsonConvert.DeserializeObject<IList<Address>>(stringResult);

        Assert.That(addresses, Is.Not.Null);
        Assert.That(addresses.Count, Is.EqualTo(2));
    }
}

public class HttpServerAccess
{
    private readonly HttpClient _client;

    public HttpServerAccess(HttpClient client)
    {
        _client = client;
        _client.BaseAddress = new Uri("http://localhost:3000");
    }

    public async Task<IList<Address>> GetAddressesAsync()
    {
        var response = await _client.GetAsync("/addresses/");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<IList<Address>>(content);

        if (result == null)
        {
            throw new InvalidOperationException("Could not get addresses from server.");
        }

        return result;
    }
}

public class AddressServerAccess : HttpServerAccess
{
    public AddressServerAccess(HttpClient client)
        : base(client)
    {
    }
}
