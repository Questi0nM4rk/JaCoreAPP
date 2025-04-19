using System;

namespace JaCoreUI.Services.Api;

/// <summary>
/// Types of errors that can occur when communicating with the API
/// </summary>
public enum ApiErrorType
{
    ConnectionFailed,
    NotFound,
    ValidationFailed,
    Unauthorized,
    Forbidden,
    BadRequest,
    DeserializationFailed,
    ConcurrencyIssue,
    Unknown
}

/// <summary>
/// Exception for API-specific errors
/// </summary>
public class ApiException : Exception
{
    /// <summary>
    /// The type of API error
    /// </summary>
    public ApiErrorType ErrorType { get; }
    
    /// <summary>
    /// Creates a new API exception
    /// </summary>
    /// <param name="errorType">The type of error</param>
    /// <param name="message">Error message</param>
    /// <param name="innerException">Optional inner exception</param>
    public ApiException(ApiErrorType errorType, string message, Exception? innerException = null) 
        : base(message, innerException)
    {
        ErrorType = errorType;
    }
} 