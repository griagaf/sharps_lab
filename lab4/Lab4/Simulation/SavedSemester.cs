using Lab4.Randomness;

namespace Lab4.Simulation;

// Семестр, который ведётся в БД: номер записи и всё, что нужно, чтобы сохранять его дни.
public record SavedSemester(int Id, Semester Semester, SystemRandomSource Random);
