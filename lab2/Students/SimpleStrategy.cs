using Lab2.Domain;

namespace Lab2.Students;

public class SimpleStrategy : ISkipStrategy
{
    private readonly int _observationDays;

    public SimpleStrategy(int observationDays = 15)
    {
        _observationDays = observationDays;
    }

    public string Name => $"Наблюдаю {_observationDays} дней, потом прогуливаю предметы, где не спрашивали";

    public bool[] DecideDay(int day, IReadOnlyStudentHistory history)
    {
        var goToLesson = new bool[Subjects.Count];

        foreach (Subject subject in Subjects.All)
        {
            if (day <= _observationDays)
            {
                goToLesson[(int)subject] = true;
            }
            else
            {
                goToLesson[(int)subject] = WasAskedDuringObservation(subject, history);
            }
        }

        return goToLesson;
    }

    private bool WasAskedDuringObservation(Subject subject, IReadOnlyStudentHistory history)
    {
        for (int day = 1; day <= _observationDays; day++)
        {
            if (history.WasAsked(day, subject) == true)
            {
                return true;
            }
        }

        return false;
    }
}
