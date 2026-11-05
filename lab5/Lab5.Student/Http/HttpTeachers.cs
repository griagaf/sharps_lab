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

    public async Task<bool> HoldLessonAsync(Subject subject, LessonRequest request, CancellationToken cancellationToken)
    {
        HttpClient http = _httpClientFactory.CreateClient(GetClientName(subject));

        using HttpResponseMessage response = await http.PostAsJsonAsync(TeacherRoutes.Lessons, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        LessonResponse lesson = await response.Content.ReadFromJsonAsync<LessonResponse>(cancellationToken)
            ?? throw new HttpRequestException($"Преподаватель предмета {subject} вернул пустой ответ");

        return lesson.Asked;
    }
}
