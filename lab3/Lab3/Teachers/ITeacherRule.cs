namespace Lab3.Teachers;

public interface ITeacherRule
{
    string Description { get; }

    bool ShouldAsk(bool[] askedYesterday);
}
