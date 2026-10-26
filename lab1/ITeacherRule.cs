namespace Lab1;

public interface ITeacherRule
{
    string Description { get; }

    bool ShouldAsk(bool[] askedYesterday);
}
