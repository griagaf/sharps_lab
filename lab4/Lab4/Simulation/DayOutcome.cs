using Lab4.Domain;

namespace Lab4.Simulation;

/// <param name="GoToLesson">Решение студента по каждому предмету: пошёл (true) или прогулял.</param>
/// <param name="Asked">Спросил ли преподаватель по каждому предмету — и там, где студента не было.</param>
/// <param name="SkippedSubjects">Прогулы этого дня. При вылете — только до него включительно.</param>
public record DayOutcome(int Day, bool[] GoToLesson, bool[] Asked, List<Subject> SkippedSubjects);
