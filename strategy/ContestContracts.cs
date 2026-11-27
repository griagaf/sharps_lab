public enum Subject
{
    Calculus,
    LinearAlgebra,
    DiscreteMath,
    Programming,
    Physics,
    English
}

public interface ISkipStrategy
{
    string Name { get; }

    /// <summary>Решение по каждому предмету до начала дня. true — идти, false — прогулять.</summary>
    bool[] DecideDay(int day, IReadOnlyStudentHistory history);
}

public interface IReadOnlyStudentHistory
{
    bool Attended(int day, Subject subject);

    /// <summary>Возвращает null, если исход пары неизвестен студенту.</summary>
    bool? WasAsked(int day, Subject subject);
}
