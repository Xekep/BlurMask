namespace InkRecognition;
public sealed partial class LegacyNativeOrdinaryParentWordCandidates
{
    internal static LegacyNativeOrdinaryParentWordCandidates ForEnglishNode1(LegacyEnglishCaseNode1Geometry geometry,
        LegacySymbolContextFactory contexts,LegacyNativeWordAcousticRequestScorer scorer,LegacyNativeWordTransitionCostCell cell)
    {
        ArgumentNullException.ThrowIfNull(geometry);ArgumentNullException.ThrowIfNull(contexts);
        return new(LegacyOrdinaryRecognitionGraph.FromSameSource(geometry.ActualGraph),geometry.Alphabet,geometry.Categories,
            contexts,geometry.Operations(),scorer,cell,new byte[80],null);
    }
}
