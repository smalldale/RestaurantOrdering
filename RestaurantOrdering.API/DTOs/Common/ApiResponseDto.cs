using System.Text.Json.Serialization;

namespace RestaurantOrdering.API.DTOs.Common;

/// <summary>
/// Standard response envelope returned by every business endpoint.
/// </summary>
/// <typeparam name="T">Type of the payload in <see cref="Data"/>.</typeparam>
public class ApiResponseDto<T>
{
    /// <summary>
    /// Indicates whether the request succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Human-readable result message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Response payload. <c>null</c> when the request fails.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Validation or business error messages. Empty on success.
    /// </summary>
    public List<string> Errors { get; set; } = [];

    /// <summary>
    /// HTTP status code used by controllers to build the response.
    /// This value is not serialized to the client.
    /// </summary>
    [JsonIgnore]
    public int StatusCode { get; set; } = StatusCodes.Status200OK;

    /// <summary>
    /// Creates a successful response with a <c>200 OK</c> status.
    /// </summary>
    /// <param name="data">Response payload.</param>
    /// <param name="message">Result message.</param>
    /// <returns>A successful response envelope.</returns>
    public static ApiResponseDto<T> SuccessResult(
        T data,
        string message = "Success")
    {
        return new ApiResponseDto<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = StatusCodes.Status200OK
        };
    }

    /// <summary>
    /// Creates a failed response with a <c>400 Bad Request</c> status.
    /// </summary>
    /// <param name="message">Result message.</param>
    /// <param name="errors">Optional validation or business errors.</param>
    /// <returns>A failed response envelope.</returns>
    public static ApiResponseDto<T> ErrorResult(
        string message,
        List<string>? errors = null)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? [],
            StatusCode = StatusCodes.Status400BadRequest
        };
    }

    /// <summary>
    /// Creates a failed response with a <c>404 Not Found</c> status.
    /// </summary>
    /// <param name="message">Result message.</param>
    /// <param name="errors">Optional error details.</param>
    /// <returns>A failed response envelope.</returns>
    public static ApiResponseDto<T> NotFoundResult(
        string message,
        List<string>? errors = null)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? [],
            StatusCode = StatusCodes.Status404NotFound
        };
    }

    /// <summary>
    /// Creates a failed response with a <c>409 Conflict</c> status.
    /// </summary>
    /// <param name="message">Result message.</param>
    /// <param name="errors">Optional error details.</param>
    /// <returns>A failed response envelope.</returns>
    public static ApiResponseDto<T> ConflictResult(
        string message,
        List<string>? errors = null)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? [],
            StatusCode = StatusCodes.Status409Conflict
        };
    }
}
