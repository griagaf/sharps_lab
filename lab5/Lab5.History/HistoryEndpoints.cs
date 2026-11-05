using Lab5.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Lab5.History;

public static class HistoryEndpoints
{
    private const string LessonRoute = "/semesters/{semesterId:guid}/days/{day:int}/subjects/{subject}";

    public static IEndpointRouteBuilder MapHistoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(LessonRoute, SaveLesson);
        endpoints.MapGet(LessonRoute, FindLesson);

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

    private static Results<Ok<LessonRecord>, NotFound, BadRequest<string>> FindLesson(
        Guid semesterId, int day, Subject subject, IHistoryStore store)
    {
        string? error = Validate(day, subject);

        if (error is not null)
        {
            return TypedResults.BadRequest(error);
        }

        bool? asked = store.Find(new LessonKey(semesterId, day, subject));

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
