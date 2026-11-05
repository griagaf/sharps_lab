using Lab5.Contracts;

namespace Lab5.History;

/// <summary>Одна пара: какой семестр, какой день, какой предмет.</summary>
public record LessonKey(Guid SemesterId, int Day, Subject Subject);
