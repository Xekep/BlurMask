namespace InkRecognition;

//Friend-only observations of an actually scheduled call. This seam cannot
//force a native gate, alter owning heap storage or create a successful result.
internal sealed record LegacyMainSoftHyphenObservedCall(int EndingNode, byte Span,
    int PreviousEndingNode, ushort ParentOrdinal, ReadOnlyMemory<byte> Parent88,
    int BeforeCount, ReadOnlyMemory<byte> BeforeHeap128,
    int AfterCount, ReadOnlyMemory<byte> AfterHeap128,
    LegacyNativeSoftHyphenWordPrepassResult Result);
