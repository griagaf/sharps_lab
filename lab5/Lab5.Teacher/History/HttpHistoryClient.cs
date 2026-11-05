using System.Net;
using Lab5.Contracts;

namespace Lab5.Teacher.History;

// Адрес сервиса Истории задаётся в HttpClient.BaseAddress при регистрации.
public class HttpHistoryClient : IHistoryClient
{
    private readonly HttpClient _http;

    public HttpHistoryClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<bool?> FindAskedAsync(LessonKey lesson, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await _http.GetAsync(HistoryRoutes.ForLesson(lesson), cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        LessonRecord record = await response.Content.ReadFromJsonAsync<LessonRecord>(cancellationToken)
            ?? throw new HttpRequestException($"История вернула пустой ответ для {lesson}");

        return record.Asked;
    }

    public async Task SaveAsync(LessonKey lesson, bool asked, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await _http.PutAsJsonAsync(
            HistoryRoutes.ForLesson(lesson), new LessonRecord(asked), cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
