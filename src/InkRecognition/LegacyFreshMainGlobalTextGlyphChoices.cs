using System.Runtime.CompilerServices;
using System.Buffers.Binary;
[assembly:InternalsVisibleTo("LegacyGlobalTextGlyphChoices.Probe")]
namespace InkRecognition;

public sealed class LegacyFreshMainGlobalPublishedGlyphChoices
{
    public LegacyFreshMainGlobalTextGlyphChoices Owner{get;}
    public ulong OriginalReference{get;}
    public int OriginalSignedMarker=>unchecked((short)(OriginalReference>>40));
    public LegacyFreshMainGlobalGlyphDocumentPositions SourcePositions{get;}
    public LegacyPreparedGlyphAlternativeSelection Selection{get;}
    public LegacyNativeTextGlyphCallerResult Published{get;}
    internal LegacyFreshMainGlobalPublishedGlyphChoices(LegacyFreshMainGlobalTextGlyphChoices owner,ulong reference,
        LegacyFreshMainGlobalGlyphDocumentPositions positions,LegacyPreparedGlyphAlternativeSelection selection,
        LegacyNativeTextGlyphCallerResult published)
    {Owner=owner;OriginalReference=reference;SourcePositions=positions;Selection=selection;Published=published;}
    public byte[]? CopySelectedUtf8(CancellationToken token=default)
    {
        Owner.Validate(token);int marker=Published.AdjustedMarkerIndex;
        if((uint)marker>=(uint)Published.Records.Count)return null;
        var text=Published.Records[marker].Text;Owner.ValidateText(text,token);return text.ReadBytes();
    }
}

/// <summary>Same live global V68 -> all-owner refresh -> selection ->213A0
/// alternatives in the pre-NN3E. This does not publish CHAR/WORD/TEXT nodes.</summary>
public sealed class LegacyFreshMainGlobalTextGlyphChoices
{
    private static readonly ConditionalWeakTable<LegacyFreshMainGlobalNoCandidateSourceCompletion,
        LegacyFreshMainGlobalTextGlyphChoices> owners=new();
    private readonly object gate=new();
    private readonly LegacyLibraryGlobalTextPublicationScope scope;
    private readonly LegacyNativeTextPublicationData properties;
    private readonly LegacyNativeUnicodeNormalizationData normalization;
    private readonly LegacyNativeCircledPayloadData circles;
    private readonly LegacyNativeUtf8StringPool strings;
    private readonly byte[] header;
    private readonly LegacyFreshMainGlobalGlyphProjectionContext projection;
    private bool entered,faulted;
    private LegacyFreshMainGlobalTextSourceWrapper? sourceWrapper;
    public LegacyFreshMainGlobalNoCandidateSourceCompletion Completion{get;}
    public LegacyFreshMainGlobalOrdinaryGlyphRefresh Refresh{get;}
    public LegacyFreshMainGlobalGlyphUtf8Emission Emitter{get;}
    public LegacyOwnedRawInkDocument Document=>scope.Document;
    public bool IsFaulted{get{lock(gate)return faulted;}}
    internal LegacyNativeUtf8StringPool StringPool=>strings;
    internal LegacyLibraryGlobalTextPublicationScope Scope=>scope;
    internal LegacyNativeTextPublicationData Properties=>properties;
    internal LegacyNativeUnicodeNormalizationData Normalization=>normalization;
    internal LegacyFreshMainGlobalGlyphProjectionContext Projection=>projection;
    private LegacyFreshMainGlobalTextGlyphChoices(LegacyLibraryGlobalTextPublicationScope scope,
        LegacyFreshMainGlobalNoCandidateSourceCompletion completion,ReadOnlySpan<byte> resource,
        LegacyNativeTextPublicationData properties,LegacyNativeUnicodeNormalizationData normalization,
        LegacyNativeCircledPayloadData circles,CancellationToken token)
    {
        this.scope=scope;Completion=completion;this.properties=properties;this.normalization=normalization;this.circles=circles;
        strings=scope.SharedState.Strings;header=resource.Slice(99,16).ToArray();
        Refresh=LegacyFreshMainGlobalOrdinaryGlyphRefresh.Prepare(resource,completion,token);
        Emitter=LegacyFreshMainGlobalGlyphUtf8Emission.Prepare(completion,normalization,circles,token);
        projection=new(scope,completion,token);
        Validate(token);
    }
    public static LegacyFreshMainGlobalTextGlyphChoices Prepare(LegacyLibraryGlobalTextPublicationScope actualScope,
        LegacyFreshMainGlobalNoCandidateSourceCompletion actualCompletion,ReadOnlySpan<byte> acousticResource,
        LegacyNativeTextPublicationData properties,LegacyNativeUnicodeNormalizationData normalization,
        LegacyNativeCircledPayloadData circles,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualScope);ArgumentNullException.ThrowIfNull(actualCompletion);
        ArgumentNullException.ThrowIfNull(properties);ArgumentNullException.ThrowIfNull(normalization);ArgumentNullException.ThrowIfNull(circles);
        actualScope.Validate();actualCompletion.Validate(token);
        if(!ReferenceEquals(actualScope.SourceCompletion,actualCompletion)||!actualCompletion.IsTopV68Completed||
            !ReferenceEquals(actualScope.SharedState.GlobalTopOwner,actualCompletion.Source.Session!.Ownership)||
            !ReferenceEquals(actualScope.SharedState.Document,actualCompletion.Source.Owner.SourceDocument))
            throw new ArgumentException("Identical actual pre-NN global scope, successful V68 and shared document/TOP required.");
        //Validate resource even when a prior same-completion owner is cached.
        _=LegacyFreshMainGlobalOrdinaryGlyphRefresh.Prepare(acousticResource,actualCompletion,token);
        byte[] resource=acousticResource.ToArray();
        var result=owners.GetValue(actualCompletion,_=>new(actualScope,actualCompletion,resource,properties,normalization,circles,token));
        if(!ReferenceEquals(result.scope,actualScope)||!ReferenceEquals(result.properties,properties)||
            !ReferenceEquals(result.normalization,normalization)||!ReferenceEquals(result.circles,circles))
            throw new ArgumentException("Existing same-completion publication cannot change its owner or Unicode data.");
        result.Validate(token);return result;
    }
    internal void Validate(CancellationToken token)
    {
        scope.Validate();Completion.Validate(token);
        if(faulted)throw new InvalidOperationException("Failed global glyph-choice publication cannot restart.");
        if(!ReferenceEquals(scope.SourceCompletion,Completion)||!ReferenceEquals(strings,scope.Context.Strings)||
            !ReferenceEquals(scope.SharedState.GlobalTopOwner,Emitter.TopOwner))
            throw new InvalidOperationException("Global source/string/TOP ownership changed.");
    }
    internal void ValidateText(LegacyNativeInternedUtf8 text,CancellationToken token)
    {Validate(token);strings.Validate(text);Validate(token);}
    public IReadOnlyList<LegacyFreshMainGlobalPublishedGlyphChoices> Publish(ReadOnlySpan<byte> allocatedRecordStorage40,
        ReadOnlySpan<byte> selectionAllocation4,CancellationToken token=default)
    {
        lock(gate)
        {
            Validate(token);
            if(allocatedRecordStorage40.Length!=40||selectionAllocation4.Length%4!=0||selectionAllocation4.Length>65536*4)
                throw new ArgumentException("Complete allocator40 and bounded whole selection4 allocation required.");
            if(entered)throw new InvalidOperationException("This global glyph-choice publication already entered.");
            var references=Completion.Word.Word.FlatReferences.ToArray();
            if(references.Length is <1 or>4096)throw new NotSupportedException("Bounded nonempty actual WORD reference list required.");
            foreach(ulong reference in references)
                if(unchecked((short)(reference>>24))<0)
                    throw new NotSupportedException("Global choices currently require ordinary nonnegative sources; no protocol/span coercion.");
            entered=true;
            try
            {
                byte[] beforeWord=Completion.Word.Word.CopyStorage();int nn=Completion.Source.CachedMainGroupEvaluations;
                var positions=new Dictionary<ulong,LegacyFreshMainGlobalGlyphDocumentPositions>();
                //Native024F30 first pass: ALL references before ANY selection/emission.
                foreach(ulong reference in references)
                {
                    Validate(token);ulong key=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference);
                    var refreshed=Refresh.RefreshOwner(key,allocatedRecordStorage40,token);
                    if(!positions.ContainsKey(key))positions.Add(key,
                        projection.Project(refreshed.Source,token));
                }
                var output=new List<LegacyFreshMainGlobalPublishedGlyphChoices>();int records=0,bytes=0;
                foreach(ulong reference in references)
                {
                    Validate(token);ulong key=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference);
                    var selected=Refresh.Select(reference,allocatedRecordStorage40,selectionAllocation4,token:token);
                    if(!selected.Selection.Succeeded)throw new NotSupportedException("Actual alternative allocation refused; no accepted text fabricated.");
                    var owner=new LegacyNativeGlyphRecordOwner([Emitter.TopOwner.GlyphPool.CopyOwnerNode(key,token)]);
                    //0283B0 fresh local138.byte8=0; no prior-result/property12D0065 input.
                    var result=LegacyNativeTextGlyphCaller.Produce(owner,Emitter.PrimaryAlphabet,header,
                        selected.Selection.CopySelectionRows4(),key,false,unchecked((int)selected.Selection.SelectedMarker!.Value),
                        strings,properties,normalization,actualControlLabelLookup:text=>Emitter.ReadControlLabel(text,token),
                        actualFullGlyphEmitter:glyph=>Emitter.Emit(glyph,token),cancellationToken:token);
                    records=checked(records+result.Records.Count);
                    foreach(var row in result.Records){strings.Validate(row.Text);bytes=checked(bytes+(int)row.ByteCount);}
                    if(records>65536||bytes>8_388_608)throw new NotSupportedException("Portable aggregate glyph publication budget exceeded.");
                    output.Add(new(this,reference,positions[key],selected.Selection,result));
                }
                if(!Completion.Word.Word.CopyStorage().AsSpan().SequenceEqual(beforeWord)||
                    !Completion.Word.Word.FlatReferences.SequenceEqual(references)||
                    Emitter.Completion.Snapshot.SourcePolicy.Owner.AcousticEvaluations!=nn)
                    throw new InvalidOperationException("Glyph-choice publication changed WORD references or performed extra NN.");
                Validate(token);return output.AsReadOnly();
            }
            catch{faulted=true;throw;}
        }
    }
    internal void ClaimSourceWrapper(LegacyFreshMainGlobalTextSourceWrapper actual,CancellationToken token)
    {
        lock(gate)
        {
            Validate(token);
            if(!ReferenceEquals(actual.Choices,this)||!ReferenceEquals(actual.Scope,scope)||entered)
                throw new ArgumentException("One actual identical global wrapper/publication required.");
            actual.ValidatePreclaim(token);entered=true;sourceWrapper=actual;
        }
    }
    internal void ValidateWrapper(LegacyFreshMainGlobalTextSourceWrapper actual,CancellationToken token)
    {
        Validate(token);
        if(!ReferenceEquals(sourceWrapper,actual)||!ReferenceEquals(actual.Choices,this))
            throw new ArgumentException("Exact claimed global source wrapper required.");
    }
    internal LegacyGlobalSourceSelectedGlyph SelectForWrapper(LegacyFreshMainGlobalTextSourceWrapper actual,
        int index,ReadOnlySpan<byte> allocation4,CancellationToken token)
    {
        ValidateWrapper(actual,token);actual.RequireNextSelection(index,token);
        ulong reference=actual.ReferenceAt(index),key=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference);
        uint marker=unchecked((uint)(int)(short)(reference>>40));
        var selected=LegacyPreparedGlyphAlternativeSelection.Select(Emitter.TopOwner.GlyphPool.CopyOwnerNode(key,token),
            marker,allocation4,cancellationToken:token);
        if(!selected.Succeeded)throw new NotSupportedException("Actual global wrapper selection allocation refused.");
        return new(actual,index,reference,selected,new([Emitter.TopOwner.GlyphPool.CopyOwnerNode(key,token)]));
    }
    internal LegacyNativeTextGlyphCallerResult EmitFreshForWrapper(LegacyGlobalSourceSelectedGlyph selected,CancellationToken token)
    {
        ValidateWrapper(selected.Owner,token);selected.Owner.RequireCurrentSelection(selected,token);
        return LegacyNativeTextGlyphCaller.Produce(selected.Glyphs,Emitter.PrimaryAlphabet,header,
            selected.Selection.CopySelectionRows4(),LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(selected.Reference),false,
            unchecked((int)selected.Selection.SelectedMarker!.Value),strings,properties,normalization,
            actualControlLabelLookup:text=>Emitter.ReadControlLabel(text,token),
            actualFullGlyphEmitter:glyph=>Emitter.Emit(glyph,token),cancellationToken:token);
    }
    internal void FaultSourceWrapper(LegacyFreshMainGlobalTextSourceWrapper actual)
    {lock(gate){if(!ReferenceEquals(sourceWrapper,actual))throw new ArgumentException("Exact wrapper claim required.");faulted=true;}}
}
