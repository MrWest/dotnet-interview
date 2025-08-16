namespace TodoApi.ExternalApi
{
    /// <summary>
    /// Base exception for external API related errors.
    /// </summary>
    public class ExternalApiException : Exception
    {
        public ExternalApiException(string message) : base(message) { }
        public ExternalApiException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when the external API returns a 404 Not Found response.
    /// </summary>
    public class ExternalApiNotFoundException : ExternalApiException
    {
        public ExternalApiNotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception thrown when the external API returns a 400 Bad Request response.
    /// </summary>
    public class ExternalApiBadRequestException : ExternalApiException
    {
        public ExternalApiBadRequestException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception thrown when the external API returns a 401 Unauthorized response.
    /// </summary>
    public class ExternalApiUnauthorizedException : ExternalApiException
    {
        public ExternalApiUnauthorizedException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception thrown when the external API returns a 429 Too Many Requests response.
    /// </summary>
    public class ExternalApiRateLimitException : ExternalApiException
    {
        public ExternalApiRateLimitException(string message) : base(message) { }
    }
}