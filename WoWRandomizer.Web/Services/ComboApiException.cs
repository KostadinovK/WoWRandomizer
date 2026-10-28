using System.Net;

namespace WoWRandomizer.Web.Services;

/// <summary>The API answered with an error status code.</summary>
public sealed class ComboApiException : Exception
{
    public ComboApiException(HttpStatusCode statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; }
}
