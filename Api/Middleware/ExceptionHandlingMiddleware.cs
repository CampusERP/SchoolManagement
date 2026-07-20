using System.Net;
using System.Text.Json;
using System.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Application.Common.Exceptions;
using Domain.Exceptions;

namespace Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            ValidationException ve => (
                HttpStatusCode.BadRequest,
                "One or more validation errors occurred.",
                ve.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
                         .GroupBy(e => e.PropertyName)
                         .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            ),
            NotFoundException nfe   => (HttpStatusCode.NotFound,       nfe.Message,  (object?)null),
            ForbiddenException fe   => (HttpStatusCode.Forbidden,       fe.Message,  null),
            DomainException de      => (HttpStatusCode.BadRequest,      de.Message,  null),
            DbUpdateConcurrencyException => (
                HttpStatusCode.Conflict,
                "The record was modified by another process. Please reload and try again.",
                (object?)null),
            DbUpdateException due    => (
                HttpStatusCode.Conflict,
                "The operation conflicts with existing data (e.g. a duplicate value). Please check your input.",
                (object?)null),
            _                       => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            message,
            errors
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
