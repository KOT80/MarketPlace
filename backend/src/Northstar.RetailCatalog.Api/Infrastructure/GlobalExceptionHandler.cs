using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;

namespace Northstar.RetailCatalog.Api.Infrastructure;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            FluentValidation.ValidationException validationException =>
                new ValidationProblemDetails(
                    validationException.Errors
                        .GroupBy(f => f.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(f => f.ErrorMessage).ToArray()))
                {
                    Title = "Проверка входных данных не пройдена",
                    Status = StatusCodes.Status400BadRequest,
                },

            Northstar.RetailCatalog.Application.Exceptions.NotFoundException notFound =>
                Problem("Ресурс не найден", notFound.Message, httpContext, StatusCodes.Status404NotFound,
                    "https://tools.ietf.org/html/rfc9110#section-15.5.5"),

            Northstar.RetailCatalog.Application.Exceptions.SkuConflictException conflict =>
                Problem("Конфликт данных", conflict.Message, httpContext, StatusCodes.Status409Conflict,
                    "https://tools.ietf.org/html/rfc9110#section-15.5.9",
                    extensions: new Dictionary<string, object?> { ["sku"] = conflict.Sku }),

            Northstar.RetailCatalog.Domain.Exceptions.StateConflictException stateConflict =>
                Problem("Недопустимый переход состояния", stateConflict.Message, httpContext, StatusCodes.Status409Conflict,
                    "https://tools.ietf.org/html/rfc9110#section-15.5.9"),

            Northstar.RetailCatalog.Domain.Exceptions.DomainRuleException rule =>
                Problem("Бизнес-правило нарушено", rule.Message, httpContext, StatusCodes.Status400BadRequest,
                    "https://tools.ietf.org/html/rfc9110#section-15.5.1"),

            Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException =>
                Problem("Данные изменены параллельной операцией",
                    "Товар был изменён другим запросом. Загрузите актуальную версию и повторите попытку.",
                    httpContext, StatusCodes.Status409Conflict,
                    "https://tools.ietf.org/html/rfc9110#section-15.5.9"),

            Microsoft.EntityFrameworkCore.DbUpdateException dbUpdate
                when dbUpdate.InnerException is System.Data.Common.DbException dbException
                && dbException.SqlState == "23505" =>
                Problem("Конфликт уникальности",
                    "Нарушено ограничение уникальности в базе данных (например, артикул уже существует).",
                    httpContext, StatusCodes.Status409Conflict,
                    "https://tools.ietf.org/html/rfc9110#section-15.5.9"),

            Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateFk
                when dbUpdateFk.InnerException is System.Data.Common.DbException dbFk
                && dbFk.SqlState == "23503" =>
                Problem("Нарушение ссылочной целостности",
                    "Связанная запись отсутствует или изменена параллельно.",
                    httpContext, StatusCodes.Status409Conflict,
                    "https://tools.ietf.org/html/rfc9110#section-15.5.9"),

            _ => null,
        };

        if (problemDetails is null)
        {
            logger.LogError(exception, "Необработанная ошибка запроса {Path}", httpContext.Request.Path);
            problemDetails = Problem(
                "Внутренняя ошибка сервера",
                "Произошла непредвиденная ошибка. Повторите попытку позже.",
                httpContext, StatusCodes.Status500InternalServerError,
                "https://tools.ietf.org/html/rfc9110#section-15.6.1");
        }

        httpContext.Response.ContentType = "application/problem+json";
        await Results.Problem(problemDetails).ExecuteAsync(httpContext);
        return true;
    }

    private static ProblemDetails Problem(
        string title,
        string detail,
        HttpContext httpContext,
        int status,
        string type,
        Dictionary<string, object?>? extensions = null)
    {
        var problem = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = status,
            Type = type,
            Instance = httpContext.Request.Path,
        };

        if (extensions is not null)
        {
            foreach (var pair in extensions)
            {
                problem.Extensions[pair.Key] = pair.Value;
            }
        }

        return problem;
    }
}
