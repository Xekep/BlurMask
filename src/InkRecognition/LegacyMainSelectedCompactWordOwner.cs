using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyMainSecondaryWordPublication.Probe")]
namespace InkRecognition;

//Exactly one genuinely selected compact owner. This union does not construct
//a count1 terminal around a configured2410 winner.
internal sealed class LegacyMainSelectedCompactWordOwner
{
    internal LegacyOwnedMainCompactWordMaterialization? Single { get; }
    internal LegacyOwnedMainSecondaryCompactTerminal? Secondary { get; }
    internal LegacyOwnedFreshMainWordControllerPolicy Controller => Single is not null
        ? Single.TerminalResult.PassResult.ControllerOwner : Secondary!.Materialization.PassResult.SourcePolicy.Controller;
    internal LegacyOwnedRussianMainGlyphConfidence ConfidenceOwner => Single?.ConfidenceOwner ?? Secondary!.Materialization.ConfidenceOwner;
    internal LegacyFreshCaseMainSourcePolicy? CaseSourcePolicy => Single is not null
        ? Single.CaseSourcePolicy : Secondary!.Materialization.PassResult.SourcePolicy;
    internal int RecordCount => Single?.RecordCount ?? Secondary!.Materialization.RecordCount;
    internal int SelectedRecordIndex => Single?.SelectedRecordIndex ?? Secondary!.SelectedRecordIndex;
    //2271F0 explicitly passesFFFF through221840 to21FFD0 for each choice.
    //The initial cursor then loads compact1E;2410's selected slot is not this.
    internal ushort SelectedAlternative => ushort.MaxValue;
    internal LegacyPreparedMainTerminalWinner Winner => Single?.TerminalResult.Winner ?? Secondary!.Selection.Winner;
    internal object Identity => (object?)Single ?? Secondary!;
    private LegacyMainSelectedCompactWordOwner(LegacyOwnedMainCompactWordMaterialization? single,
        LegacyOwnedMainSecondaryCompactTerminal? secondary)
    { Single = single; Secondary = secondary; }
    internal static LegacyMainSelectedCompactWordOwner FromSingle(LegacyOwnedMainCompactWordMaterialization single)
    { ArgumentNullException.ThrowIfNull(single); return new(single, null); }
    internal static LegacyMainSelectedCompactWordOwner FromSecondary(LegacyOwnedMainSecondaryCompactTerminal secondary)
    { ArgumentNullException.ThrowIfNull(secondary); return new(null, secondary); }
    internal byte[] CopyCompactStorage() => Single?.CopyCompactStorage() ?? Secondary!.Materialization.CopyCompactStorage();
    internal int[] CopyNodeStarts2478() => Single?.CopyNodeStarts2478() ?? Secondary!.Materialization.CopyNodeStarts2478();
    internal int[] CopyCumulativePathCosts() => Single?.TerminalResult.PassResult.CopyCumulativePathCosts() ??
        Secondary!.Materialization.PassResult.CopyCumulativePathCosts();
}
