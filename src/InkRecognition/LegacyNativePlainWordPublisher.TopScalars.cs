[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("LegacyEnglishTopScalars.Probe")]
namespace InkRecognition;

public sealed partial class LegacyNativePlainWordPublisher
{
    //Allocation-free validator for the privately constructed, never-forked
    //single-publisher TOP domain. It authenticates the actual selected275 row,
    //not merely its ordinal. No words/hashsets/selected snapshots are created.
    internal (int Count,int Selected) ReadFreshSingletonTopScalars(
        LegacyNativePublishedWordNode actual,LegacyNativeGlyphRecordPool expectedPool,
        CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(actual);ArgumentNullException.ThrowIfNull(expectedPool);
        if(released||!ReferenceEquals(pool,expectedPool)||interning.Publishers.Count!=1||
            !ReferenceEquals(interning.Publishers[0],this)||interning.Spans.Count!=1||!ReferenceEquals(Head,actual)||
            actual.Next is not null||!ReferenceEquals(actual.Word.Pool,pool)||
            !interning.Spans.TryGetValue(actual.Span,out var words)||words.Count is<1 or>4095||
            (uint)actual.WordIndex>=(uint)words.Count||!ReferenceEquals(words[actual.WordIndex],actual.Word))
            throw new InvalidOperationException("Same fresh single-publisher TOP/head/pool/selected275 required.");
        int count=words.Count,selected=actual.WordIndex;
        token.ThrowIfCancellationRequested();return(count,selected);
    }
}
