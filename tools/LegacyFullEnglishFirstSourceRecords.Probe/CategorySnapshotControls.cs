using InkRecognition;
using System.Runtime.InteropServices;
using System.Buffers.Binary;
using System.Security.Cryptography;
namespace FullFirstSourceRecordsProbe;

//NOT EXECUTED. Future friend probe only, reuses one completed genuine B7.
internal static class CategorySnapshotControls
{
    internal sealed record MutationProof(string Before,string Mutated,string Restored,string BaselineRows,string PreparedBeforeRows,string PreparedAfterRows);
    internal static MutationProof? LastProof;
    private static string ProfileHash(LegacyCursiveCategoryProfile profile)
    {
        using var h=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);byte[] scalar=new byte[4];
        foreach(sbyte x in profile.StartGeometryMap.Span){scalar[0]=unchecked((byte)x);h.AppendData(scalar.AsSpan(0,1));}foreach(sbyte x in profile.EndGeometryMap.Span){scalar[0]=unchecked((byte)x);h.AppendData(scalar.AsSpan(0,1));}
        foreach(var p in profile.Prototypes.Span){foreach(float x in new[]{p.First,p.Second,p.Third}){BinaryPrimitives.WriteInt32LittleEndian(scalar,BitConverter.SingleToInt32Bits(x));h.AppendData(scalar);}scalar[0]=p.Flag?(byte)1:(byte)0;h.AppendData(scalar.AsSpan(0,1));}
        return Convert.ToHexString(h.GetHashAndReset()).ToLowerInvariant();
    }
    internal static string Run(LegacyFullEnglishFirstGlyphEmission cached)
    {
        LastProof=null;GuardAdmission.Require(cached);var child=cached.CopyCompletedChildren()[0];var exposed=child.Original.Factory.Categories;
        if(!MemoryMarshal.TryGetArray(exposed.StartGeometryMap,out ArraySegment<sbyte> start)||start.Array is null||
           !MemoryMarshal.TryGetArray(exposed.EndGeometryMap,out ArraySegment<sbyte> end)||end.Array is null||
           !MemoryMarshal.TryGetArray(exposed.Prototypes,out ArraySegment<LegacyCursiveCategoryPrototype> proto)||proto.Array is null)
            throw new InvalidDataException("Actual exposed category arrays required; no extra-copy mutation test.");
        var savedStart=exposed.StartGeometryMap.ToArray();var savedEnd=exposed.EndGeometryMap.ToArray();var savedProto=exposed.Prototypes.ToArray();
        var baseline=LegacyFullEnglishFirstSourceRecords.Prepare(cached);
        if(baseline.OrdinaryReferenceCount>64||baseline.ProjectedRangeFragments>512)
            throw new NotSupportedException("Category guard runs only bounded small actual cached fixtures.");
        //Precharge all THREE future stages plus detached snapshots. These are
        //diagnostic logical bounds separate from a production Run, not RSS.
        if(checked(3*baseline.AdditionalLogicalPeakBytes+1024*1024)>128L*1024*1024||
           checked(3*baseline.LogicalCopyWorkBytes+1024*1024)>256L*1024*1024||
           checked(3*baseline.LogicalVisitWork+1024*1024)>256L*1024*1024)
            throw new NotSupportedException("Category guard diagnostic envelope.");
        string before=ProfileHash(exposed),mutated="",restored="",rows0="",rows1="",rows2="";baseline.Run();rows0=SourceExports.HashRows(baseline);var preparedBeforeMutation=LegacyFullEnglishFirstSourceRecords.Prepare(cached);
        int nn=child.Original.Factory.Candidates.Child.Owner.NetworkEvaluationCount;
        try
        {
            for(int i=0;i<start.Count;i++)start.Array[start.Offset+i]=-1;
            for(int i=0;i<end.Count;i++)end.Array[end.Offset+i]=-1;
            proto.Array[proto.Offset]=proto.Array[proto.Offset] with{First=17.25f,Second=23.5f};
            if(exposed.StartGeometryMap.Span.SequenceEqual(savedStart)||exposed.Prototypes.Span.SequenceEqual(savedProto))
                throw new InvalidDataException("Must change the ACTUAL returned backing, not a detached copy.");
            mutated=ProfileHash(exposed);if(mutated==before)throw new InvalidDataException("Actual profile semantic bytes did not change");
            var preparedAfterMutation=LegacyFullEnglishFirstSourceRecords.Prepare(cached);
            preparedBeforeMutation.Run();preparedAfterMutation.Run();
            Equal(baseline,preparedBeforeMutation);Equal(baseline,preparedAfterMutation);rows1=SourceExports.HashRows(preparedBeforeMutation);rows2=SourceExports.HashRows(preparedAfterMutation);
            if(child.Original.Factory.Candidates.Child.Owner.NetworkEvaluationCount!=nn)throw new InvalidDataException("No NN replay for category guard.");
        }
        finally
        {
            savedStart.CopyTo(start.Array,start.Offset);savedEnd.CopyTo(end.Array,end.Offset);savedProto.CopyTo(proto.Array,proto.Offset);
            if(!exposed.StartGeometryMap.Span.SequenceEqual(savedStart)||!exposed.EndGeometryMap.Span.SequenceEqual(savedEnd)||!exposed.Prototypes.Span.SequenceEqual(savedProto))
                throw new InvalidDataException("Actual category backing restoration failed.");
            restored=ProfileHash(exposed);if(restored!=before)throw new InvalidDataException("Actual category semantic bit restoration failed");
        }
        LastProof=new(before,mutated,restored,rows0,rows1,rows2);
        return "actual-category-ROM-mutated-before-and-after-Prepare/raw-source-snapshot-stable/restored/noNN";
    }
    private static void Equal(LegacyFullEnglishFirstSourceRecords expected,LegacyFullEnglishFirstSourceRecords actual)
    {
        if(!actual.IsComplete||actual.IsFaulted)throw new InvalidDataException("Complete owned B8 category guard required.");
        var a=expected.CopyRows();var b=actual.CopyRows();if(a.Count!=b.Count)throw new InvalidDataException("Category guard row count.");
        for(int i=0;i<a.Count;i++)
        {
            if(!ReferenceEquals(a[i].Owner,expected)||!ReferenceEquals(b[i].Owner,actual)||!ReferenceEquals(a[i].Child,b[i].Child)||
               a[i].Reference!=b[i].Reference||a[i].WordOrdinal!=b[i].WordOrdinal||a[i].ReferenceOrdinal!=b[i].ReferenceOrdinal||
               !a[i].Record.CopyStorage().Span.SequenceEqual(b[i].Record.CopyStorage().Span)||
               !a[i].Record.Ranges.SequenceEqual(b[i].Record.Ranges)||!a[i].CopyChildRanges().AsSpan().SequenceEqual(b[i].CopyChildRanges()))
                throw new InvalidDataException("Recovered caller category memory affected actual owned720.");
        }
        var x=expected.CopyDescriptors();var y=actual.CopyDescriptors();if(x.Length!=y.Length)throw new InvalidDataException("Category guard descriptor count.");
        for(int i=0;i<x.Length;i++)if(!x[i].AsSpan().SequenceEqual(y[i]))throw new InvalidDataException("Category guard descriptor change.");
    }
}
