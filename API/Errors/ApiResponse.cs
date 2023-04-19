namespace API.Errors;

public class ApiResponse
{
    public ApiResponse(int statusCode, string message = null)
    {
        StatusCode = statusCode;
        Message = message ?? GetDefaultMessageForStatusCode(statusCode);
    }

    public int StatusCode { get; set; }
    public string Message { get; set; }

    private string GetDefaultMessageForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            400 => "You have made a bad request. Think about it.",
            401 => "You are not authorized. You must be to reach what you want.",
            404 => "Resource was not found. It should be?",
            500 => "Some internal error that possible will make Alice very angry.",
            _ => null
        };
    }

}
