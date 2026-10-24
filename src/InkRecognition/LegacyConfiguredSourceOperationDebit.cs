[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("LegacyConfiguredTwoAltAugmentation.Probe")]

namespace InkRecognition;

//Private numeric accounting for ONE genuine configured operation. This is not
//a source/request/model/prediction issuer and accepts no replacement vectors.
internal sealed class LegacyConfiguredSourceOperationDebit
{
    internal enum Site { Owner, Preparation, Selected, Group, Context, BasicRaster,
        DynamicBuilder, DynamicSamples, Directional, Intersections, Neural, Cache, Count }
    private readonly long[] copiesBySite=new long[(int)Site.Count],workBySite=new long[(int)Site.Count];
    private readonly LegacyFreshMainConfiguredSingleLinePlainRecognitionResult source;
    private readonly LegacyOwnedRussianMainGlyphConfidence confidence;
    private readonly LegacyRussianModelBundle model;
    private LegacyFreshMainConfiguredWordAugmentation? owner;
    private long copies,work;
    private int phase,thread,activeGroup=-1;
    private bool scoringScopeReserved,scoringScopeIssued;
    private LegacyConfiguredSourceOperationDebit(LegacyFreshMainConfiguredSingleLinePlainRecognitionResult source,
        LegacyOwnedRussianMainGlyphConfidence confidence)
    {this.source=source;this.confidence=confidence;model=confidence.Model;Charge(Site.Owner,288,128);}

    internal static LegacyConfiguredSourceOperationDebit Prepare(
        LegacyFreshMainConfiguredSingleLinePlainRecognitionResult actual,LegacyOwnedMainCandidateSource candidate,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var controller=candidate.SourcePolicy.Controller.Owner;
        if(!candidate.IsConfigured||!ReferenceEquals(candidate.ConfiguredResult,actual)||
            !ReferenceEquals(controller.MainLineSource,actual.Sampled)||!ReferenceEquals(controller.SourceInk,actual.Sampled.Cuts.PreparedInk)||
            !ReferenceEquals(controller.MainLineAcoustic.Context,actual.Context)||!ReferenceEquals(actual.Context.Source,actual.Sampled)||
            !ReferenceEquals(actual.Compact.ConfidenceOwner,candidate.ConfidenceOwner)||
            !ReferenceEquals(candidate.ConfidenceOwner.Graph,actual.Sampled.SourceRanges.Graph)||
            !ReferenceEquals(candidate.ConfidenceOwner.Model,controller.CommonModel)||
            !ReferenceEquals(candidate.ConfidenceOwner.CaseDictionaryOwner,candidate.SourcePolicy.DictionaryOwner))
            throw new ArgumentException("Identical genuine configured sampled/context/confidence/model lineage required.");
        var d=new LegacyConfiguredSourceOperationDebit(actual,candidate.ConfidenceOwner);
        var ink=controller.SourceInk;int j=ink.Points.Length,s=ink.StrokePointCounts.Length;
        var groups=controller.SourceGraph.Groups;int c=groups.Count;long ranges=0;
        //Length traversal is paid BEFORE scanning; every range array length is
        //fixed even when supported external Memory access changes its contents.
        d.Charge(Site.Preparation,0,checked(128L*c+128));
        foreach(var g in groups)
        {token.ThrowIfCancellationRequested();ranges=checked(ranges+g.Ranges.Length);}
        //Once: K snapshot+clear, 3 category loads and their bounded map/Intern
        //scratch, WithTags/export/tags/counts, graph ranges/outer refs, headers,
        //descriptor/residue/scalar/config metadata. No new model tensor load.
        const long k=572763,category=4336;
        d.Charge(Site.Preparation,checked(2*k+6*category+100L*j+4L*s+12*ranges+8L*c+16384),
            checked(10*k+384L*(195+35+19+7)+128L*(j+s+ranges)+16384));
        return d;
    }
    internal void Preparing(LegacyOwnedRussianMainGlyphConfidence actual,LegacyFreshMainSingleLineSampled sampled,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(phase!=0||owner is null||!ReferenceEquals(actual,confidence)||!ReferenceEquals(sampled,source.Sampled))
            throw new ArgumentException("Same private preparing source/confidence required.");
    }
    internal void ReserveScoringScope(LegacyOwnedRussianMainGlyphConfidence actual,
        LegacyFreshMainSingleLineSampled sampled,CancellationToken token)
    {
        Preparing(actual,sampled,token);
        if(scoringScopeReserved)throw new InvalidOperationException("One private scoring scope reservation required.");
        //Before ordinary/scoring construction: added scoring-owner ref8 plus
        //NEW-only sealed scope's two original typed references16. No old-path
        //allocation is introduced, but every old scoring instance grows8B.
        Charge(Site.Owner,24,64);scoringScopeReserved=true;
    }
    internal void IssueScoringScope(LegacyOwnedFreshMainWordControllerPolicy controller,
        LegacyOwnedRussianMainGlyphConfidence actual,LegacyOwnedMainGlyphSourceMetadata? metadata,CancellationToken token)
    {
        Preparing(actual,controller.Owner.MainLineSource,token);
        if(!scoringScopeReserved||scoringScopeIssued||
            !ReferenceEquals(controller.Owner.MainLineSource,source.Sampled)||
            !ReferenceEquals(controller.Owner.CommonModel,model)||
            (metadata is not null&&(!ReferenceEquals(metadata.Ownership.Controller,controller)||
                !ReferenceEquals(metadata.SourceOverlay.ConfidenceOwner,actual))))
            throw new ArgumentException("Exact original metadata/controller/confidence and reserved private scope required.");
        scoringScopeIssued=true;
    }
    internal void Attach(LegacyFreshMainConfiguredWordAugmentation actual)
    {ArgumentNullException.ThrowIfNull(actual);if(owner is not null||phase!=0)throw new InvalidOperationException("Fresh private debit required.");owner=actual;}
    internal void Selected(LegacyFreshMainConfiguredWordAugmentation actual,LegacyConfiguredWordAugmentationEnvelope.Bounds bound)
    {if(!ReferenceEquals(owner,actual)||phase!=0)throw new InvalidOperationException("Same preparing configured debit required.");Charge(Site.Selected,bound.CopyBytes,bound.Work);}
    internal void Enter(LegacyFreshMainConfiguredWordAugmentation actual,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(!ReferenceEquals(owner,actual)||phase!=0)throw new InvalidOperationException("Fresh same debit entry required.");
        LegacyConfiguredSourceCompletionClaim.Require(actualSourceTop(),actual,false,token);
        thread=Environment.CurrentManagedThreadId;phase=1;
    }
    private LegacyFreshMainPlainResultOwnership actualSourceTop()=>source.Alternatives.TopOwner??throw new InvalidOperationException("Actual TOP missing.");
    private void Live(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(phase!=1||thread!=Environment.CurrentManagedThreadId||owner is null)
            throw new InvalidOperationException("Live same-thread configured debit required.");
        LegacyConfiguredSourceCompletionClaim.Require(actualSourceTop(),owner,false,token);
    }
    internal void RequireConfidence(LegacyOwnedRussianMainGlyphConfidence actual,int group,CancellationToken token)
    {Live(token);if(!ReferenceEquals(actual,confidence)||!ReferenceEquals(actual.Model,model)||group!=activeGroup)throw new ArgumentException("Same active confidence/group required.");}
    internal void RequireOwner(LegacyOwnedRussianMainGlyphConfidence actual,CancellationToken token)
    {Live(token);if(!ReferenceEquals(actual,confidence)||!ReferenceEquals(actual.Model,model))throw new ArgumentException("Same configured confidence owner required.");}
    internal void RequireLine(LegacyFreshMainSingleLineSampled actual,LegacyFreshMainSingleLineContext context,CancellationToken token)
    {Live(token);if(!ReferenceEquals(actual,source.Sampled)||!ReferenceEquals(context,source.Context)||activeGroup<0)throw new ArgumentException("Same active sampled/context required.");}
    internal void BeginGroup(LegacyOwnedRussianMainGlyphConfidence actual,int group,CancellationToken token)
    {
        Live(token);if(!ReferenceEquals(actual,confidence)||activeGroup!=-1)throw new InvalidOperationException("One exact cache miss at a time required.");
        var groups=confidence.Graph.Groups;if((uint)group>=(uint)groups.Count)throw new ArgumentOutOfRangeException(nameof(group));
        var ranges=groups[group].Ranges.Span;
        if(ranges.Length is<1 or>15)throw new NotSupportedException("Existing actual glyph range limit exceeded.");
        Charge(Site.Group,0,checked(32L*ranges.Length+128));
        long p=0;foreach(var r in ranges)
        {token.ThrowIfCancellationRequested();if(r.EndPointExclusive<=r.StartPoint)throw new ArgumentException("Actual nonempty group range required.");p=checked(p+r.EndPointExclusive-r.StartPoint);}
        if(p is<1 or>4096)throw new NotSupportedException("Existing actual glyph point limit exceeded.");
        int s=source.Sampled.Cuts.PreparedInk.StrokePointCounts.Length,w=source.Context.Groups.Count;
        if(s is<1 or>256||w>1024)throw new NotSupportedException("Existing prepared/context limits exceeded.");
        //Four chronological orderings, TWO normalizations, raw dynamic input,
        //context arrays/selection and bounded final-cache/confidence scratch.
        //Baseline output988B is paid here for either point or raster branch.
        Charge(Site.Group,checked(24*p+256L*ranges.Length+16L*s+988+16384),checked(256*p+128L*ranges.Length*s+128L*ranges.Length*ranges.Length+16384));
        Charge(Site.Context,checked(8L*ranges.Length+16L*w+168),checked(64L*w*28+16*p+16L*s+1024));
        //Source separation after the learned score uses these same ranges.
        Charge(Site.Cache,checked(4L*s+16L*ranges.Length),checked(1024+16*p+32L*ranges.Length+4L*s));
        //The original shortlist signed sort runs INSIDE ScoreGroup. Reserve
        //its finite150-element worst-case source comparisons BEFORE that call;
        //a later ConvertCache charge must not retroactively pay this work.
        Charge(Site.Cache,0,4L*150*150);
        activeGroup=group;
    }
    internal void EndGroup(LegacyOwnedRussianMainGlyphConfidence actual,int group,bool succeeded)
    {
        if(!ReferenceEquals(actual,confidence)||activeGroup!=group||thread!=Environment.CurrentManagedThreadId)
            throw new InvalidOperationException("Only the actual active miss may finish.");
        activeGroup=-1;if(!succeeded)phase=-1;
    }
    internal void BasicRaster(int points,int strokes,CancellationToken token)
    {
        Live(token);Charge(Site.BasicRaster,checked(24L*points+256L*strokes+32768+16384),
            checked(128L*points+128L*strokes*strokes+32L*16384+8L*2*30*128));
    }
    internal void DynamicInput(int points,CancellationToken token)
    {Live(token);Charge(Site.DynamicBuilder,2L*4096*12+64,checked(128L*points+128));}
    internal void DynamicPoint(CancellationToken token)
    {Live(token);Charge(Site.DynamicBuilder,0,64);}
    internal void DynamicSamples(int count,CancellationToken token)
    {
        Live(token);if(count is<1 or>4096)throw new NotSupportedException("Existing dynamic resample limit required.");
        long n=count-1;Charge(Site.DynamicSamples,checked(12L*count+20*n),checked(128L*count+32*n*n));
    }
    internal void DirectionalInput(int points,int strokes,CancellationToken token)
    {Live(token);Charge(Site.Directional,checked(12L*points+64L*strokes+32*255+1024),checked(128L*points+128*255));}
    internal void DirectionalSamples(int count,int strokes,CancellationToken token)
    {
        Live(token);if(count is<1 or>255)throw new NotSupportedException("Existing directional sample limit required.");
        Charge(Site.Directional,checked(170L*count+16L*strokes+8192),checked(512L*count+128L*strokes+32768));
    }
    internal void Intersections(int points,int cells,CancellationToken token)
    {
        Live(token);if(points is<1 or>255||cells is<1 or>10000)throw new NotSupportedException("Existing directional grid/sample bounds required.");
        long pairs=(long)points*(points-1)/2;
        Charge(Site.Intersections,checked(4L*cells+24L*points),checked(4L*cells+16L*points+32*pairs));
    }
    internal void IntersectionInterval(int count,CancellationToken token)
    {Live(token);if(count is<0 or>255)throw new ArgumentOutOfRangeException(nameof(count));Charge(Site.Intersections,0,8L*count);}
    internal void Neural(CancellationToken token)
    {Live(token);Charge(Site.Neural,13128+1440+256,368720+1520+16384);}
    internal void ConvertCache(int count,CancellationToken token)
    {Live(token);if(count is<0 or>150)throw new NotSupportedException("Existing shortlist limit required.");Charge(Site.Cache,checked(82L*count+8*19+16),checked(256L*count+1024));}
    internal void Complete(CancellationToken token)
    {Live(token);if(activeGroup!=-1)throw new InvalidOperationException("Active cache miss remains.");phase=2;}
    // Scalar-only terminal DATA; no copied/returned counter arrays or selectors.
    // The same augmentation's private RAW exporter authenticates its own debit
    // reference, thread and active export phase before any counter read.
    internal void WriteRawTerminalDiagnostic(LegacyFreshMainConfiguredWordAugmentation actual,
        System.Text.Json.Utf8JsonWriter writer,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(writer);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(owner,actual))throw new InvalidOperationException("Exact RAW debit owner required.");
        actual.RequireRawDiagnosticDebit(this);
        if(phase is < -1 or >2||copiesBySite.Length!=12||workBySite.Length!=12||(int)Site.Count!=12||
            (phase==0?thread!=0:thread!=Environment.CurrentManagedThreadId)||
            actual.IsTopV68Completed&&(phase!=2||activeGroup!=-1))
            throw new InvalidOperationException("Actual terminal RAW debit phase/thread required.");
        writer.WriteStartObject("sourceDebit");writer.WriteNumber("copies",copies);
        writer.WriteNumber("work",work);writer.WriteNumber("phase",phase);writer.WriteNumber("activeGroup",activeGroup);
        writer.WriteStartArray("sites");
        for(int i=0;i<12;i++)
        {
            token.ThrowIfCancellationRequested();writer.WriteStartObject();writer.WriteNumber("site",i);
            writer.WriteNumber("copies",copiesBySite[i]);writer.WriteNumber("work",workBySite[i]);writer.WriteEndObject();
        }
        writer.WriteEndArray();writer.WriteEndObject();
    }
    private void Charge(Site site,long copy,long visits)
    {
        if(copy<0||visits<0||phase<0||phase==2)throw new InvalidOperationException("Invalid or terminal configured debit.");
        long c=checked(copies+copy),w=checked(work+visits);
        if(c>LegacyConfiguredWordAugmentationEnvelope.CopyLimit||w>LegacyConfiguredWordAugmentationEnvelope.WorkLimit)
        {phase=-1;throw new NotSupportedException("Cumulative configured source feature/cache envelope exceeded.");}
        copies=c;work=w;copiesBySite[(int)site]=checked(copiesBySite[(int)site]+copy);workBySite[(int)site]=checked(workBySite[(int)site]+visits);
    }
}

//The additional implementation belongs to this NEW file. The OLD scoring
//owner retains its original typed metadata field and validation unchanged.
public sealed partial class LegacyOwnedRussianMainGlyphScoring
{
    private ConfiguredScoringScope CreateConfiguredScope(LegacyConfiguredSourceOperationDebit debit,
        LegacyOwnedMainGlyphSourceMetadata? originalMetadata,CancellationToken token)
    {
        debit.IssueScoringScope(Controller,ConfidenceOwner,originalMetadata,token);
        return ConfiguredScoringScope.Create(originalMetadata,debit);
    }
    private sealed class ConfiguredScoringScope
    {
        private readonly LegacyOwnedMainGlyphSourceMetadata? originalMetadata;
        private readonly LegacyConfiguredSourceOperationDebit debit;
        private ConfiguredScoringScope(LegacyOwnedMainGlyphSourceMetadata? originalMetadata,
            LegacyConfiguredSourceOperationDebit debit)
        {this.originalMetadata=originalMetadata;this.debit=debit;}
        internal static ConfiguredScoringScope Create(LegacyOwnedMainGlyphSourceMetadata? originalMetadata,
            LegacyConfiguredSourceOperationDebit debit)=>new(originalMetadata,debit);
        internal LegacyConfiguredSourceOperationDebit ReadOperation(LegacyOwnedRussianMainGlyphScoring scoring,CancellationToken token)
        {
            if(!ReferenceEquals(scoring.metadata,originalMetadata))
                throw new InvalidOperationException("Original typed metadata changed.");
            debit.RequireOwner(scoring.ConfidenceOwner,token);return debit;
        }
    }
}
