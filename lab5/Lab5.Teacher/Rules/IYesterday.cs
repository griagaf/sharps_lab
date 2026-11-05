using Lab5.Contracts;

namespace Lab5.Teacher.Rules;

/// <summary>Вчерашний день семестра — всё, что правило может узнать о прошлом.</summary>
public interface IYesterday
{
    /// <returns>Спросили ли вчера по предмету. В первый день семестра вчера никого не спрашивали.</returns>
    Task<bool> WasAskedAsync(Subject subject, CancellationToken cancellationToken);
}
