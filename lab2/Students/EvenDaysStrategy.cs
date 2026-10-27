using Lab2.Domain;

namespace Lab2.Students;

public class EvenDaysStrategy : ISkipStrategy
{
    public string Name => "Хожу на все пары по чётным дням, по нечётным прогуливаю всё";

    public bool[] DecideDay(int day, IReadOnlyStudentHistory history)
    {
        var goToLesson = new bool[Subjects.Count];
        bool isEvenDay = day % 2 == 0;

        foreach (Subject subject in Subjects.All)
        {
            goToLesson[(int)subject] = isEvenDay;
        }

        return goToLesson;
    }
}
