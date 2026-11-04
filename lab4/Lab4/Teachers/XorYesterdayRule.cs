using Lab4.Domain;

namespace Lab4.Teachers;

public class XorYesterdayRule : ITeacherRule
{
    public XorYesterdayRule(Subject subjectA, Subject subjectB)
    {
        if (subjectA == subjectB)
        {
            throw new ArgumentException("Предметы A и Б не должны совпадать");
        }

        SubjectA = subjectA;
        SubjectB = subjectB;
    }

    public Subject SubjectA { get; }

    public Subject SubjectB { get; }

    public string Description =>
        $"спрашивает, если вчера спросили ровно по одному из предметов «{Subjects.GetName(SubjectA)}» " +
        $"и «{Subjects.GetName(SubjectB)}»";

    public bool ShouldAsk(bool[] askedYesterday)
    {
        return askedYesterday[(int)SubjectA] != askedYesterday[(int)SubjectB];
    }
}
