namespace Lab4.Persistence.Records;

// Строка таблицы days.
public class DayRecord
{
    public int Day { get; init; }

    public int TotalPleasure { get; init; }

    public bool IsExpelled { get; init; }

    public bool[] GoToLesson { get; init; } = [];

    public bool[] Asked { get; init; } = [];

    public int RandomDraws { get; init; }
}
