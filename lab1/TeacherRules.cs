namespace Lab1;

public static class TeacherRules
{
    public static ITeacherRule[] CreateForAllSubjects(Random random)
    {
        var rules = new ITeacherRule[Subjects.Count];

        for (int i = 0; i < rules.Length; i++)
        {
            rules[i] = CreateRandomRule(random);
        }

        return rules;
    }

    private static ITeacherRule CreateRandomRule(Random random)
    {
        int ruleNumber = random.Next(1, 4);

        switch (ruleNumber)
        {
            case 1:
                return new RandomRule(random);

            case 2:
                return new CopyYesterdayRule(PickRandomSubject(random));

            default:
                Subject subjectA = PickRandomSubject(random);
                Subject subjectB = PickRandomSubject(random);

                while (subjectB == subjectA)
                {
                    subjectB = PickRandomSubject(random);
                }

                return new XorYesterdayRule(subjectA, subjectB);
        }
    }

    private static Subject PickRandomSubject(Random random)
    {
        return Subjects.All[random.Next(Subjects.Count)];
    }
}
