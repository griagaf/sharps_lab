using Lab4.Persistence;
using Lab4.Simulation;
using Lab4.Students;

namespace Lab4.Tests.Integration;

public class SemesterServiceTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _postgres;

    public SemesterServiceTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    // Новый сервис на каждый вызов — как будто программу перезапустили.
    // Первые 20 дней стратегия ходит на всё, так что до 20-го дня студент точно не вылетит.
    private SemesterService CreateService()
    {
        return new SemesterService(new SemesterRepository(_postgres.DataSource), new SemesterFactory(new EvenDaysStrategy()));
    }

    private static async Task PlayDaysAsync(SemesterService service, SavedSemester saved, int days)
    {
        for (int i = 0; i < days && !saved.Semester.IsFinished; i++)
        {
            DayOutcome outcome = saved.Semester.PlayNextDay();
            await service.SaveDayAsync(saved, outcome);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(15)]
    public async Task SavedOnDay_IsRestoredIdentically(int day)
    {
        SemesterService service = CreateService();
        SavedSemester original = await service.StartNewAsync();
        await PlayDaysAsync(service, original, day);

        SavedSemester? restored = await CreateService().ContinueUnfinishedAsync();

        Assert.NotNull(restored);
        Assert.Equal(original.Id, restored.Id);
        Assert.Equal(original.Semester.DaysPassed, restored.Semester.DaysPassed);
        Assert.Equal(original.Semester.TotalPleasure, restored.Semester.TotalPleasure);
        Assert.Equal(original.Semester.IsExpelled, restored.Semester.IsExpelled);
        Assert.Equal(original.Random.Seed, restored.Random.Seed);
        Assert.Equal(original.Random.Draws, restored.Random.Draws);
        Assert.Equal(
            original.Semester.TeacherRules.Select(rule => rule.Description),
            restored.Semester.TeacherRules.Select(rule => rule.Description));
    }

    [Fact]
    public async Task ContinuedAfterRestore_GoesLikeUninterrupted()
    {
        SemesterService service = CreateService();
        SavedSemester original = await service.StartNewAsync();
        await PlayDaysAsync(service, original, 15);

        SavedSemester? restored = await CreateService().ContinueUnfinishedAsync();
        Assert.NotNull(restored);

        // Дальше оба идут только в памяти: original — это прогон без остановки.
        while (!original.Semester.IsFinished)
        {
            DayOutcome expected = original.Semester.PlayNextDay();
            DayOutcome actual = restored.Semester.PlayNextDay();

            Assert.Equal(expected.GoToLesson, actual.GoToLesson);
            Assert.Equal(expected.Asked, actual.Asked);
        }

        Assert.True(restored.Semester.IsFinished);
        Assert.Equal(original.Semester.DaysPassed, restored.Semester.DaysPassed);
        Assert.Equal(original.Semester.TotalPleasure, restored.Semester.TotalPleasure);
    }

    [Fact]
    public async Task Continue_SkipsFinishedSemesters()
    {
        SemesterService service = CreateService();
        SavedSemester unfinished = await service.StartNewAsync();
        await PlayDaysAsync(service, unfinished, 3);

        SavedSemester finished = await service.StartNewAsync();
        await PlayDaysAsync(service, finished, Semester.DaysInSemester);
        Assert.True(finished.Semester.IsFinished);

        SavedSemester? continued = await CreateService().ContinueUnfinishedAsync();

        Assert.NotNull(continued);
        Assert.Equal(unfinished.Id, continued.Id);
    }
}
