using Lab3.Domain;
using Lab3.Students;

namespace Lab3.Tests.Students;

public class StudentHistoryTests
{
    [Fact]
    public void AttendedAndAsked_IsRemembered()
    {
        var history = new StudentHistory();

        history.SaveLesson(1, Subject.Physics, attended: true, asked: true);

        Assert.True(history.Attended(1, Subject.Physics));
        Assert.True(history.WasAsked(1, Subject.Physics));
    }

    [Fact]
    public void AttendedAndNotAsked_IsRemembered()
    {
        var history = new StudentHistory();

        history.SaveLesson(1, Subject.Physics, attended: true, asked: false);

        Assert.True(history.Attended(1, Subject.Physics));
        Assert.False(history.WasAsked(1, Subject.Physics));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Skipped_StudentDoesNotKnowIfAsked(bool asked)
    {
        var history = new StudentHistory();

        history.SaveLesson(1, Subject.Physics, attended: false, asked: asked);

        Assert.False(history.Attended(1, Subject.Physics));
        Assert.Null(history.WasAsked(1, Subject.Physics));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(100)]
    public void UnknownDay_NothingIsKnown(int day)
    {
        var history = new StudentHistory();

        Assert.False(history.Attended(day, Subject.Calculus));
        Assert.Null(history.WasAsked(day, Subject.Calculus));
    }

    [Fact]
    public void DaysBeforeSavedDay_StayUnknown()
    {
        var history = new StudentHistory();

        history.SaveLesson(3, Subject.English, attended: true, asked: true);

        Assert.Null(history.WasAsked(1, Subject.English));
        Assert.Null(history.WasAsked(2, Subject.English));
        Assert.True(history.WasAsked(3, Subject.English));
    }

    [Fact]
    public void Subjects_AreStoredSeparately()
    {
        var history = new StudentHistory();

        history.SaveLesson(1, Subject.Calculus, attended: true, asked: true);
        history.SaveLesson(1, Subject.English, attended: true, asked: false);

        Assert.True(history.WasAsked(1, Subject.Calculus));
        Assert.False(history.WasAsked(1, Subject.English));
        Assert.Null(history.WasAsked(1, Subject.Physics));
    }
}
