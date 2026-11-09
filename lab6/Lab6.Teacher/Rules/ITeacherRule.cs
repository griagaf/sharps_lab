namespace Lab6.Teacher.Rules;

public interface ITeacherRule
{
    string Description { get; }

    Task<bool> ShouldAskAsync(IYesterday yesterday, CancellationToken cancellationToken);
}
