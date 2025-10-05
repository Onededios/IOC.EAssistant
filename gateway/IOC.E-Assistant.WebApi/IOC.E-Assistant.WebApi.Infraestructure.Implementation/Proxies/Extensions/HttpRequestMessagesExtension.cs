﻿namespace IOC.E_Assistant.WebApi.Infrastructure.Implementation;
public static class HttpRequestMessagesExtension
{
    public static HttpRequestMessage AddHeaders(this HttpRequestMessage request, Dictionary<string, string> headers)
    {
        foreach (var header in headers)
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return request;
    }
}