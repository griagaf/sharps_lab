namespace Lab6.Contracts;

/// <summary>Сообщение в очереди: преподаватель провёл пару и решил, спросить ли на ней.</summary>
public record LessonHeld(LessonKey Lesson, bool Asked);
