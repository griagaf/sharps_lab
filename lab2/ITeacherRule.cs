namespace Lab2;

public interface ITeacherRule
{
    string Description { get; }

    bool ShouldAsk(bool[] askedYesterday);
}
