using Lab4.Domain;
using Lab4.Persistence;
using Lab4.Persistence.Records;
using Lab4.Randomness;
using Lab4.Teachers;

namespace Lab4.Simulation;

// Переводит семестр в записи для БД и обратно.
public class SemesterService
{
    private readonly SemesterRepository _repository;
    private readonly SemesterFactory _semesterFactory;

    public SemesterService(SemesterRepository repository, SemesterFactory semesterFactory)
    {
        _repository = repository;
        _semesterFactory = semesterFactory;
    }

    public async Task<SavedSemester> StartNewAsync()
    {
        var random = new SystemRandomSource();
        Semester semester = _semesterFactory.CreateSemester(random);

        var record = new SemesterRecord { Seed = random.Seed, RandomDraws = random.Draws };

        foreach (Subject subject in Subjects.All)
        {
            record.Rules.Add(ToRecord(subject, semester.TeacherRules[(int)subject]));
        }

        int semesterId = await _repository.CreateAsync(record);
        return new SavedSemester(semesterId, semester, random);
    }

    /// <returns>Последний незавершённый семестр или null, если продолжать нечего.</returns>
    /// <exception cref="InvalidDataException">Сохранённые данные противоречат друг другу.</exception>
    public async Task<SavedSemester?> ContinueUnfinishedAsync()
    {
        int? semesterId = await _repository.FindUnfinishedAsync(Semester.DaysInSemester);

        if (semesterId is null)
        {
            return null;
        }

        SemesterRecord record = await _repository.LoadAsync(semesterId.Value)
            ?? throw new InvalidDataException($"Семестр #{semesterId} пропал из БД");

        // Рандом продолжает с того места, где остановился после последнего сохранённого дня.
        int draws = record.Days.Count == 0 ? record.RandomDraws : record.Days[^1].RandomDraws;
        var random = new SystemRandomSource(record.Seed, draws);

        Semester semester = _semesterFactory.CreateSemester(CreateRules(record.Rules, random));

        foreach (DayRecord day in record.Days)
        {
            ReplayDay(semester, day);
        }

        return new SavedSemester(semesterId.Value, semester, random);
    }

    public Task SaveDayAsync(SavedSemester saved, DayOutcome outcome)
    {
        var day = new DayRecord
        {
            Day = outcome.Day,
            TotalPleasure = saved.Semester.TotalPleasure,
            IsExpelled = saved.Semester.IsExpelled,
            GoToLesson = outcome.GoToLesson,
            Asked = outcome.Asked,
            RandomDraws = saved.Random.Draws,
        };

        return _repository.SaveDayAsync(saved.Id, day);
    }

    private static TeacherRuleRecord ToRecord(Subject subject, ITeacherRule rule)
    {
        return rule switch
        {
            RandomRule => new TeacherRuleRecord { Subject = subject.ToString(), Kind = "Random" },
            CopyYesterdayRule copy => new TeacherRuleRecord
            {
                Subject = subject.ToString(),
                Kind = "CopyYesterday",
                SubjectA = copy.SubjectA.ToString(),
            },
            XorYesterdayRule xor => new TeacherRuleRecord
            {
                Subject = subject.ToString(),
                Kind = "XorYesterday",
                SubjectA = xor.SubjectA.ToString(),
                SubjectB = xor.SubjectB.ToString(),
            },
            _ => throw new ArgumentException($"Правило {rule.GetType().Name} нельзя сохранить в БД"),
        };
    }

    private static ITeacherRule[] CreateRules(List<TeacherRuleRecord> records, IRandomSource random)
    {
        var rules = new ITeacherRule?[Subjects.Count];

        foreach (TeacherRuleRecord record in records)
        {
            rules[(int)ParseSubject(record.Subject)] = record.Kind switch
            {
                "Random" => new RandomRule(random),
                "CopyYesterday" => new CopyYesterdayRule(ParseSubject(record.SubjectA)),
                "XorYesterday" => new XorYesterdayRule(ParseSubject(record.SubjectA), ParseSubject(record.SubjectB)),
                _ => throw new InvalidDataException($"Неизвестный вид правила «{record.Kind}»"),
            };
        }

        if (rules.Any(rule => rule is null))
        {
            throw new InvalidDataException("В БД сохранены правила не для всех предметов");
        }

        return rules!;
    }

    private static void ReplayDay(Semester semester, DayRecord day)
    {
        if (day.Day != semester.DaysPassed + 1)
        {
            throw new InvalidDataException($"После дня {semester.DaysPassed} сохранён день {day.Day}");
        }

        try
        {
            semester.ReplayDay(day.GoToLesson, day.Asked);
        }
        catch (Exception e) when (e is ArgumentException or InvalidOperationException)
        {
            throw new InvalidDataException($"День {day.Day} нельзя повторить: {e.Message}", e);
        }

        if (semester.TotalPleasure != day.TotalPleasure || semester.IsExpelled != day.IsExpelled)
        {
            throw new InvalidDataException($"День {day.Day}: сохранённый прогресс не сходится с решениями и вопросами");
        }
    }

    private static Subject ParseSubject(string? name)
    {
        if (Enum.TryParse(name, out Subject subject) && Enum.IsDefined(subject))
        {
            return subject;
        }

        throw new InvalidDataException($"Неизвестный предмет «{name}»");
    }
}
