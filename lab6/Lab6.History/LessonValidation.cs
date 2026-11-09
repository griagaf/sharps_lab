using Lab6.Contracts;

namespace Lab6.History;

/// <summary>Проверка пары, пришедшей извне: из адреса запроса или из сообщения в очереди.</summary>
public static class LessonValidation
{
    /// <returns>Что не так с парой или null, если всё в порядке.</returns>
    public static string? Validate(int day, Subject subject)
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
