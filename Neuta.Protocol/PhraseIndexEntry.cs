namespace Neuta.Protocol;

/// <summary>
/// A singing phrase in <c>phrases.json</c>. Phoneme indices are zero-based, with an exclusive end.
/// The label times are in 100 ns units.
/// </summary>
public sealed record PhraseIndexEntry(
    int EnginePhraseNumber,
    int StartPhonemeIndex,
    int EndPhonemeIndexExclusive,
    long StartTime100ns,
    long EndTime100ns);
