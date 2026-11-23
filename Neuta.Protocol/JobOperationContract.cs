using System.Collections.Immutable;

namespace Neuta.Protocol;

public enum JobOperation
{
    ConvertScore,
    PredictTiming,
    PrepareTrack,
    PredictF0,
    PredictMelSpec,
    SynthesizeWav,
    RenderPhrase
}

public sealed record JobOperationContract(
    ImmutableArray<string> InputFiles,
    ImmutableArray<string> OutputFiles,
    bool RequiresModel = true)
{
    private static readonly ImmutableArray<string> TimingOutputs = [
        ArtifactNames.FullLabel,
        ArtifactNames.TimingLabel,
        ArtifactNames.EnginePhraseList,
        ArtifactNames.PhraseIndex];

    private static readonly ImmutableArray<string> PhraseInputs = [
        ArtifactNames.FullLabel,
        ArtifactNames.TimingLabel,
        ArtifactNames.PhraseIndex];

    private static readonly ImmutableArray<string> PhraseInputsWithF0 = [
        ArtifactNames.FullLabel,
        ArtifactNames.TimingLabel,
        ArtifactNames.PhraseIndex,
        ArtifactNames.F0];

    private static readonly JobOperationContract ConvertScore =
        new([ArtifactNames.Score], [ArtifactNames.FullLabel, ArtifactNames.MonoLabel], RequiresModel: false);
    private static readonly JobOperationContract PredictTiming =
        new([ArtifactNames.FullLabel], TimingOutputs);
    private static readonly JobOperationContract PrepareTrack =
        new([ArtifactNames.Score], TimingOutputs);
    private static readonly JobOperationContract PredictF0 =
        new(PhraseInputs, [ArtifactNames.F0]);
    private static readonly JobOperationContract PredictMelSpec =
        new(PhraseInputsWithF0, [ArtifactNames.MelSpec]);
    private static readonly JobOperationContract SynthesizeWav =
        new(PhraseInputsWithF0.Add(ArtifactNames.MelSpec), [ArtifactNames.Wav]);
    private static readonly JobOperationContract RenderPhrase = new(PhraseInputsWithF0, [ArtifactNames.Wav]);

    public bool RequiresPhraseNumber => InputFiles.Contains(ArtifactNames.PhraseIndex);
    public bool RequiresF0FrameCount => InputFiles.Contains(ArtifactNames.F0);
    public bool ProducesWav => OutputFiles.Contains(ArtifactNames.Wav);

    /// <summary>
    /// Returns the <see cref="JobOperationContract"/> for the specified <see cref="JobOperation"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static JobOperationContract For(JobOperation operation) => operation switch
    {
        JobOperation.ConvertScore => ConvertScore,
        JobOperation.PredictTiming => PredictTiming,
        JobOperation.PrepareTrack => PrepareTrack,
        JobOperation.PredictF0 => PredictF0,
        JobOperation.PredictMelSpec => PredictMelSpec,
        JobOperation.SynthesizeWav => SynthesizeWav,
        JobOperation.RenderPhrase => RenderPhrase,
        _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
    };
}
