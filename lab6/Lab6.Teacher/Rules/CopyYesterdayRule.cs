using Lab6.Contracts;

namespace Lab6.Teacher.Rules;

public class CopyYesterdayRule : ITeacherRule
{
    public CopyYesterdayRule(Subject subjectA)
    {
        SubjectA = subjectA;
    }

    public Subject SubjectA { get; }

    public string Description => $"спрашивает, если вчера спросили по предмету «{Subjects.GetName(SubjectA)}»";

    public Task<bool> ShouldAskAsync(IYesterday yesterday, CancellationToken cancellationToken)
    {
        return yesterday.WasAskedAsync(SubjectA, cancellationToken);
    }
}
