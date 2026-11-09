namespace Lab6.Contracts;

public static class HistoryQueues
{
    /// <summary>Очередь проведённых пар: преподаватели пишут в неё <see cref="LessonHeld"/>, История читает.</summary>
    public const string Lessons = "history.lessons";
}
