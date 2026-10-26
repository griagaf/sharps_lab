namespace Lab2;

public class TeacherRuleGenerator
{
    private readonly IRandomSource _random;

    public TeacherRuleGenerator(IRandomSource random)
    {
        _random = random;
    }

    public ITeacherRule[] CreateForAllSubjects()
    {
        var rules = new ITeacherRule[Subjects.Count];

        for (int i = 0; i < rules.Length; i++)
        {
            rules[i] = CreateRandomRule();
        }

        return rules;
    }

    private ITeacherRule CreateRandomRule()
    {
        int ruleNumber = _random.Next(3) + 1;

        switch (ruleNumber)
        {
            case 1:
                return new RandomRule(_random);

            case 2:
                return new CopyYesterdayRule(PickRandomSubject());

            default:
                Subject subjectA = PickRandomSubject();
                Subject subjectB = PickRandomSubject();

                while (subjectB == subjectA)
                {
                    subjectB = PickRandomSubject();
                }

                return new XorYesterdayRule(subjectA, subjectB);
        }
    }

    private Subject PickRandomSubject()
    {
        return Subjects.All[_random.Next(Subjects.Count)];
    }
}
