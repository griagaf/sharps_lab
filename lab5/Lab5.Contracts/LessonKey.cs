namespace Lab5.Contracts;

/// <summary>Одна пара: какой семестр, какой день, какой предмет.</summary>
public record LessonKey(Guid SemesterId, int Day, Subject Subject);
