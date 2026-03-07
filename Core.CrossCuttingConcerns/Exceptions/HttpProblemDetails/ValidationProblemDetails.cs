using Core.CrossCuttingConcerns.Exceptions.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.CrossCuttingConcerns.Exceptions.HttpProblemDetails;

public class ValidationProblemDetails : ProblemDetails
{
    public IEnumerable<ValidationExceptionModel> Errors { get; init; }

    // RFC 7807 standard fields
    public ValidationProblemDetails(IEnumerable<ValidationExceptionModel> errors)
    {
        Title = "Validation Error";
        Detail = "One or more validation errors occurred.";
        Status = StatusCodes.Status400BadRequest; // Set the HTTP status code to 400 Bad Request
        Type = "https://example.com/probs/validation-error"; // A URI reference that identifies the problem type
        Errors = errors ?? Array.Empty<ValidationExceptionModel>();
    }
}
