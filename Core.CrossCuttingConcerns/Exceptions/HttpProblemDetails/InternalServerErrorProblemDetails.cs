using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core.CrossCuttingConcerns.Exceptions.HttpProblemDetails;

public class InternalServerErrorProblemDetails : ProblemDetails
{
    // RFC 7807 standard fields
    public InternalServerErrorProblemDetails(string detail)
    {
        Title = "Internal Server Error";
        Detail = "Internal Server Error";
        Status = StatusCodes.Status500InternalServerError; // Set the HTTP status code to 500 Internal Server Error
        Type = "https://example.com/probs/internal-server-error"; // A URI reference that identifies the problem type
    }
}