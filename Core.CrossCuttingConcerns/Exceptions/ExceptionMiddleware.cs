using Core.CrossCuttingConcerns.Exceptions.Handlers;
using Microsoft.AspNetCore.Http;

namespace Core.CrossCuttingConcerns.Exceptions;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HttpExceptionHandler _httpExceptionHandler;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _httpExceptionHandler = new HttpExceptionHandler();
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(httpContext.Response, exception);
        }
    }

    private Task HandleExceptionAsync(HttpResponse httpResponse, Exception exception)
    {
        httpResponse.ContentType = "application/json";
        _httpExceptionHandler.Response = httpResponse;
        return _httpExceptionHandler.HandleExceptionAsync(exception);
    }
}
