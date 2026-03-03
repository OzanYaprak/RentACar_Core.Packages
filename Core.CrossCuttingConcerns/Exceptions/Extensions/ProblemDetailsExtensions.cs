using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.CrossCuttingConcerns.Exceptions.Extensions;

public static class ProblemDetailsExtensions
{
    /// <summary>
    /// This extension method serializes a ProblemDetails object to a JSON string using System.Text.Json.
    /// Test için oluşturulmuş bir extension methoddur. ProblemDetails nesnesini JSON formatına dönüştürür.
    /// 
    //public static string AsJson(ProblemDetails problemDetails)
    //{
    //    return JsonSerializer.Serialize(problemDetails);
    //}

    /// <summary>
    /// Serializes the specified problem details object to its JSON string representation.
    /// </summary>
    /// <typeparam name="TProblemDetail">The type of the problem details object to serialize. Must derive from ProblemDetails.</typeparam>
    /// <param name="problemDetail">The problem details object to serialize to JSON. Cannot be null.</param>
    /// <returns>A JSON string representation of the specified problem details object.</returns>
    public static string AsJson<TProblemDetail>(this TProblemDetail problemDetail) where TProblemDetail : ProblemDetails => JsonSerializer.Serialize(problemDetail);
}


