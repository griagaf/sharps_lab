using Lab4.Students;
using Lab4.Teachers;

namespace Lab4.Simulation;

public class SemesterFactory
{
    private readonly TeacherRuleGenerator _ruleGenerator;
    private readonly ISkipStrategy _strategy;

    public SemesterFactory(TeacherRuleGenerator ruleGenerator, ISkipStrategy strategy)
    {
        _ruleGenerator = ruleGenerator;
        _strategy = strategy;
    }

    public Semester CreateSemester()
    {
        return new Semester(_ruleGenerator.CreateForAllSubjects(), _strategy);
    }
}
