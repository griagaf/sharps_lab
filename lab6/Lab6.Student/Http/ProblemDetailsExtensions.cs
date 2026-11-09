using System.Net.Http.Json;
using System.Text.Json;

namespace Lab6.Student.Http;

/// <summary>Чтение стандартного тела ошибки HTTP — ProblemDetails.</summary>
public static class ProblemDetailsExtensions
{
    /// <returns>Причина ошибки (поле detail) или null, если тело другое.</returns>
    public static async Task<string?> ReadProblemDetailAsync(this HttpContent content, CancellationToken cancellationToken)
    {
        try
        {
            ProblemDetailsBody? problem = await content.ReadFromJsonAsync<ProblemDetailsBody>(cancellationToken);
            return string.IsNullOrWhiteSpace(problem?.Detail) ? null : problem.Detail;
        }
        // InvalidOperationException — так ReadFromJsonAsync сообщает о неизвестном charset.
        catch (Exception e) when (e is JsonException or InvalidOperationException)
        {
            return null;
        }
    }

    // Нужна только причина, остальные поля ProblemDetails не читаем.
    private record ProblemDetailsBody(string? Detail);
}
