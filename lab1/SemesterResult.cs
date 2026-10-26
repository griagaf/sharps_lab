namespace Lab1;

public class SemesterResult
{
    public SemesterResult(int totalPleasure, int daysPassed)
    {
        TotalPleasure = totalPleasure;
        DaysPassed = daysPassed;
    }

    public SemesterResult(int expelledDay, Subject expelledSubject)
    {
        TotalPleasure = 0;
        DaysPassed = expelledDay;
        IsExpelled = true;
        ExpelledSubject = expelledSubject;
    }

    public int TotalPleasure { get; }

    public int DaysPassed { get; }

    public bool IsExpelled { get; }

    public Subject ExpelledSubject { get; }

    public double AveragePleasurePerDay => DaysPassed == 0 ? 0 : (double)TotalPleasure / DaysPassed;
}
