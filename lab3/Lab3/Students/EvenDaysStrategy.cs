using Lab3.Domain;

namespace Lab3.Students;

public class EvenDaysStrategy : ISkipStrategy
{
    private readonly int _observationDays;

    public EvenDaysStrategy(int observationDays = 20)
    {
        _observationDays = observationDays;
    }

    public string Name => $"Первые {_observationDays} дней хожу на всё, потом по чётным хожу на всё, а по нечётным прогуливаю, где ни разу не спрашивали";

    public bool[] DecideDay(int day, IReadOnlyStudentHistory history)
    {
        var goToLesson = new bool[Subjects.Count];
        bool attendEverything = day <= _observationDays || day % 2 == 0;

        foreach (Subject subject in Subjects.All)
        {
            goToLesson[(int)subject] = attendEverything || WasEverAsked(day, subject, history);
        }

        return goToLesson;
    }

    private static bool WasEverAsked(int today, Subject subject, IReadOnlyStudentHistory history)
    {
        for (int day = 1; day < today; day++)
        {
            if (history.WasAsked(day, subject) == true)
            {
                return true;
            }
        }

        return false;
    }
}
