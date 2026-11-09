using Lab6.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Lab6.Teacher;

public static class LessonEndpoints
{
    public static IEndpointRouteBuilder MapLessonEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(TeacherRoutes.Lessons, TeachLesson);

        return endpoints;
    }

    private static async Task<Results<Ok<LessonResponse>, BadRequest<string>>> TeachLesson(
        LessonRequest request, Lecturer lecturer, CancellationToken cancellationToken)
    {
        if (request.Day < 1)
        {
            return TypedResults.BadRequest("День должен быть не меньше 1");
        }

        bool asked = await lecturer.TeachAsync(request, cancellationToken);
        return TypedResults.Ok(new LessonResponse(asked));
    }
}
