using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace JaCoreUI.Services.Api;

/// <summary>
/// Base class for API clients that provides common functionality
/// </summary>
public class ApiClientBase
{
    protected readonly HttpClient _httpClient;
    protected readonly string _baseUrl;
    protected readonly JsonSerializerOptions _jsonOptions;
    
    public ApiClientBase(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001/api";
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }
    
    /// <summary>
    /// Sets the authentication token for API requests
    /// </summary>
    public void SetAuthToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
    
    /// <summary>
    /// Handles HTTP exceptions and converts them to specific error types
    /// </summary>
    protected ApiException HandleApiException(HttpResponseMessage response, Exception ex)
    {
        return response.StatusCode switch
        {
            System.Net.HttpStatusCode.NotFound => new ApiException(ApiErrorType.NotFound, "Resource not found", ex),
            System.Net.HttpStatusCode.Unauthorized => new ApiException(ApiErrorType.Unauthorized, "Unauthorized access", ex),
            System.Net.HttpStatusCode.Forbidden => new ApiException(ApiErrorType.Forbidden, "Access forbidden", ex),
            System.Net.HttpStatusCode.BadRequest => new ApiException(ApiErrorType.ValidationFailed, "Validation failed", ex),
            _ => new ApiException(ApiErrorType.Unknown, $"API error: {ex.Message}", ex)
        };
    }
    
    /// <summary>
    /// Sends a GET request to the API and returns the result
    /// </summary>
    protected async Task<T> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/{endpoint}");
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
            return result ?? throw new ApiException(ApiErrorType.DeserializationFailed, "Failed to deserialize response");
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException(ApiErrorType.ConnectionFailed, $"Failed to connect to API: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new ApiException(ApiErrorType.DeserializationFailed, $"Failed to deserialize response: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Unknown error: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Sends a POST request to the API and returns the result
    /// </summary>
    protected async Task<TResult> PostAsync<TData, TResult>(string endpoint, TData data)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/{endpoint}", data, _jsonOptions);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<TResult>(_jsonOptions);
            return result ?? throw new ApiException(ApiErrorType.DeserializationFailed, "Failed to deserialize response");
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException(ApiErrorType.ConnectionFailed, $"Failed to connect to API: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new ApiException(ApiErrorType.DeserializationFailed, $"Failed to deserialize response: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Unknown error: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Sends a POST request to the API with no expected result
    /// </summary>
    protected async Task PostAsync<TData>(string endpoint, TData data)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/{endpoint}", data, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException(ApiErrorType.ConnectionFailed, $"Failed to connect to API: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Unknown error: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Sends a PUT request to the API
    /// </summary>
    protected async Task PutAsync<TData>(string endpoint, TData data)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/{endpoint}", data, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException(ApiErrorType.ConnectionFailed, $"Failed to connect to API: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Unknown error: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Sends a DELETE request to the API
    /// </summary>
    protected async Task DeleteAsync(string endpoint)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/{endpoint}");
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException(ApiErrorType.ConnectionFailed, $"Failed to connect to API: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Unknown error: {ex.Message}", ex);
        }
    }
} 