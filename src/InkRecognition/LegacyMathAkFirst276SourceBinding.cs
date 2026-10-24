namespace InkRecognition;

internal sealed partial class LegacyMathAkOriginalInputContext
{
    //Explicit NEW source route. Same positive13B6/13A0/sampler/segments chronology;
    //no historical diagnostic resume and no FIRST evaluator/prepared flag.
    internal static First276AlphabetDiagnosticData PrepareFreshFirstAlphabetDiagnostic(
        LegacyMathAkSourceGeometry original,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(original);token.ThrowIfCancellationRequested();
        Require13A0Admission();
        int s=original.StrokeCount,n=original.PointCount,p=checked(n+4*s);
        if(s is<1 or>64||n is<1 or>LegacyMathAkSourceGeometry.MaximumPoints||
            p>LegacyMathAkProcessedStrokePrefix.MaximumExpandedPoints)
            throw new InvalidOperationException("Original source cardinality mismatch.");
        var budget=new Source13A0Budget();int w=(s>>5)+1,a=(s+31)>>5;
        budget.Reserve(checked(32L*n+100L*p+228L*s+6L*s*(s+1)+8L*s*w+4L*(a+w)+216));
        budget.Reserve(checked(316L*p+156L*s+68));
        budget.Reserve(FreshCommonWorkspace.MaximumPayloadBytes);
        //Additional real binding/returned DATA only; no cloned sampled backing.
        budget.Reserve(First276SourceBinding.AdditionalPayloadBytes);
        var frame=LegacyMathAkOriginalInputFrame.CreateFresh(original,token);
        var context=new LegacyMathAkOriginalInputContext(frame);
        FreshCommonWorkspace? header=null;
        try
        {
            header=FreshCommonWorkspace.Seed(context,frame,token);
            context.prefix=LegacyMathAkProcessedStrokePrefix.PrepareOriginalInput(frame,token);
            context.Preflight(token);context.RunFresh13B6(token);
            header.JoinAfterActual13B6(token);
            context.Run13A0(budget,null,null,token);
            var sampled=context.Run13A2Sampler(token);
            var segments=context.Build13A2Segments(sampled,token);
            header.BindWords(sampled,segments,token);
            var data=header.ReadFirstAlphabetInside(token);
            token.ThrowIfCancellationRequested();context.phase=3;return data;
        }
        catch{header?.Poison();context.phase=-1;throw;}
    }

    private sealed partial class FreshCommonWorkspace
    {
        internal void RequireFirstAlphabet(LegacyMathAkOriginalInputContext actualOwner,
            LegacyMathAkFeatureProfile actualProfile,Source13A2Output actualSampled,
            Source13A2SegmentFrame actualSegments,CancellationToken token)
        {
            Require(3,token);
            if(!ReferenceEquals(owner,actualOwner)||!ReferenceEquals(profile,actualProfile)||
                !ReferenceEquals(sampled,actualSampled)||!ReferenceEquals(segments,actualSegments)||
                !actualSegments.MatchesSampled(actualSampled,token)||
                actualSegments.Count!=actualSampled.Markers.Length||words!=actualSegments.RequiredWorkWords)
                throw new InvalidOperationException("Actual FIRST alphabet source binding changed.");
        }
        internal First276AlphabetDiagnosticData ReadFirstAlphabetInside(CancellationToken token)
        {
            RequireFirstAlphabet(owner,profile,sampled!,segments!,token);
            return First276SourceBinding.ReadInside(owner,this,profile,sampled!,segments!,words,token);
        }
    }
    private sealed partial class First276SourceBinding
    {
        //Five refs40 + embedded typed operand ref8 + four I32s16 +three I64s24.
        internal const long BindingPayloadBytes=88;
        //Returned record66 +14ushort DATA28; no live owner retained by DATA.
        internal const long AdditionalPayloadBytes=BindingPayloadBytes+66+28;
        internal const long AdditionalCopyBytes=BindingPayloadBytes+66+28+28+8+8;
        internal const long AdditionalSourceUnits=105755; //Derived by SOURCE audit.
        private readonly LegacyMathAkOriginalInputContext owner;
        private readonly FreshCommonWorkspace header;
        private readonly LegacyMathAkFeatureProfile profile;
        private readonly Source13A2Output sampled;
        private readonly Source13A2SegmentFrame segments;
        private readonly First276PreparationOperand operand;
        private readonly int thread,nodeCount,words;
        private int state;
        private readonly long chargedBytes,chargedCopies,chargedWork;
        private First276SourceBinding(LegacyMathAkOriginalInputContext owner,FreshCommonWorkspace header,
            LegacyMathAkFeatureProfile profile,Source13A2Output sampled,Source13A2SegmentFrame segments,
            int words,long bytes,long work)
        {
            this.owner=owner;this.header=header;this.profile=profile;this.sampled=sampled;this.segments=segments;
            thread=Environment.CurrentManagedThreadId;nodeCount=segments.Count;this.words=words;state=1;
            chargedBytes=bytes;chargedCopies=AdditionalCopyBytes;chargedWork=work;
            operand=new First276PreparationOperand(this);
        }
        private void Require(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(state!=1||thread!=Environment.CurrentManagedThreadId||
                !ReferenceEquals(owner.input.OriginalOwner.FeatureProfile,profile)||
                !ReferenceEquals(sampled.Owner,owner)||segments.Count!=nodeCount||
                sampled.Markers.Length!=nodeCount||words!=(nodeCount>>5)+1)
                throw new InvalidOperationException("FIRST alphabet operand is stale or foreign.");
            header.RequireFirstAlphabet(owner,profile,sampled,segments,token);
        }
        internal static First276AlphabetDiagnosticData ReadInside(
            LegacyMathAkOriginalInputContext owner,FreshCommonWorkspace header,
            LegacyMathAkFeatureProfile profile,Source13A2Output sampled,
            Source13A2SegmentFrame segments,int words,CancellationToken token)
        {
            //Private actual objects only. All remaining-cap checks precede ANY
            //new binding/labels/returned-record allocation. Native order retained.
            token.ThrowIfCancellationRequested();
            long priorBytes=segments.ChargedBytes,priorWork=segments.ChargedWork;
            if(priorBytes<0||priorWork<0||
                priorBytes>Source13A2SegmentPayloadLimit-AdditionalPayloadBytes||
                priorWork>Source13A2SegmentWorkLimit-AdditionalSourceUnits||
                AdditionalCopyBytes>Source13A2SegmentWorkLimit)
                throw new NotSupportedException("FIRST alphabet source remaining reservation exhausted.");
            long totalBytes=checked(priorBytes+AdditionalPayloadBytes),totalWork=checked(priorWork+AdditionalSourceUnits);
            var binding=new First276SourceBinding(owner,header,profile,sampled,segments,words,totalBytes,totalWork);
            try
            {
                int count=binding.operand.ReadCount(token);
                var labels=new ushort[14]; //28B zero +28B writes already prepaid.
                for(int i=0;i<labels.Length;i++)labels[i]=binding.operand.ResolveSpecial(i,token);
                if(binding.operand.ReadCount(token)!=count)
                    throw new InvalidOperationException("Private immutable alphabet count changed.");
                binding.Require(token);
                var headerData=header.ExportOnce(token);
                binding.state=2;
                return new First276AlphabetDiagnosticData(1,count,labels,headerData,
                    LegacyMathAkFeatureProfile.FirstAlphabetAdditionalPayloadBytes,
                    binding.chargedBytes-priorBytes,binding.chargedCopies,binding.chargedWork-priorWork,
                    17,7,false,false);
            }
            catch{binding.state=-1;throw;}
        }
        //No caller can manufacture this operand. It carries SAME actual AL
        //through the live binding, never a numeric/source pointer or DATA image.
        private readonly struct First276PreparationOperand
        {
            private readonly First276SourceBinding binding;
            internal First276PreparationOperand(First276SourceBinding binding){this.binding=binding;}
            internal ushort ReadCount(CancellationToken token)
            {binding.Require(token);return binding.profile.FirstAlphabetCount;}
            internal ushort ResolveSpecial(int ordinal,CancellationToken token)
            {binding.Require(token);return binding.profile.ResolveFirstAlphabetSpecial(ordinal);}
            //Byte17 and1A..1F opaque.00..07 is a typed reference, never bytes.
            //Unused here: actual request consumers may read only proved scalars.
            internal byte ReadKnownScalar(int offset,CancellationToken token)
            {
                binding.Require(token);
                if(offset is>=8 and<=0x16)return 0;
                if(offset is 0x18 or 0x19)return 255;
                throw new InvalidOperationException("Native request byte is opaque or a typed AL reference.");
            }
        }
    }
    //66 logical fields; labels caller DATA only. Does not retain binding/Context.
    internal sealed record First276AlphabetDiagnosticData(int Version,int AlphabetCount,
        ushort[] SpecialLabels,FreshWorkspaceDiagnosticData Header,long ProfileRetainedPayload,
        long AdditionalPayload,long AdditionalCopies,long AdditionalSourceWork,
        int KnownRequestNumericBytes,int OpaqueRequestBytes,bool HasPreparedFirstReceiver,bool HasCandidateAuthority);
}
