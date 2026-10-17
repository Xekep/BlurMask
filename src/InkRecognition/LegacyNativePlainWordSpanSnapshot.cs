using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyMainWordSpanStorage.Probe")]
namespace InkRecognition;

/// <summary>Internal current275 payloads and actual279 selections. It is not
/// source/V68 completion. A ranking attempt consumes the snapshot even if a
/// later validation or cancellation fails; no retry or rollback is invented.</summary>
internal sealed class LegacyNativePlainWordSpanSnapshot
{
    private readonly LegacyNativePlainWordPublisher publisher;
    private readonly object nativeOwner;
    private int applyEntered;
    internal LegacyNativeWordSourceSpan Span {get;}
    internal IReadOnlyList<LegacyNativePlainWordRecord> Words {get;}
    internal IReadOnlyList<LegacyNativePublishedWordNode> SelectedNodes {get;}
    internal IReadOnlyList<int> SelectedOrdinals {get;}
    private readonly List<LegacyNativePlainWordRecord> nativeList;
    internal LegacyNativePlainWordSpanSnapshot(LegacyNativePlainWordPublisher publisher,object nativeOwner,
        LegacyNativeWordSourceSpan span,List<LegacyNativePlainWordRecord> list,
        LegacyNativePlainWordRecord[] rows,LegacyNativePublishedWordNode[] selected)
    {
        this.publisher=publisher;this.nativeOwner=nativeOwner;Span=span;nativeList=list;
        Words=Array.AsReadOnly(rows);SelectedNodes=Array.AsReadOnly(selected);
        SelectedOrdinals=Array.AsReadOnly(selected.Select(n=>n.WordIndex).ToArray());
    }
    internal bool RetainsNativeList(List<LegacyNativePlainWordRecord> actual)=>ReferenceEquals(nativeList,actual);
    internal void BeginApply(LegacyNativePlainWordPublisher actualPublisher,object actualNativeOwner)
    {
        if(!ReferenceEquals(publisher,actualPublisher)||!ReferenceEquals(nativeOwner,actualNativeOwner))
            throw new ArgumentException("The actual publisher and same-TOP span owner must be retained.");
        if(Interlocked.CompareExchange(ref applyEntered,1,0)!=0)
            throw new InvalidOperationException("A275 ranking snapshot cannot be retried after an entered attempt.");
    }
}
