namespace Neuta.Protocol;

public static class ProtocolConstants
{
    /// <summary>
    /// The protocol version. Editor and agent must match to communicate.
    /// </summary>
    public const int Version = 1;
    /// <summary>
    /// Maximum number of UTF-16 characters in one NDJSON line.
    /// </summary>
    public const int MaxLineCharacters = 65_536;
    public const int MaxQueuedJobs = 64;
    public const int DefaultStartupTimeoutMs = 10_000;
    public const int DefaultJobTimeoutMs = 180_000;
    /// <summary>
    /// The frame period for F0. <c>f0.bin</c> uses a frame period of 10ms.
    /// </summary>
    public const int F0FramePeriod100ns = 100_000;
    public const int MelSpecValuesPerFrame = 100;

    public const string InputDirectory = "in";
    public const string WorkDirectory = "work";
    public const string OutputDirectory = "out";
}

public static class ArtifactNames
{
    public const string Score = "score.musicxml";
    public const string FullLabel = "full.lab";
    public const string MonoLabel = "mono.lab";
    public const string TimingLabel = "timing.lab";
    public const string EnginePhraseList = "phrase_list.txt";
    public const string PhraseIndex = "phrases.json";
    public const string F0 = "phrase.f0";
    public const string MelSpec = "phrase.melspec";
    public const string Wav = "phrase.wav";
}
