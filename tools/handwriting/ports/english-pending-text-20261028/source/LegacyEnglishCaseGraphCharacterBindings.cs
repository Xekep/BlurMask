using System.Buffers.Binary;
namespace InkRecognition;
public sealed partial class LegacyOwnedMainTextSourceInterner
{
    internal LegacyOwnedMainTextSourceInternResult InternEnglish(LegacyEnglishCaseNode0Completion.GraphCharacterSource actual,CancellationToken token)
    {
        actual.Context.RequireCurrent(actual,token);
        if(!ReferenceEquals(actual.Context.Interner,this)||!ReferenceEquals(actual.Context.Document,Document))
            throw new InvalidOperationException("Same private EN CHAR interner/original document required.");
        return InternCore(actual.CopyRanges(token),null,token);
    }
}
public sealed partial class LegacyOwnedFreshTextSourceContext
{
    internal LegacyOwnedFreshTextSelection? CreateEnglishCharacter(LegacyEnglishCaseNode0Completion.GraphCharacterPayload actual,CancellationToken token)
    {
        actual.Context.RequireInitializing(actual,token);
        if(!actual.Source.Ordinary||!ReferenceEquals(actual.Context.Selections,this)||!ReferenceEquals(actual.Context.Document,Document)||!actual.Context.MatchesGraphSelectionBinding(this,token))
            throw new InvalidOperationException("Same fresh raw-original EN CHAR registration transaction required.");
        long output=-1;
        return CreateCore(actual.Source.CopyRanges(token),0,ref output,0,token);
    }
}
public sealed partial class LegacyPreparedMainTextGlyphReuse
{
    // Pure numeric prepared seam only. It does not issue a CHAR/source capability.
    internal static LegacyPreparedMainTextGlyphReuse PrepareEnglishNumeric(LegacyNativeUtf8StringPool strings,
        LegacyNativeTextPublicationData properties,LegacyNativeUnicodeNormalizationData normalization,
        IReadOnlyList<LegacyPreparedTextGlyphReuseRecord> initial,uint aggregateFlags)
        =>new(strings,properties,normalization,initial,aggregateFlags);
    private LegacyPreparedMainTextGlyphReuse(LegacyNativeUtf8StringPool actualStrings,
        LegacyNativeTextPublicationData actualProperties,LegacyNativeUnicodeNormalizationData actualNormalization,
        IReadOnlyList<LegacyPreparedTextGlyphReuseRecord> initial,uint aggregateFlags)
    {
        ArgumentNullException.ThrowIfNull(actualStrings);ArgumentNullException.ThrowIfNull(actualProperties);ArgumentNullException.ThrowIfNull(actualNormalization);ArgumentNullException.ThrowIfNull(initial);
        if(initial.Count>65536)throw new ArgumentOutOfRangeException(nameof(initial));
        strings=actualStrings;properties=actualProperties;normalization=actualNormalization;AggregateFlags=aggregateFlags;
        foreach(var row in initial){if(row.Text is not null)strings.Validate(row.Text);rows.Add(new(row.CopyStorage40(),row.Text));}
    }
    private LegacyPreparedMainTextGlyphReuse(LegacyEnglishCaseNode0Completion.GraphCharacterPayload actual,CancellationToken token)
    {
        actual.Context.RequireInitializing(actual,token);strings=actual.Context.Strings;properties=actual.Context.Properties;normalization=actual.Context.Normalization;
        AggregateFlags=actual.Flags;
        if(actual.Initial.Length>65536)throw new NotSupportedException("Bounded complete actual CHAR records required.");
        foreach(var row in actual.Initial){token.ThrowIfCancellationRequested();if(row.Text is not null)strings.Validate(row.Text);rows.Add(new(row.CopyStorage40(),row.Text));}
    }
    internal static LegacyPreparedMainTextGlyphReuse CreateEnglish(LegacyEnglishCaseNode0Completion.GraphCharacterPayload actual,CancellationToken token)=>new(actual,token);
    internal IReadOnlyList<LegacyPreparedTextGlyphReuseRecord> CopyEnglishRecords(LegacyEnglishCaseNode0Completion.GraphCharacterPayload payload,CancellationToken token)
    {
        payload.Context.Require(token);
        if(!ReferenceEquals(payload.Reuse,this)||!ReferenceEquals(strings,payload.Context.Strings)||!payload.Complete)throw new InvalidOperationException("Actual same-context initialized CHAR records required.");
        lock(gate)
        {
            if(failed)throw new InvalidOperationException("Failed CHAR prefix is not a complete snapshot.");
            var copy=new LegacyPreparedTextGlyphReuseRecord[rows.Count];
            for(int i=0;i<copy.Length;i++){token.ThrowIfCancellationRequested();copy[i]=new(rows[i].Storage,rows[i].Text);}
            token.ThrowIfCancellationRequested();return Array.AsReadOnly(copy);
        }
    }
    internal int ReadEnglishSnapshotByteCount(LegacyEnglishCaseNode0Completion.GraphCharacterPayload payload,int expectedRows,CancellationToken token)
    {
        payload.Context.Require(token);
        if(!ReferenceEquals(payload.Reuse,this)||!ReferenceEquals(strings,payload.Context.Strings)||!payload.Complete)
            throw new InvalidOperationException("Actual same-context initialized CHAR records required.");
        lock(gate)
        {
            if(failed||rows.Count!=expectedRows)throw new InvalidOperationException("Same complete CHAR snapshot extent required.");
            int total=0;
            foreach(var row in rows)
            {
                token.ThrowIfCancellationRequested();
                var text=row.Text??throw new InvalidOperationException("Complete CHAR text required.");
                strings.Validate(text);total=checked(total+text.ByteCount);
            }
            token.ThrowIfCancellationRequested();return total;
        }
    }
    // Scalar read from the SAME live private row backing, never an imported snapshot.
    internal uint ReadEnglishCurrentMarkerFlags(LegacyEnglishCaseNode0Completion.GraphCharacterSource source,
        LegacyEnglishCaseNode0Completion.GraphCharacterPayload payload,int marker,CancellationToken token)
    {
        source.Context.RequireCurrent(source,token);
        if(!ReferenceEquals(source.Context,payload.Context)||!ReferenceEquals(payload.Reuse,this)||
            !ReferenceEquals(strings,payload.Context.Strings)||!payload.Complete)
            throw new InvalidOperationException("Actual same-current-source completed CHAR flags required.");
        lock(gate)
        {
            if(failed)throw new InvalidOperationException("Failed CHAR prefix is not a complete snapshot.");
            token.ThrowIfCancellationRequested();
            if((uint)marker>=(uint)rows.Count)throw new NotSupportedException("Actual selected CHAR marker unavailable.");
            uint flags=BinaryPrimitives.ReadUInt32LittleEndian(rows[marker].Storage.AsSpan(8));
            token.ThrowIfCancellationRequested();return flags;
        }
    }
    internal LegacyPreparedTextGlyphReuseResult ApplyEnglish(LegacyEnglishCaseNode0Completion.GraphCharacterPayload payload,
        LegacyEnglishCaseNode0Completion.GraphCharacterSource actual,CancellationToken token)
    {
        actual.Context.RequireReusable(payload,actual,token);
        if(!ReferenceEquals(payload.Reuse,this)||!ReferenceEquals(strings,actual.Context.Strings)||!ReferenceEquals(properties,actual.Context.Properties)||!ReferenceEquals(normalization,actual.Context.Normalization))
            throw new InvalidOperationException("Exact same-owner CHAR21760 records/string pool/properties required.");
        byte[] selected=actual.Selection.CopySelectionRows4();ulong key=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(actual.Reference);
        return Process(selected.Length/4,unchecked((int)actual.Selection.SelectedMarker!.Value),i=>
        {
            ushort ordinal=BinaryPrimitives.ReadUInt16LittleEndian(selected.AsSpan(i*4));
            var glyph=actual.Glyphs.Resolve(key|((ulong)ordinal<<40));
            return new(glyph.PublicationFlags,actual.Context.EmitReuse(actual,glyph,token));
        },token,actual.Context);
    }
}
internal sealed partial class LegacyEnglishCaseNode1Geometry
{
    internal int ReadGraphCharacterLine(LegacyEnglish119MainLocalCandidates local,LegacyOrdinaryCursiveCandidateGraph graph,int node,ushort arc,CancellationToken token)
    {
        source.Validate(token);
        if(IsFaulted||!ReferenceEquals(source.Owner,local)||!ReferenceEquals(source.Graph,graph))throw new InvalidOperationException("Same owned current group/descriptor lineage required.");
        source.RequireOrdinaryGroup(node,arc);
        return source.LineId;
    }
}
