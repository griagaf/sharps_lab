using System.Net.Http.Json;
using Lab5.Contracts;
using Lab5.Student.Simulation;

namespace Lab5.Student.Http;

// У каждого предмета свой именованный HttpClient с адресом его преподавателя.
public class HttpTeachers : ITeachers
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpTeachers(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public static string GetClientName(Subject subject)
    {
        return $"teacher-{subject}";
    }

    /// <exception cref="HttpRequestException">Преподаватель недоступен, ответил ошибкой или не ответил вовремя.</exception>
    public async Task<bool> HoldLessonAsync(Subject subject, LessonRequest request, CancellationToken cancellationToken)
    {
        HttpClient http = _httpClientFactory.CreateClient(GetClientName(subject));
        string teacher = $"Преподаватель «{Subjects.GetName(subject)}» ({http.BaseAddress})";

        try
        {
            return await SendAsync(http, request, cancellationToken);
        }
        catch (HttpRequestException e)
        {
            // Сама причина (отказ в соединении, обрыв) лежит во вложенном исключении.
            throw new HttpRequestException($"{teacher}: {e.GetBaseException().Message}", e, e.StatusCode);
        }
        catch (TaskCanceledException e) when (!cancellationToken.IsCancellationRequested)
        {
            // Таймаут HttpClient выглядит как отмена, хотя её никто не просил.
            throw new HttpRequestException($"{teacher} не ответил за {http.Timeout.TotalSeconds} с", e);
        }
    }

    private static async Task<bool> SendAsync(HttpClient http, LessonRequest request, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await http.PostAsJsonAsync(TeacherRoutes.Lessons, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        LessonResponse lesson = await response.Content.ReadFromJsonAsync<LessonResponse>(cancellationToken)
            ?? throw new HttpRequestException("пустой ответ");

        return lesson.Asked;
    }
}
