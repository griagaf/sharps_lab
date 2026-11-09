using Lab6.Contracts;

namespace Lab6.Student.Simulation;

/// <summary>Преподаватели всех предметов: к кому студент приходит на пару или кого прогуливает.</summary>
public interface ITeachers
{
    /// <returns>Спросил ли преподаватель предмета на этой паре.</returns>
    Task<bool> HoldLessonAsync(Subject subject, LessonRequest request, CancellationToken cancellationToken);
}
