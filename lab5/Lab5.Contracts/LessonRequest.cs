namespace Lab5.Contracts;

/// <summary>Студент пришёл на пару или прогулял её. Предмет определяется тем, к какому преподавателю пришёл запрос.</summary>
public record LessonRequest(Guid SemesterId, int Day, bool Attended);
