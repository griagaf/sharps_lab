using Lab3.Domain;

namespace Lab3.Tests;

// Массивы bool по предметам: для решений стратегии и для "кого спрашивали вчера".
public static class Flags
{
    public static bool[] None()
    {
        return new bool[Subjects.Count];
    }

    public static bool[] All()
    {
        return AllExcept();
    }

    public static bool[] Only(params Subject[] subjects)
    {
        var flags = new bool[Subjects.Count];

        foreach (Subject subject in subjects)
        {
            flags[(int)subject] = true;
        }

        return flags;
    }

    public static bool[] AllExcept(params Subject[] subjects)
    {
        var flags = new bool[Subjects.Count];

        for (int i = 0; i < flags.Length; i++)
        {
            flags[i] = !subjects.Contains((Subject)i);
        }

        return flags;
    }
}
