using Lab4.Domain;

namespace Lab4.Teachers;

public class CopyYesterdayRule : ITeacherRule
{
    public CopyYesterdayRule(Subject subjectA)
    {
        SubjectA = subjectA;
    }

    public Subject SubjectA { get; }

    public string Description => $"спрашивает, если вчера спросили по предмету «{Subjects.GetName(SubjectA)}»";

    public bool ShouldAsk(bool[] askedYesterday)
    {
        return askedYesterday[(int)SubjectA];
    }
}
