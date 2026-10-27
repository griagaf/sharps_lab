using Lab3.Domain;
using Lab3.Simulation;
using Lab3.Students;
using Lab3.Teachers;
using Lab3.Tests.Fakes;

namespace Lab3.Tests.Simulation;

public class SemesterFactoryTests
{
    private static Semester CreateSemester(int observationDays, params int[] randomNumbers)
    {
        var random = new FakeRandomSource(randomNumbers);
        var factory = new SemesterFactory(new TeacherRuleGenerator(random), new EvenDaysStrategy(observationDays));
        return factory.CreateSemester();
    }

    [Fact]
    public void CreateSemester_UsesGeneratedRules()
    {
        Semester semester = CreateSemester(20, 0, 1, 0, 2, 1, 2, 0, 0, 0);

        Assert.IsType<RandomRule>(semester.TeacherRules[0]);
        Assert.IsType<CopyYesterdayRule>(semester.TeacherRules[1]);
        Assert.IsType<XorYesterdayRule>(semester.TeacherRules[2]);
        Assert.Equal(0, semester.DaysPassed);
    }

    [Fact]
    public void WholeSemester_IsPredictableWithFakeRandom()
    {
        // 6 нулей: все преподаватели спрашивают случайно, дальше по 6 чисел на день (0 = спросить)
        Semester semester = CreateSemester(
            1,
            0, 0, 0, 0, 0, 0,
            1, 1, 1, 1, 1, 1,
            0, 1, 1, 1, 1, 1,
            0, 1, 0, 1, 1, 1);

        // день 1: наблюдает, ходит на всё, никого не спросили
        List<Subject> firstDay = semester.PlayNextDay();
        Assert.Empty(firstDay);
        Assert.Equal(1, semester.TotalPleasure);

        // день 2: чётный, ходит на всё, спросили по матану
        List<Subject> secondDay = semester.PlayNextDay();
        Assert.Empty(secondDay);
        Assert.Equal(2, semester.TotalPleasure);

        // день 3: нечётный, идёт только на матан, а спросили ещё и по дискре
        List<Subject> thirdDay = semester.PlayNextDay();
        Assert.Equal(new[] { Subject.LinearAlgebra, Subject.DiscreteMath }, thirdDay);
        Assert.True(semester.IsExpelled);
        Assert.Equal(Subject.DiscreteMath, semester.ExpelledSubject);
        Assert.Equal(0, semester.TotalPleasure);
        Assert.Equal(3, semester.DaysPassed);
    }

    [Fact]
    public void SameRandomNumbers_GiveSameSemester()
    {
        int[] numbers = { 1, 2, 2, 0, 5, 0, 1, 3, 2, 4, 1, 0 };
        var dayNumbers = new List<int>(numbers);

        for (int i = 0; i < 40 * 2; i++)
        {
            dayNumbers.Add(i % 3 == 0 ? 0 : 1);
        }

        Semester first = CreateSemester(5, dayNumbers.ToArray());
        Semester second = CreateSemester(5, dayNumbers.ToArray());

        for (int day = 1; day <= 40 && !first.IsExpelled; day++)
        {
            Assert.Equal(first.PlayNextDay(), second.PlayNextDay());
            Assert.Equal(first.TotalPleasure, second.TotalPleasure);
            Assert.Equal(first.IsExpelled, second.IsExpelled);
        }

        Assert.Equal(first.DaysPassed, second.DaysPassed);
    }
}
