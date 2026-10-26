namespace Lab2;

public class CopyYesterdayRule : ITeacherRule
{
    private readonly Subject _subjectA;

    public CopyYesterdayRule(Subject subjectA)
    {
        _subjectA = subjectA;
    }

    public string Description => $"спрашивает, если вчера спросили по предмету «{Subjects.GetName(_subjectA)}»";

    public bool ShouldAsk(bool[] askedYesterday)
    {
        return askedYesterday[(int)_subjectA];
    }
}
