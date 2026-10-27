using Lab3.Domain;
using Lab3.Simulation;
using Lab3.Students;
using Lab3.Teachers;
using Lab3.Tests.Fakes;

namespace Lab3.Tests.Simulation;

public class SemesterFactoryTests
{
    private static Semester CreateSemester(params int[] randomNumbers)
    {
        var random = new FakeRandomSource(randomNumbers);
        var factory = new SemesterFactory(new TeacherRuleGenerator(random), new EvenDaysStrategy());
        return factory.CreateSemester();
    }

    [Fact]
    public void CreateSemester_UsesGeneratedRules()
    {
        Semester semester = CreateSemester(0, 1, 0, 2, 1, 2, 0, 0, 0);

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
            0, 0, 0, 0, 0, 0,
            1, 1, 1, 1, 1, 1,
            0, 0, 0, 0, 0, 0,
            1, 1, 0, 1, 1, 1);

        // день 1: нечётный, прогуливает всё, никого не спросили
        List<Subject> firstDay = semester.PlayNextDay();
        Assert.Equal(Subjects.Count, firstDay.Count);
        Assert.Equal(Subjects.Count + 1, semester.TotalPleasure);

        // день 2: чётный, ходит на всё, поэтому вопросы по всем предметам не страшны
        List<Subject> secondDay = semester.PlayNextDay();
        Assert.Empty(secondDay);
        Assert.Equal(Subjects.Count + 1 + 1, semester.TotalPleasure);

        // день 3: снова прогуливает всё, а по дискре спросили
        semester.PlayNextDay();
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

        Semester first = CreateSemester(dayNumbers.ToArray());
        Semester second = CreateSemester(dayNumbers.ToArray());

        for (int day = 1; day <= 40 && !first.IsExpelled; day++)
        {
            Assert.Equal(first.PlayNextDay(), second.PlayNextDay());
            Assert.Equal(first.TotalPleasure, second.TotalPleasure);
            Assert.Equal(first.IsExpelled, second.IsExpelled);
        }

        Assert.Equal(first.DaysPassed, second.DaysPassed);
    }
}
