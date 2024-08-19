using System.Net;
using System.Text.Json;

namespace Rivr.Test;

public class MockHttpMessageHandler(
    object? expectedResponse = null,
    HttpStatusCode expectedStatusCode = HttpStatusCode.OK
) :
    HttpMessageHandler
{
    public int PerformedRequestsCount { get; private set; }
    public string? RequestContent { get; private set; }
    public T? GetRequestContent<T>() => JsonSerializer.Deserialize<T>(RequestContent ?? string.Empty);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        PerformedRequestsCount++;
        if (request.Content != null)
        {
            RequestContent = await request.Content.ReadAsStringAsync(cancellationToken);
        }

        var content = expectedResponse is null
            ? new StringContent(string.Empty)
            : new StringContent(JsonSerializer.Serialize(expectedResponse));

        return new HttpResponseMessage(expectedStatusCode)
        {
            Content = content
        };
    }
}