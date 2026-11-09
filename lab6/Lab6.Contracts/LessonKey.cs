namespace Lab6.Contracts;

/// <summary>Одна пара: какой семестр, какой день, какой предмет.</summary>
public record LessonKey(Guid SemesterId, int Day, Subject Subject);
