using Lab6.Contracts;
using Lab6.Teacher.History;
using Lab6.Teacher.Rules;
using Microsoft.Extensions.Options;

namespace Lab6.Teacher;

/// <summary>Проводит пару по своему предмету: решает по правилу, спросить ли, и записывает это в Историю.</summary>
public class Lecturer
{
    private readonly Subject _subject;
    private readonly ITeacherRule _rule;
    private readonly IHistoryClient _history;
    private readonly ILogger<Lecturer> _logger;

    public Lecturer(IOptions<TeacherOptions> options, ITeacherRule rule, IHistoryClient history, ILogger<Lecturer> logger)
    {
        // Наличие предмета проверено при старте (ValidateOnStart).
        _subject = options.Value.Subject!.Value;
        _rule = rule;
        _history = history;
        _logger = logger;
    }

    /// <returns>Спросил ли преподаватель. Спрашивает он и тогда, когда студента нет.</returns>
    public async Task<bool> TeachAsync(LessonRequest request, CancellationToken cancellationToken)
    {
        var yesterday = new Yesterday(_history, request.SemesterId, request.Day);
        bool asked = await _rule.ShouldAskAsync(yesterday, cancellationToken);

        await _history.SaveAsync(new LessonKey(request.SemesterId, request.Day, _subject), asked, cancellationToken);

        _logger.LogInformation(
            "Семестр {SemesterId}, день {Day}: студент {Presence}, {Asked}",
            request.SemesterId,
            request.Day,
            request.Attended ? "пришёл" : "прогулял",
            asked ? "спросил" : "не спросил");

        return asked;
    }
}
