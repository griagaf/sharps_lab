using Lab2.Students;
using Lab2.Teachers;

namespace Lab2.Simulation;

public class SemesterFactory
{
    private readonly TeacherRuleGenerator _ruleGenerator;
    private readonly ISkipStrategy _strategy;

    public SemesterFactory(TeacherRuleGenerator ruleGenerator, ISkipStrategy strategy)
    {
        _ruleGenerator = ruleGenerator;
        _strategy = strategy;
    }

    public Semester CreateSemester(int number)
    {
        return new Semester(number, _ruleGenerator.CreateForAllSubjects(), _strategy);
    }
}
