using System.Collections.Concurrent;
using Lab6.Contracts;

namespace Lab6.History;

public class InMemoryHistoryStore : IHistoryStore
{
    private readonly ConcurrentDictionary<LessonKey, bool> _lessons = new ConcurrentDictionary<LessonKey, bool>();

    // Кто ждёт ещё не записанную пару. Все, кто ждёт одну и ту же пару, получают одну задачу.
    private readonly ConcurrentDictionary<LessonKey, TaskCompletionSource<bool>> _waiters = new();

    public void Save(LessonKey lesson, bool asked)
    {
        _lessons[lesson] = asked;

        if (_waiters.TryRemove(lesson, out TaskCompletionSource<bool>? waiter))
        {
            waiter.TrySetResult(asked);
        }
    }

    private bool? Find(LessonKey lesson)
    {
        return _lessons.TryGetValue(lesson, out bool asked) ? asked : null;
    }

    public async Task<bool?> WaitAsync(LessonKey lesson, TimeSpan timeout, CancellationToken cancellationToken)
    {
        if (Find(lesson) is bool asked)
        {
            return asked;
        }

        TaskCompletionSource<bool> waiter = _waiters.GetOrAdd(
            lesson, _ => new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously)
        );

        // Save мог пройти между первой проверкой и GetOrAdd и тогда не нашёл ожидающего.
        if (Find(lesson) is bool savedMeanwhile)
        {
            return savedMeanwhile;
        }

        try
        {
            return await waiter.Task.WaitAsync(timeout, cancellationToken);
        }
        // Ожидающий остаётся в словаре: его могут делить другие запросы, а уберёт его первая же запись этой пары.
        catch (TimeoutException)
        {
            return null;
        }
    }
}
