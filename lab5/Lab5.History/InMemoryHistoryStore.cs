using System.Collections.Concurrent;

namespace Lab5.History;

public class InMemoryHistoryStore : IHistoryStore
{
    private readonly ConcurrentDictionary<LessonKey, bool> _lessons = new ConcurrentDictionary<LessonKey, bool>();

    public void Save(LessonKey lesson, bool asked)
    {
        _lessons[lesson] = asked;
    }

    public bool? Find(LessonKey lesson)
    {
        return _lessons.TryGetValue(lesson, out bool asked) ? asked : null;
    }
}
