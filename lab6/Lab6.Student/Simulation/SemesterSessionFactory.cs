using Lab6.Student.Students;

namespace Lab6.Student.Simulation;

public class SemesterSessionFactory
{
    private readonly ISkipStrategy _strategy;
    private readonly ITeachers _teachers;

    public SemesterSessionFactory(ISkipStrategy strategy, ITeachers teachers)
    {
        _strategy = strategy;
        _teachers = teachers;
    }

    /// <summary>Новый семестр со своим номером: История его записи не смешает с прошлыми.</summary>
    public SemesterSession StartNew()
    {
        return new SemesterSession(new Semester(_strategy), _teachers);
    }
}
