using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathUnusedUnitPruning.Probe")]

namespace InkRecognition.Math;

/// <summary>Prepared finite041660 assignment filtering and unused-unit removal
/// with the actual fingerprinted EF3 gate. No auxiliary98 bytes or completed
/// record are mutated; request/correction ownership remains separate.</summary>
internal sealed class LegacyPreparedMathUnusedUnitPruning
{
    private readonly uint[] units;
    private readonly int[] assignments, counts, oldToNew, suppressed;
    private readonly byte[] deletions;
    internal uint[] CopyUnits() => units.ToArray();
    internal int[] CopyAssignments() => assignments.ToArray();
    internal int[] CopyUseCounts() => counts.ToArray();
    internal int[] CopyOldToNew() => oldToNew.ToArray();
    internal int[] CopySuppressedSegments() => suppressed.ToArray();
    internal byte[] CopyDeletionMask() => deletions.ToArray();
    // The outer EF4 caller rejects removal where its earlier insertion mask
    // was nonzero. A computed pruning result alone is not acceptance.
    internal bool PreservesInsertedUnitsPrepared(ReadOnlySpan<byte> insertionMask,
        CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(insertionMask.Length!=deletions.Length)
            throw new ArgumentException("Complete original-coordinate insertion mask required.");
        for(int i=0;i<deletions.Length;i++)
        {
            token.ThrowIfCancellationRequested();
            if(deletions[i]!=0&&insertionMask[i]!=0)return false;
        }
        return true;
    }
    private LegacyPreparedMathUnusedUnitPruning(uint[] units, int[] assignments,
        int[] counts, int[] oldToNew, int[] suppressed, byte[] deletions)
    { this.units=units;this.assignments=assignments;this.counts=counts;
      this.oldToNew=oldToNew;this.suppressed=suppressed;this.deletions=deletions; }

    internal static LegacyPreparedMathUnusedUnitPruning EvaluatePrepared(
        ReadOnlySpan<uint> sourceUnits, ReadOnlySpan<int> sourceAssignments,
        ReadOnlySpan<long> strokeEnds, LegacyMath2ObservableTokens observable,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(observable);token.ThrowIfCancellationRequested();
        if(sourceUnits.Length is <1 or >1000 || sourceAssignments.Length is <1 or >6000 || strokeEnds.Length>6000)
            throw new ArgumentException("Bounded nonempty prepared units and complete assignments required.");
        foreach(int index in sourceAssignments)
            if(index<0||index>=sourceUnits.Length)
                throw new ArgumentException("041660 entry requires fully assigned valid original positions.");
        for(int i=0;i<strokeEnds.Length;i++)
            if(strokeEnds[i]<1||strokeEnds[i]>sourceAssignments.Length||i>0&&strokeEnds[i]<=strokeEnds[i-1])
                throw new ArgumentException("Ordered distinct exclusive stroke-end coordinates required.");
        var ends=strokeEnds.ToArray().ToHashSet();
        int[] assignments=sourceAssignments.ToArray(),counts=new int[sourceUnits.Length];
        var suppressed=new List<int>();int previous=-1;
        for(int segment=0;segment<assignments.Length;segment++)
        {
            token.ThrowIfCancellationRequested();int original=assignments[segment];
            bool end=segment+1==assignments.Length||ends.Contains(segment+1);
            bool begin=segment==0||ends.Contains(segment);
            if(original!=previous&&end&&!begin)
            {assignments[segment]=-1;suppressed.Add(segment);}
            else counts[original]++;
            previous=original; //Original predecessor, even when suppressed.
        }
        byte[] deletions=new byte[sourceUnits.Length];int[] map=new int[sourceUnits.Length];
        var retained=new List<uint>();
        for(int original=0;original<sourceUnits.Length;original++)
        {
            token.ThrowIfCancellationRequested();uint unit=sourceUnits[original];
            bool remove=counts[original]<1&&((unit&0x00ff0000u)!=0||observable.IsObservable((ushort)unit));
            deletions[original]=(byte)(remove?1:0);
            map[original]=remove?-1:retained.Count;
            if(!remove)retained.Add(unit);
        }
        for(int segment=0;segment<assignments.Length;segment++)
            if(assignments[segment]!=-1)assignments[segment]=map[assignments[segment]];
        token.ThrowIfCancellationRequested();
        return new(retained.ToArray(),assignments,counts,map,suppressed.ToArray(),deletions);
    }
}
