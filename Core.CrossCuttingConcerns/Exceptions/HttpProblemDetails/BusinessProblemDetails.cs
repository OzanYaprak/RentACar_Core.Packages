using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core.CrossCuttingConcerns.Exceptions.HttpProblemDetails;

public class BusinessProblemDetails : ProblemDetails
{
    // RFC 7807 standard fields
    public BusinessProblemDetails(string detail)
    {
        Title = "Rule Violation";
        Detail = detail;
        Status = StatusCodes.Status400BadRequest; // Set the HTTP status code to 400 Bad Request
        Type = "https://example.com/probs/business-rule-violation"; // A URI reference that identifies the problem type
    }
}
