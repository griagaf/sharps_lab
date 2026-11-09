using Lab6.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

namespace Lab6.History;

public static class HistoryEndpoints
{
    public static IEndpointRouteBuilder MapHistoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(HistoryRoutes.Lesson, FindLesson);

        return endpoints;
    }

    // Записи приходят из очереди с задержкой, поэтому отсутствующую пару сначала ждём и только потом отвечаем 404.
    private static async Task<Results<Ok<LessonRecord>, NotFound, BadRequest<string>>> FindLesson(
        Guid semesterId,
        int day,
        Subject subject,
        IHistoryStore store,
        IOptions<HistoryOptions> options,
        CancellationToken cancellationToken)
    {
        string? error = LessonValidation.Validate(day, subject);

        if (error is not null)
        {
            return TypedResults.BadRequest(error);
        }

        bool? asked = await store.WaitAsync(
            new LessonKey(semesterId, day, subject), options.Value.ReadTimeout, cancellationToken);

        return asked is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(new LessonRecord(asked.Value));
    }
}
