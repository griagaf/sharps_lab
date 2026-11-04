using Lab4.Randomness;
using Lab4.Students;
using Lab4.Teachers;

namespace Lab4.Simulation;

public class SemesterFactory
{
    private readonly ISkipStrategy _strategy;

    public SemesterFactory(ISkipStrategy strategy)
    {
        _strategy = strategy;
    }

    // У каждого семестра свой рандом: им выбираются правила, им же потом спрашивает случайное правило.
    public Semester CreateSemester(IRandomSource random)
    {
        return new Semester(new TeacherRuleGenerator(random).CreateForAllSubjects(), _strategy);
    }

    // Семестр с уже выбранными правилами — например, загруженными из БД.
    public Semester CreateSemester(ITeacherRule[] teacherRules)
    {
        return new Semester(teacherRules, _strategy);
    }
}
