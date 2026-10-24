[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LegacyMathAkFreshWorkspace.Probe")]

namespace InkRecognition;

internal sealed partial class LegacyMathAkOriginalInputContext
{
    //Opt-in SOURCE diagnostic only. No existing factory/default path is changed.
    //The early header claim is NOT Context.Claim and cannot satisfy Live().
    internal static FreshWorkspaceDiagnosticData PrepareFreshWorkspaceDiagnostic(
        LegacyMathAkSourceGeometry original,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(original);token.ThrowIfCancellationRequested();
        Require13A0Admission();
        int s=original.StrokeCount,n=original.PointCount,p=checked(n+4*s);
        if(s is<1 or>64||n is<1 or>LegacyMathAkSourceGeometry.MaximumPoints||
            p>LegacyMathAkProcessedStrokePrefix.MaximumExpandedPoints)
            throw new InvalidOperationException("Original source cardinality mismatch.");
        var budget=new Source13A0Budget();int w=(s>>5)+1,a=(s+31)>>5;
        //Existing original/source reservations remain exact and independent.
        budget.Reserve(checked(32L*n+100L*p+228L*s+6L*s*(s+1)+8L*s*w+4L*(a+w)+216));
        budget.Reserve(checked(316L*p+156L*s+68));
        //NEW112 semantic header fields +70 detached DATA fields.
        //No native descriptor108, bitmap or heap is allocated by this reserve.
        budget.Reserve(FreshCommonWorkspace.MaximumPayloadBytes);
        var frame=LegacyMathAkOriginalInputFrame.CreateFresh(original,token);
        var context=new LegacyMathAkOriginalInputContext(frame);
        FreshCommonWorkspace? header=null;
        try
        {
            //Still phase0/claim0/prefixNULL. No prior operator has run on frame.
            header=FreshCommonWorkspace.Seed(context,frame,token);
            context.prefix=LegacyMathAkProcessedStrokePrefix.PrepareOriginalInput(frame,token);
            context.Preflight(token);context.RunFresh13B6(token); //Existing Claim, unchanged.
            header.JoinAfterActual13B6(token);
            context.Run13A0(budget,null,null,token);
            var sampled=context.Run13A2Sampler(token);
            var segments=context.Build13A2Segments(sampled,token);
            header.BindWords(sampled,segments,token);
            var data=header.ExportOnce(token);
            token.ThrowIfCancellationRequested();context.phase=3;return data;
        }
        catch
        {
            header?.Poison();context.phase=-1;throw;
        }
    }

    //112 SOURCE-field bytes, excluding CLR headers/alignment. This number has
    //no relation to native108 descriptors. No arrays/delegates/cache are owned.
    private sealed partial class FreshCommonWorkspace
    {
        internal const long MaximumPayloadBytes=112+70;
        internal const long MaximumCopyBytes=116+2+4+28+48+98;
        internal const long MaximumStructuralWork=130+192+320+256;
        private readonly LegacyMathAkOriginalInputContext owner;
        private readonly LegacyMathAkOriginalInputFrame input;
        private readonly LegacyMathAkFeatureProfile profile;
        private readonly object document;
        private Source13A2Output? sampled;
        private Source13A2SegmentFrame? segments;
        private readonly int thread;
        private int state,earlyClaim,words;
        private readonly int heapCapacity;
        private readonly int descriptorCount,tempHeapCount,resultSlotCount,hypothesisCount,freeSlotCount;
        private long payload,copies,work;

        private FreshCommonWorkspace(LegacyMathAkOriginalInputContext owner,LegacyMathAkOriginalInputFrame input)
        {
            this.owner=owner;this.input=input;profile=input.OriginalOwner.FeatureProfile;
            document=input.DocumentIdentity;sampled=null;segments=null;
            thread=Environment.CurrentManagedThreadId;state=1;earlyClaim=0;words=0;
            //Typed tail13A4+32 scalar was parsed once from the SAME private
            //SHA-authenticated profile snapshot; never Program.GridScale.
            heapCapacity=profile.FreshCommonHeapCapacity;
            descriptorCount=0;tempHeapCount=0;resultSlotCount=0;hypothesisCount=0;freeSlotCount=0;
            payload=112;copies=116+2+4;work=130;
            //Own seed claim only; cannot authorize13B6/FIRST or alter old claim.
            if(Interlocked.CompareExchange(ref earlyClaim,1,0)!=0)
                throw new InvalidOperationException("Fresh workspace seed already claimed.");
        }
        internal static FreshCommonWorkspace Seed(LegacyMathAkOriginalInputContext owner,
            LegacyMathAkOriginalInputFrame input,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(owner.phase!=0||owner.claim!=0||owner.prefix is not null||
                !ReferenceEquals(owner.input,input)||!ReferenceEquals(owner.documentIdentity,input.DocumentIdentity))
                throw new InvalidOperationException("Fresh workspace requires the untouched private input context.");
            return new FreshCommonWorkspace(owner,input);
        }
        private void Require(int expected,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(thread!=Environment.CurrentManagedThreadId||earlyClaim!=1||state!=expected||
                !ReferenceEquals(owner.input,input)||!ReferenceEquals(owner.documentIdentity,document)||
                !ReferenceEquals(input.DocumentIdentity,document)||
                !ReferenceEquals(input.OriginalOwner.FeatureProfile,profile))
                throw new InvalidOperationException("Fresh workspace lineage, thread or stage mismatch.");
            owner.Live(); //Never weakened to accept a pre-prefix/finished Context.
        }
        private void Charge(long bytes,long copied,long visited)
        {
            //24 counter-store bytes and32 structural units per call are already
            //included in caller constants. Failure changes no ledger fields.
            if(bytes<0||copied<0||visited<0||bytes>MaximumPayloadBytes-payload||
                copied>MaximumCopyBytes-copies||visited>MaximumStructuralWork-work)
                throw new NotSupportedException("Fresh header source reservation exhausted.");
            payload=checked(payload+bytes);copies=checked(copies+copied);work=checked(work+visited);
        }
        internal void JoinAfterActual13B6(CancellationToken token)
        {
            Require(1,token);
            if(owner.descriptorCount<0||owner.resultCount<0||owner.resultCount>owner.descriptorCount||
                owner.descriptorCount>input.StrokeCount||owner.resultCount>input.StrokeCount)
                throw new InvalidOperationException("Actual13B6 result prefix changed.");
            //Positive descriptor8 and1390 owners remain retained in owner.
            Charge(0,28,192);
            if(Interlocked.CompareExchange(ref state,2,1)!=1)
                throw new InvalidOperationException("Fresh workspace join cannot restart.");
        }
        internal void BindWords(Source13A2Output actualSampled,Source13A2SegmentFrame actualSegments,
            CancellationToken token)
        {
            Require(2,token);
            if(!actualSegments.MatchesSampled(actualSampled,token)||
                !ReferenceEquals(actualSampled.Owner,owner)||owner.source13A0 is null||
                actualSegments.Count!=actualSampled.Markers.Length||actualSegments.OriginalCount!=input.StrokeCount||
                descriptorCount!=0||tempHeapCount!=0||resultSlotCount!=0||hypothesisCount!=0||freeSlotCount!=0||words!=0)
                throw new InvalidOperationException("Fresh common workspace or sampled source changed.");
            int required=actualSegments.RequiredWorkWords;
            if(required!=(actualSampled.Markers.Length>>5)+1||required<1)
                throw new InvalidOperationException("Actual source workspace width mismatch.");
            Charge(0,48,320);
            if(Interlocked.CompareExchange(ref state,3,2)!=2)
                throw new InvalidOperationException("Fresh workspace words cannot rebind.");
            sampled=actualSampled;segments=actualSegments;words=required;
            //No old result-slot key calloc: zero slots allocate ZERO key bytes.
        }
        internal FreshWorkspaceDiagnosticData ExportOnce(CancellationToken token)
        {
            Require(3,token);segments!.Validate(token);
            if(!ReferenceEquals(sampled!.Owner,owner)||segments.Count!=sampled.Markers.Length)
                throw new InvalidOperationException("Bound private workspace source changed.");
            Charge(70,98,256);
            if(Interlocked.CompareExchange(ref state,4,3)!=3)
                throw new InvalidOperationException("Fresh workspace DATA already exported.");
            return new(1,words,heapCapacity,descriptorCount,tempHeapCount,resultSlotCount,hypothesisCount,
                freeSlotCount,owner.descriptorCount,owner.resultCount,segments.Count,payload,copies,work,false,false);
        }
        internal void Poison()
        {
            //4B reserved at seed even on success. No fallible debit masks error.
            Interlocked.Exchange(ref state,-1);
        }
    }
    //70 logical scalar field bytes, caller DATA only. No actualFrame/Context,
    //arrays, masks, handles, request issuer or historical-resume constructor.
    internal sealed record FreshWorkspaceDiagnosticData(int Version,int WorkWords,int HeapCapacity,
        int Descriptor108Count,int TemporaryHeapCount,int ResultSlot16Count,int Hypothesis24Count,int FreeSlotCount,
        int PositiveDescriptor8Count,int Positive1390Count,int SegmentCount,long ChargedPayloadBytes,
        long ChargedCopyBytes,long ChargedStructuralWork,bool HasFirstRequest,bool HasCandidateAuthority);
}
