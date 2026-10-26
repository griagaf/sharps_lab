namespace Lab2.Domain;

public static class Subjects
{
    public static readonly Subject[] All = Enum.GetValues<Subject>();

    public static int Count => All.Length;

    public static string GetName(Subject subject)
    {
        return subject switch
        {
            Subject.Calculus => "Матанализ",
            Subject.LinearAlgebra => "Линейная алгебра",
            Subject.DiscreteMath => "Дискретная математика",
            Subject.Programming => "Программирование",
            Subject.Physics => "Физика",
            Subject.English => "Английский язык",
            _ => throw new ArgumentOutOfRangeException(nameof(subject), subject, "Неизвестный предмет"),
        };

    }
}
