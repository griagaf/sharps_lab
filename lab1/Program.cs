using System.Text;
using Lab1;

Console.OutputEncoding = Encoding.UTF8;

var random = new Random();
ITeacherRule[] teacherRules = TeacherRules.CreateForAllSubjects(random);
ISkipStrategy strategy = new EvenDaysStrategy();

Console.WriteLine($"Стратегия: {strategy.Name}");
Console.WriteLine();
Console.WriteLine("Правила преподавателей:");

foreach (Subject subject in Subjects.All)
{
    Console.WriteLine($"  {Subjects.GetName(subject),-22} {teacherRules[(int)subject].Description}");
}

Console.WriteLine();

var semester = new Semester(teacherRules, strategy);
SemesterResult result = semester.Run();

if (result.IsExpelled)
{
    Console.WriteLine($"Студент вылетел на {result.DaysPassed}-й день: прогулял «{Subjects.GetName(result.ExpelledSubject)}», а там спросили");
}
else
{
    Console.WriteLine("Студент дожил до конца семестра");
}

Console.WriteLine($"Общее удовольствие за семестр: {result.TotalPleasure}");
Console.WriteLine($"Среднее удовольствие в день: {result.AveragePleasurePerDay:F2}");
