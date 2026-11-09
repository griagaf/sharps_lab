using System.Net;
using System.Text.Json;
using Lab6.Contracts;

namespace Lab6.Teacher.History;

// Адрес сервиса Истории задаётся в HttpClient.BaseAddress при регистрации.
public class HttpHistoryClient : IHistoryClient
{
    private readonly HttpClient _http;

    public HttpHistoryClient(HttpClient http)
    {
        _http = http;
    }

    /// <exception cref="HistoryException">История недоступна, ответила ошибкой, непонятно или не вовремя.</exception>
    public async Task<bool?> FindAskedAsync(LessonKey lesson, CancellationToken cancellationToken)
    {
        try
        {
            return await FindAskedCoreAsync(lesson, cancellationToken);
        }
        catch (Exception e) when (IsHistoryFailure(e, cancellationToken))
        {
            throw new HistoryException($"Не удалось прочитать из Истории {lesson}: {Describe(e)}", e);
        }
    }

    private async Task<bool?> FindAskedCoreAsync(LessonKey lesson, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await _http.GetAsync(HistoryRoutes.ForLesson(lesson), cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        LessonRecord record = await ReadRecordAsync(response.Content, cancellationToken)
            ?? throw new JsonException("пустой ответ");

        return record.Asked;
    }

    /// <exception cref="JsonException">Битый JSON, не тот объект или неизвестная кодировка.</exception>
    private static async Task<LessonRecord?> ReadRecordAsync(HttpContent content, CancellationToken cancellationToken)
    {
        try
        {
            return await content.ReadFromJsonAsync<LessonRecord>(ContractJson.Options, cancellationToken);
        }
        catch (InvalidOperationException e)
        {
            // Так ReadFromJsonAsync сообщает о неизвестном charset в Content-Type.
            throw new JsonException(e.Message, e);
        }
    }

    // Отмена, о которой никто не просил, — это таймаут HttpClient.
    private static bool IsHistoryFailure(Exception e, CancellationToken cancellationToken)
    {
        return e is HttpRequestException or JsonException
            || (e is TaskCanceledException && !cancellationToken.IsCancellationRequested);
    }

    private string Describe(Exception e)
    {
        return e is TaskCanceledException
            ? $"нет ответа за {_http.Timeout.TotalSeconds} с"
            // Сама причина (отказ в соединении, обрыв) лежит во вложенном исключении.
            : e.GetBaseException().Message;
    }
}
