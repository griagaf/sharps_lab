using Lab6.Contracts;

namespace Lab6.Teacher.Rules;

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

    public async Task<bool> ShouldAskAsync(IYesterday yesterday, CancellationToken cancellationToken)
    {
        bool askedA = await yesterday.WasAskedAsync(SubjectA, cancellationToken);
        bool askedB = await yesterday.WasAskedAsync(SubjectB, cancellationToken);

        return askedA != askedB;
    }
}
