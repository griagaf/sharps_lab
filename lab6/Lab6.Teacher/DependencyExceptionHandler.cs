using Lab6.Teacher.History;
using Lab6.Teacher.Queue;
using Microsoft.AspNetCore.Diagnostics;

namespace Lab6.Teacher;

/// <summary>
/// Сбой Истории или брокера — не ошибка преподавателя, поэтому 502 Bad Gateway с причиной в теле
/// и одна строка в логе вместо стека. Остальные исключения не трогает: они остаются 500.
/// </summary>
public class DependencyExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;
    private readonly ILogger<DependencyExceptionHandler> _logger;

    public DependencyExceptionHandler(IProblemDetailsService problemDetails, ILogger<DependencyExceptionHandler> logger)
    {
        _problemDetails = problemDetails;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        string? title = exception switch
        {
            HistoryException => "Не удалось провести пару: сбой сервиса Истории",
            BrokerException => "Не удалось провести пару: сбой очереди Истории",
            _ => null,
        };

        if (title is null)
        {
            return false;
        }

        _logger.LogWarning("Пара не проведена: {Reason}", exception.Message);

        httpContext.Response.StatusCode = StatusCodes.Status502BadGateway;

        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Title = title,
                Detail = exception.Message,
                Status = StatusCodes.Status502BadGateway,
            },
        });
    }
}
