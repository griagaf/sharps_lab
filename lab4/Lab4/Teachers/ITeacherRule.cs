namespace Lab4.Teachers;

public interface ITeacherRule
{
    string Description { get; }

    bool ShouldAsk(bool[] askedYesterday);
}
