using Lab6.Teacher.History;
using Microsoft.AspNetCore.Diagnostics;

namespace Lab6.Teacher;

/// <summary>
/// Сбой Истории — не ошибка преподавателя, поэтому 502 Bad Gateway с причиной в теле
/// и одна строка в логе вместо стека. Остальные исключения не трогает: они остаются 500.
/// </summary>
public class HistoryExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;
    private readonly ILogger<HistoryExceptionHandler> _logger;

    public HistoryExceptionHandler(IProblemDetailsService problemDetails, ILogger<HistoryExceptionHandler> logger)
    {
        _problemDetails = problemDetails;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not HistoryException)
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
                Title = "Не удалось провести пару: сбой сервиса Истории",
                Detail = exception.Message,
                Status = StatusCodes.Status502BadGateway,
            },
        });
    }
}
