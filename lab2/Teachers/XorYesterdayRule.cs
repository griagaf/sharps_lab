using Lab2.Domain;

namespace Lab2.Teachers;

public class XorYesterdayRule : ITeacherRule
{
    private readonly Subject _subjectA;
    private readonly Subject _subjectB;

    public XorYesterdayRule(Subject subjectA, Subject subjectB)
    {
        if (subjectA == subjectB)
        {
            throw new ArgumentException("Предметы A и Б не должны совпадать");
        }

        _subjectA = subjectA;
        _subjectB = subjectB;
    }

    public string Description =>
        $"спрашивает, если вчера спросили ровно по одному из предметов «{Subjects.GetName(_subjectA)}» " +
        $"и «{Subjects.GetName(_subjectB)}»";

    public bool ShouldAsk(bool[] askedYesterday)
    {
        return askedYesterday[(int)_subjectA] != askedYesterday[(int)_subjectB];
    }
}
