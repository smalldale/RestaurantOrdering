using System.Text.Json.Serialization;

namespace RestaurantOrdering.API.DTOs.Common;

public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = [];

    // Transport detail used by controllers to pick the HTTP status code.
    // It is not serialized, so the response body shape stays unchanged.
    [JsonIgnore]
    public int StatusCode { get; set; } = StatusCodes.Status200OK;

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
