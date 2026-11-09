using Lab6.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

namespace Lab6.History;

public static class HistoryEndpoints
{
    public static IEndpointRouteBuilder MapHistoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(HistoryRoutes.Lesson, SaveLesson);
        endpoints.MapGet(HistoryRoutes.Lesson, FindLesson);

        return endpoints;
    }

    private static Results<NoContent, BadRequest<string>> SaveLesson(
        Guid semesterId, int day, Subject subject, LessonRecord record, IHistoryStore store)
    {
        string? error = Validate(day, subject);

        if (error is not null)
        {
            return TypedResults.BadRequest(error);
        }

        store.Save(new LessonKey(semesterId, day, subject), record.Asked);
        return TypedResults.NoContent();
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
        string? error = Validate(day, subject);

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

    private static string? Validate(int day, Subject subject)
    {
        if (day < 1)
        {
            return "День должен быть не меньше 1";
        }

        if (!Enum.IsDefined(subject))
        {
            return $"Неизвестный предмет {subject}";
        }

        return null;
    }
}
