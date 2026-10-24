using System.Buffers.Binary;
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("LegacyEnglishGlyphUtf8.ActualEntry")]
namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    internal sealed record GraphGlyphUtf8Data(ulong Reference,uint[] Flags,byte[][] Utf8,byte[] RetainedSelection4,int Marker);
    internal sealed record GraphGlyphUtf8Result(GraphGlyphChoicesResult Choices,GraphGlyphUtf8Data[] Glyphs);
    private LegacyNativeUnicodeNormalizationData? graphGlyphNormalization;
    private GraphGlyphUtf8Data[]? graphGlyphUtf8;
    // This private owning path emits glyph records, not an accepted TEXT/document.
    // Only authentic sealed normalization tables are accepted; source publication
    // properties are taken from the SAME initialization owner below.
    internal GraphGlyphUtf8Result RunGraphFirstGlyphUtf8(LegacyNativeUnicodeNormalizationData normalization,
        CancellationToken token=default,Action<GraphMainConfidenceObservation>? observer=null,bool captureSourceInputs=false)
    {
        ArgumentNullException.ThrowIfNull(normalization);primary.RequireGraphLoop(this,token);
        if(graphGlyphNormalization is not null)throw new InvalidOperationException("One owning glyph UTF8 execution required.");
        graphGlyphNormalization=normalization;
        var choices=RunGraphFirstGlyphChoices(token,observer,captureSourceInputs);
        return new(choices,(graphGlyphUtf8??throw new InvalidOperationException("No owning glyph UTF8 publication."))
            .Select(x=>x with{Flags=x.Flags.ToArray(),Utf8=x.Utf8.Select(b=>b.ToArray()).ToArray(),RetainedSelection4=x.RetainedSelection4.ToArray()}).ToArray());
    }
    private void CompleteGraphGlyphUtf8(GraphLanguageScope stage,LegacyNativeGlyphRecordPool pool,CancellationToken token)
    {
        RequireGraphPublication(stage,token);
        var normalization=graphGlyphNormalization??throw new InvalidOperationException("Private glyph UTF8 request required.");
        var choices=graphGlyphChoices??throw new InvalidOperationException("All actual owners must be refreshed and selected first.");
        var alphabet=stage.Lease.Root.Source.Alphabet;byte[] header=alphabet.CopySerializedHeader();
        ushort headerE=BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(6));
        long rows=0,ownerRows=0;foreach(var choice in choices){token.ThrowIfCancellationRequested();rows=checked(rows+choice.Selected4.Length/4);ownerRows=checked(ownerRows+choice.Owner40.Length/40);}
        if(rows>4096||ownerRows>65536)throw new NotSupportedException("Bounded whole-owner glyph publication required.");
        // Includes owner-clone + Resolve record, decode/cluster arrays, retained
        // selections, intern keys/bytes and detached exports, charged before any
        // text owner/caller allocations. No source handles leave the private call.
        stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(65536+ownerRows*192+rows*2048+choices.Length*4096L));
        stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(131072+ownerRows*160+rows*4096+choices.Length*8192L));
        stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(16384+rows*8192));
        var properties=stage.Lease.Pass.Initializer.PlanForAudit.ReadGraphGlyphPublication(stage,token);
        using var strings=new LegacyNativeUtf8StringPool();var output=new GraphGlyphUtf8Data[choices.Length];
        for(int i=0;i<choices.Length;i++)
        {
            RequireGraphPublication(stage,token);var choice=choices[i];ulong key=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(choice.Reference);
            var node=pool.CopyOwnerNode(key,token);
            if(!node.Records.Span.SequenceEqual(choice.Owner40))throw new InvalidDataException("Live glyph owner changed after selection.");
            var owner=new LegacyNativeGlyphRecordOwner(new[]{node});
            for(int j=0;j<choice.Selected4.Length;j+=4)
            {
                uint ordinal=BinaryPrimitives.ReadUInt32LittleEndian(choice.Selected4.AsSpan(j,4));
                var glyph=owner.Resolve(key|((ulong)ordinal<<40));var descriptor=glyph.Symbol;
                if((descriptor.KindFlags&7)!=0||descriptor.SymbolOrRunLength>=alphabet.LabelCount)
                    throw new NotSupportedException("Actual control/alternate/macro UTF8 owner remains separate.");
                // No guessed wrapper filter: both native filter settings are
                // equivalent ONLY when the reached ordinary label is not HeaderE.
                if(descriptor.SymbolOrRunLength==headerE)throw new NotSupportedException("Reached HeaderE requires actual wrapper-property filter owner.");
                if(!alphabet.TryGetText(descriptor.SymbolOrRunLength,out string text)||System.Text.Encoding.UTF8.GetByteCount(text)>64)
                    throw new NotSupportedException("Bounded source ordinary UTF8 label required.");
            }
            var result=LegacyNativeTextGlyphCaller.Produce(owner,alphabet,header,choice.Selected4,key,false,
                unchecked((int)(choice.Marker??uint.MaxValue)),strings,properties,normalization,cancellationToken:token);
            RequireGraphPublication(stage,token);
            output[i]=new(choice.Reference,result.Records.Select(x=>x.Flags).ToArray(),result.Records.Select(x=>x.Text.ReadBytes()).ToArray(),result.RetainedSelectionRows.ToArray(),result.AdjustedMarkerIndex);
        }
        RequireGraphPublication(stage,token);graphGlyphUtf8=output;
    }
}
internal sealed partial class LegacyEnglishCaseInitializationPlan
{
    internal LegacyNativeTextPublicationData ReadGraphGlyphPublication(LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,CancellationToken token)
    {
        stage.Owner.RequireGraphPublication(stage,token);RequirePublished(token);
        if(!ReferenceEquals(BoundRoot,stage.Lease.Root)||!ReferenceEquals(knowledge,stage.Lease.Root.Knowledge))
            throw new InvalidOperationException("Same private source publication owner required.");
        return publication;
    }
}

public sealed partial class LegacyNativeGlyphRecordPool
{
    // Scalar private-state observation only. The owning caller authenticates
    // its live graph/pool cohort; no source/WORD identity is minted here.
    internal int ReadGraphRefreshOwnerCount(ulong canonicalKey,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(canonicalKey!=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(canonicalKey)||!nodes.TryGetValue(canonicalKey,out var node))
            throw new InvalidDataException("Same existing canonical glyph owner required.");
        return node.Rows.Count;
    }
}
