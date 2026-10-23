using InkRecognition;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
namespace FullFirstSourceRecordsProbe;

//Additional per-parent diagnostic ledger; no NN buffers/full720 history retained.
internal sealed class SourceExports
{
    internal readonly long Peak=checked(12L*1024*1024+128L*12800+64L*8192+512L*256+720L*4096);
    internal readonly long Copy=checked(32L*1024*1024+512L*12800+256L*8192+1440L*4096+4096L*256+2048L*8192);
    internal SourceExports(){if(Peak>32L*1024*1024||Copy>128L*1024*1024)throw new InvalidDataException("Probe export preflight.");}
    internal static int[] Identity(LegacySourcePointIdentity p)=>[p.Key.First,p.Key.Second,p.Key.Third,p.MetadataPointIndex,p.KeyRecordIndex,p.RemainderByte];
    internal static int[] Range(LegacyFirstResultSourceRange r)=>[r.First.First,r.First.Second,r.First.Third,r.Last.First,r.Last.Second,r.Last.Third];
    internal static void Ints(Utf8JsonWriter w,IEnumerable<int> values){w.WriteStartArray();foreach(int x in values)w.WriteNumberValue(x);w.WriteEndArray();}
    internal static void Strokes(Utf8JsonWriter w,IReadOnlyList<InkStroke> values)
    {w.WriteStartArray();foreach(var s in values){w.WriteStartArray();foreach(var p in s.Points){w.WriteStartArray();w.WriteNumberValue(BitConverter.SingleToInt32Bits(p.X));w.WriteNumberValue(BitConverter.SingleToInt32Bits(p.Y));w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();}
    internal static void Parent(Utf8JsonWriter w,LegacyFullEnglishFirstParentInput a,LegacyFullEnglishFirstSourceRecords source)
    {
        w.WriteStartObject("Parent");w.WriteString("SourceFingerprint",a.Model.Source.Fingerprint.ToLowerInvariant());
        w.WriteNumber("Resolution",a.PhysicalResolution);w.WriteNumber("Exponent",a.Model.SourceScoreExponent);w.WriteNumber("ScalePrimary",a.SourceScales.Primary);w.WriteNumber("ScaleSecondary",a.SourceScales.Secondary);
        w.WritePropertyName("RawBits");Strokes(w,a.CopyRaw());w.WritePropertyName("RetainedBits");Strokes(w,a.CopyRetainedIngress());
        w.WritePropertyName("CurrentBits");Strokes(w,source.CopyCurrentSource());w.WritePropertyName("OriginalPhysical");Ints(w,a.CopyCurrentOriginalPhysical());
        w.WriteStartArray("Chronology");foreach(var x in source.CopyCurrentChronology())Ints(w,Identity(x));w.WriteEndArray();
        w.WriteStartArray("EndingChronology");for(int s=0;s<a.Current.Count;s++)for(int p=0;p<a.Current[s].Points.Count;p++)Ints(w,Identity(a.ResolveCurrentEndingMetadata(s,p)));w.WriteEndArray();
        w.WriteStartArray("Descriptors");foreach(byte[] b in source.CopyOriginalDescriptors())w.WriteBase64StringValue(b);w.WriteEndArray();w.WriteEndObject();
    }
    internal static void Children(Utf8JsonWriter w,LegacyFullEnglishFirstGlyphEmission pass)
    {
        w.WriteStartArray("B7Children");int rows=0,refs=0,words=0;
        foreach(var child in pass.CopyCompletedChildren())
        {
            var input=child.Original.Factory.Candidates.Child;w.WriteStartObject();w.WriteNumber("Descriptor",input.DescriptorOrdinal);w.WriteBoolean("SoleFlat",input.IsSoleFlat);
            w.WritePropertyName("Map");Ints(w,input.CopyCurrentParentMap());w.WritePropertyName("RawBits");Strokes(w,input.Submission.CopyRawChildInput());
            w.WriteStartArray("Owners");foreach(var n in child.CopyGlyphOwners())
            {rows=checked(rows+n.Records.Length/40);if(rows>12800)throw new InvalidDataException("Probe owner export cap.");w.WriteStartObject();w.WriteNumber("Key",n.Key);w.WriteNumber("PackedCount",n.PackedCount);w.WriteBase64String("Records",n.Records.Span);w.WriteEndObject();}w.WriteEndArray();
            w.WriteStartArray("Ranges");foreach(ulong key in child.CopySourceRangeKeys())
            {w.WriteStartObject();w.WriteNumber("Key",key);w.WriteStartArray("Rows");foreach(var r in child.CopySourceRanges(key))Ints(w,Range(r));w.WriteEndArray();w.WriteEndObject();}w.WriteEndArray();
            w.WriteStartArray("Words");foreach(var word in child.Words)
            {if(++words>8192)throw new InvalidDataException("Probe WORD export cap.");w.WriteStartObject();w.WriteBase64String("Storage",word.CopyStorage());var rr=word.CopyReadingOrderReferences();refs=checked(refs+rr.Length);if(refs>8192)throw new InvalidDataException("Probe ref export cap.");w.WriteStartArray("References");foreach(ulong r in rr)w.WriteNumberValue(r);w.WriteEndArray();w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
        }w.WriteEndArray();
    }
    internal static string HashB7(LegacyFullEnglishFirstGlyphEmission pass)
    {
        using var h=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);byte[] scalar=new byte[8];
        void U(ulong x){BinaryPrimitives.WriteUInt64LittleEndian(scalar,x);h.AppendData(scalar);}
        foreach(var child in pass.CopyCompletedChildren())
        {
            U((ulong)child.Original.Factory.Candidates.Child.DescriptorOrdinal);
            var owners=child.CopyGlyphOwners();U((ulong)owners.Count);
            foreach(var n in owners){U(n.Key);U(n.PackedCount);U((ulong)n.Records.Length);h.AppendData(n.Records.Span);}
            var keys=child.CopySourceRangeKeys();U((ulong)keys.Length);foreach(ulong key in keys){U(key);var rr=child.CopySourceRanges(key);U((ulong)rr.Length);foreach(var r in rr)foreach(int x in Range(r))U(unchecked((ulong)(long)x));}
            U((ulong)child.Words.Count);foreach(var word in child.Words){h.AppendData(word.CopyStorage());var refs=word.CopyReadingOrderReferences();U((ulong)refs.Length);foreach(ulong r in refs)U(r);}
        }
        return Convert.ToHexString(h.GetHashAndReset()).ToLowerInvariant();
    }
    internal static string HashRows(LegacyFullEnglishFirstSourceRecords source)
    {
        using var h=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);byte[] b=new byte[8];
        void U(ulong v){BinaryPrimitives.WriteUInt64LittleEndian(b,v);h.AppendData(b);}
        var rows=source.CopyRows();U((ulong)rows.Count);
        foreach(var row in rows)
        {
            U((ulong)row.Child.Original.Factory.Candidates.Child.DescriptorOrdinal);U((ulong)row.WordOrdinal);U((ulong)row.ReferenceOrdinal);U(row.Reference);U(row.SoleFlatOverride?1UL:0UL);h.AppendData(row.Record.CopyStorage().Span);
            var original=row.CopyChildRanges();U((ulong)original.Length);foreach(var r in original)foreach(int x in Range(r))U(unchecked((ulong)(long)x));
            U((ulong)row.Record.Ranges.Count);foreach(var r in row.Record.Ranges)foreach(int x in Range(r))U(unchecked((ulong)(long)x));
        }
        return Convert.ToHexString(h.GetHashAndReset()).ToLowerInvariant();
    }
    internal static void CategoryProof(Utf8JsonWriter w)
    {
        var p=CategorySnapshotControls.LastProof??throw new InvalidDataException("Actual category mutation proof missing");
        w.WriteStartObject("CategoryMutation");w.WriteString("Before",p.Before);w.WriteString("Mutated",p.Mutated);w.WriteString("Restored",p.Restored);w.WriteString("BaselineRows",p.BaselineRows);w.WriteString("PreparedBeforeRows",p.PreparedBeforeRows);w.WriteString("PreparedAfterRows",p.PreparedAfterRows);w.WriteEndObject();
    }
    internal static string HashParent(LegacyFullEnglishFirstParentInput parent)
    {
        using var h=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);byte[] b=new byte[8];
        void U(long v){BinaryPrimitives.WriteInt64LittleEndian(b,v);h.AppendData(b);}
        U(parent.Current.Count);foreach(var stroke in parent.Current){U(stroke.Points.Count);foreach(var p in stroke.Points){U(BitConverter.SingleToInt32Bits(p.X));U(BitConverter.SingleToInt32Bits(p.Y));}}
        var order=parent.CopyCurrentOriginalPhysical();U(order.Length);foreach(int i in order)U(i);
        for(int i=0;i<parent.Current.Count;i++)for(int j=0;j<parent.Current[i].Points.Count;j++)foreach(int x in Identity(parent.ResolveCurrentPoint(i,j)))U(x);
        var descriptors=parent.CopyDescriptors();U(descriptors.Length);foreach(var d in descriptors)h.AppendData(d);
        U(BitConverter.DoubleToInt64Bits(parent.SourceScales.Primary));U(BitConverter.DoubleToInt64Bits(parent.SourceScales.Secondary));U(BitConverter.SingleToInt32Bits(parent.PhysicalResolution));
        return Convert.ToHexString(h.GetHashAndReset()).ToLowerInvariant();
    }
    internal static void Prefixes(Utf8JsonWriter w,IReadOnlyList<LifecycleControls.HistoricalPrefix> prefixes)
    {
        w.WriteStartArray("GuardPrefixes");foreach(var p in prefixes)
        {
            w.WriteStartObject();w.WriteString("Stage",p.Stage);w.WriteBoolean("Complete",p.Complete);w.WriteBoolean("Faulted",p.Faulted);w.WriteNumber("ReservedRows",p.ReservedRows);
            w.WriteStartArray("Rows");foreach(var r in p.Rows)
            {
                w.WriteStartObject();w.WriteNumber("Descriptor",r.Descriptor);w.WriteNumber("Word",r.Word);w.WriteNumber("Ordinal",r.Ordinal);w.WriteNumber("Reference",r.Reference);w.WriteBoolean("Flat",r.Flat);
                w.WriteBase64String("Storage",r.Storage);w.WriteNumber("LabelCount",r.LabelCount);w.WriteStartArray("ChildRanges");foreach(var x in r.ChildRanges)Ints(w,x);w.WriteEndArray();w.WriteStartArray("Projected");foreach(var x in r.Projected)Ints(w,x);w.WriteEndArray();w.WriteEndObject();
            }w.WriteEndArray();w.WriteStartArray("Descriptors");foreach(var d in p.Descriptors)w.WriteBase64StringValue(d);w.WriteEndArray();w.WriteEndObject();
        }w.WriteEndArray();
    }
    internal static void Rows(Utf8JsonWriter w,LegacyFullEnglishFirstSourceRecords pass)
    {
        w.WriteNumber("RowsCount",pass.OrdinaryReferenceCount);w.WriteNumber("Fragments",pass.ProjectedRangeFragments);w.WriteNumber("PointVisits",pass.GeometryPointVisits);
        w.WriteNumber("Peak",pass.AdditionalLogicalPeakBytes);w.WriteNumber("CopyWork",pass.LogicalCopyWorkBytes);w.WriteNumber("VisitWork",pass.LogicalVisitWork);
        w.WriteStartArray("Rows");foreach(var row in pass.CopyRows())
        {
            if(!ReferenceEquals(row.Owner,pass))throw new InvalidDataException("Same B8 owner row required.");
            w.WriteStartObject();w.WriteNumber("Descriptor",row.Child.Original.Factory.Candidates.Child.DescriptorOrdinal);w.WriteNumber("Word",row.WordOrdinal);w.WriteNumber("Ordinal",row.ReferenceOrdinal);w.WriteNumber("Reference",row.Reference);w.WriteBoolean("Flat",row.SoleFlatOverride);
            w.WriteBase64String("Storage",row.Record.CopyStorage().Span);w.WriteNumber("LabelCount",row.Record.LabelCount);
            w.WriteStartArray("ChildRanges");foreach(var r in row.CopyChildRanges())Ints(w,Range(r));w.WriteEndArray();
            w.WriteStartArray("Projected");foreach(var r in row.Record.Ranges)Ints(w,Range(r));w.WriteEndArray();w.WriteEndObject();
        }w.WriteEndArray();w.WriteStartArray("DescriptorsAfter");foreach(var b in pass.CopyDescriptors())w.WriteBase64StringValue(b);w.WriteEndArray();
    }
}
