namespace InkRecognition;

public sealed partial class LegacyEnglishLkSparseLanguageData
{
    // Immutable scalar reader. No Packed, owner array, memory or span escapes;
    // only NEW Node0Handles' private readonly field retains this returned value.
    internal readonly struct Node0FirstPackedReader
    {
        private readonly LegacyEnglishLkSparseLanguageData owner;
        private readonly Packed words,offsets,labels;
        private Node0FirstPackedReader(LegacyEnglishLkSparseLanguageData owner,Packed words,Packed offsets,Packed labels)
        {this.owner=owner;this.words=words;this.offsets=offsets;this.labels=labels;}
        internal static Node0FirstPackedReader Prepare(LegacyEnglishLkSparseLanguageData owner)
        {
            ArgumentNullException.ThrowIfNull(owner);
            // Exactly three BCL lookups at explicit NEW Node0 preparation,
            // before publication. Their Prepare work remains an explicit bound
            // frontier; no lookup is hidden inside a runtime scalar accessor.
            var w=owner.packed["88"];var o=owner.packed["B8.0"];var l=owner.packed["B0.1"];
            if(w.Count!=260075||w.Width!=10||o.Count!=1025||o.Width!=17||l.Count!=113643||l.Width!=10)
                throw new InvalidDataException("Exact owned FIRST sparse packed fields required.");
            return new(owner,w,o,l);
        }
        internal bool IsFor(LegacyEnglishLkSparseLanguageData actual)=>ReferenceEquals(owner,actual);
        internal int ReadWordClass(int index)=>owner.PackedValue(words,index);
        internal int ReadOrder1Offset(int index)=>owner.PackedValue(offsets,index);
        internal int ReadOrder2Label(int index)=>owner.PackedValue(labels,index);
    }
}
