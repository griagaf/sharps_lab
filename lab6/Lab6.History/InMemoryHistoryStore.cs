using System.Collections.Concurrent;
using Lab6.Contracts;

namespace Lab6.History;

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
