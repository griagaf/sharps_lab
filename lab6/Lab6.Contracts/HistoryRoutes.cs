namespace Lab6.Contracts;

public static class HistoryRoutes
{
    /// <summary>Шаблон для сервера: из него ASP.NET достаёт параметры обработчика.</summary>
    public const string Lesson = "/semesters/{semesterId:guid}/days/{day:int}/subjects/{subject}";

    /// <summary>Тот же адрес для клиента, относительно адреса сервиса Истории.</summary>
    public static string ForLesson(LessonKey lesson)
    {
        return $"semesters/{lesson.SemesterId}/days/{lesson.Day}/subjects/{lesson.Subject}";
    }
}
