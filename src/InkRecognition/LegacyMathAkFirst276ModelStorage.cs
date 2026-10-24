[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LegacyMathAkFirst276ModelContent.Probe")]

namespace InkRecognition;

public sealed partial class LegacyMathAkFeatureProfile
{
    //Once-per-profile owned weights. No views, model replacement or evaluator.
    private readonly First276ModelData firstModelData;
    internal const long First276ModelAdditionalPayloadBytes=168914+24+8;
    internal const long First276ModelAdditionalCopyBytes=2*168914+24+8+48;
    internal const long First276ModelAdditionalSourceUnits=13267216; //Derived by SOURCE audit.
    internal void RequireFirst276StorageModel(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(firstModelData is null)throw new InvalidOperationException("Private FIRST model owner is uninitialized.");
        firstModelData.RequireStoragePolicy();
    }

    //Diagnostic-only scalar DATA seam. No view, live receiver or replacement.
    internal int ReadFirst276RetainedScalar(int bank,int index,CancellationToken token=default)
    {
        RequireFirst276StorageModel(token);
        return firstModelData.ReadScalar(bank,index,token);
    }

    private struct First276ModelReservation
    {
        private long bytes,copies,work;
        internal void Charge(long b,long c,long w)
        {
            if(b<0||c<0||w<0||b>First276ModelAdditionalPayloadBytes-bytes||
                c>First276ModelAdditionalCopyBytes-copies||w>First276ModelAdditionalSourceUnits-work)
                throw new NotSupportedException("FIRST model retention reservation exhausted.");
            bytes=checked(bytes+b);copies=checked(copies+c);work=checked(work+w);
        }
    }
    private sealed class First276ModelData
    {
        private readonly short[] first;
        private readonly sbyte[] second;
        private readonly byte[] scales;
        private First276ModelData(short[] first,sbyte[] second,byte[] scales)
        {this.first=first;this.second=second;this.scales=scales;}
        private static ushort MH(byte[] b,int at)=>(ushort)(b[at]|b[at+1]<<8);
        private static uint MI(byte[] b,int at)=>(uint)(b[at]|b[at+1]<<8|b[at+2]<<16|b[at+3]<<24);
        internal static First276ModelData ReadAuthenticated(byte[] capture,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var reservation=new First276ModelReservation();
            //Full retained array/field payload, copy and named-source upper are
            //reserved BEFORE the first header read, loop or new weight array.
            reservation.Charge(First276ModelAdditionalPayloadBytes,First276ModelAdditionalCopyBytes,
                First276ModelAdditionalSourceUnits);
            if(capture.Length!=LegacyMathMixed107RawResource.ResourceBytes||
                MI(capture,8702)!=0x0107fea0||MI(capture,8706)!=168940||
                capture[8710]!=2||MH(capture,8711)!=0x3f||
                MI(capture,8713)!=276||MI(capture,8717)!=201||
                MI(capture,8721)!=0||MI(capture,8725)!=0||
                MI(capture,8729)!=258||MH(capture,8733)!=10)
                throw new InvalidDataException("Actual FIRST276 raw model frame differs.");
            var first=new short[57600];var second=new sbyte[53456];var scales=new byte[258];
            DecodeFirst(capture,first,token);
            DecodeSecond(capture,second,token);
            DecodeScales(capture,scales,token);
            token.ThrowIfCancellationRequested();
            return new First276ModelData(first,second,scales);
        }
        private static void DecodeFirst(byte[] capture,short[] target,CancellationToken token)
        {
            for(int i=0;i<57600;i++)
            {
                if((i&31)==0)token.ThrowIfCancellationRequested();
                short value=unchecked((short)MH(capture,8736+2*i));
                if(i%288>=276&&value!=0)
                    throw new InvalidDataException("Actual FIRST first-matrix padding differs.");
                target[i]=value;
            }
        }
        private static void DecodeSecond(byte[] capture,sbyte[] target,CancellationToken token)
        {
            for(int i=0;i<53456;i++)
            {
                if((i&31)==0)token.ThrowIfCancellationRequested();
                sbyte value=unchecked((sbyte)capture[123936+i]);
                if(i%208>=201&&value!=0)
                    throw new InvalidDataException("Actual FIRST second-matrix padding differs.");
                target[i]=value;
            }
        }
        private static void DecodeScales(byte[] capture,byte[] target,CancellationToken token)
        {
            for(int i=0;i<258;i++)
            {
                if((i&31)==0)token.ThrowIfCancellationRequested();
                byte value=capture[177392+i];
                if(value==0||i==0&&value!=1)
                    throw new InvalidDataException("Actual FIRST unsigned row scale differs.");
                target[i]=value;
            }
        }
        internal int ReadScalar(int bank,int index,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(bank is<0 or>2)throw new ArgumentOutOfRangeException(nameof(bank));
            int count=bank==0?57600:bank==1?53456:258;
            if((uint)index>=(uint)count)throw new ArgumentOutOfRangeException(nameof(index));
            return bank==0?first[index]:bank==1?second[index]:scales[index];
        }
        internal void RequireStoragePolicy()
        {
            if(first.Length!=57600||second.Length!=53456||scales.Length!=258)
                throw new InvalidOperationException("Private FIRST model storage changed.");
        }
    }
}

internal sealed partial class LegacyMathAkOriginalInputContext
{
    //NEW explicit owning route. Old alphabet diagnostic/source body is unchanged.
    //No historical DATA resume and no initialized FIRST/cache/candidate flag.
    internal static First276StorageDiagnosticData PrepareFreshFirstStorageDiagnostic(
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
        budget.Reserve(First276SourceBinding.AdditionalPayloadBytes);
        budget.Reserve(First276SourceBinding.StorageAdditionalPayloadBytes);
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
            var data=header.ReadFirstStorageInside(token);
            token.ThrowIfCancellationRequested();context.phase=3;return data;
        }
        catch{header?.Poison();context.phase=-1;throw;}
    }
    private sealed partial class FreshCommonWorkspace
    {
        internal First276StorageDiagnosticData ReadFirstStorageInside(CancellationToken token)
        {
            RequireFirstAlphabet(owner,profile,sampled!,segments!,token);
            return First276SourceBinding.ReadStorageInside(owner,this,profile,sampled!,segments!,words,token);
        }
    }
    private sealed partial class First276SourceBinding
    {
        //96 receiver fields +5120 typed arrays +91 detached wrapper fields.
        //Original binding88B/labels28B/ALDATA66B stay charged by old182B slice.
        internal const long StorageAdditionalPayloadBytes=96+5120+91;
        internal const long StorageAdditionalCopyBytes=96+5120+91+8;
        internal const long StorageAdditionalSourceUnits=114406; //Derived by SOURCE audit.
        internal static First276StorageDiagnosticData ReadStorageInside(
            LegacyMathAkOriginalInputContext owner,FreshCommonWorkspace header,
            LegacyMathAkFeatureProfile profile,Source13A2Output sampled,
            Source13A2SegmentFrame segments,int words,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            long priorBytes=segments.ChargedBytes,priorWork=segments.ChargedWork;
            long addedBytes=checked(AdditionalPayloadBytes+StorageAdditionalPayloadBytes);
            long addedCopies=checked(AdditionalCopyBytes+StorageAdditionalCopyBytes);
            long addedWork=checked(AdditionalSourceUnits+StorageAdditionalSourceUnits);
            if(priorBytes<0||priorWork<0||
                priorBytes>Source13A2SegmentPayloadLimit-addedBytes||
                priorWork>Source13A2SegmentWorkLimit-addedWork||
                addedCopies>Source13A2SegmentWorkLimit)
                throw new NotSupportedException("FIRST storage source remaining reservation exhausted.");
            //All new arrays/objects are prepaid. The old binding fields retain
            //their AL-only ledger slice; the receiver stores the combined total.
            var binding=new First276SourceBinding(owner,header,profile,sampled,segments,words,
                checked(priorBytes+AdditionalPayloadBytes),checked(priorWork+AdditionalSourceUnits));
            First276ExclusiveStorage? storage=null;
            try
            {
                storage=First276ExclusiveStorage.CreateInside(binding,checked(priorBytes+addedBytes),
                    addedCopies,checked(priorWork+addedWork),token);
                int count=binding.operand.ReadCount(token);
                var labels=new ushort[14];
                for(int i=0;i<labels.Length;i++)labels[i]=binding.operand.ResolveSpecial(i,token);
                if(binding.operand.ReadCount(token)!=count)
                    throw new InvalidOperationException("Private immutable alphabet count changed.");
                binding.Require(token);
                //Still actual header3/binding1 here. This changes only a private
                //allocation-state marker, not feature/output/cache knownness.
                storage.CloseAllocationForData(token);
                var headerData=header.ExportOnce(token);
                binding.state=2;
                var alphabet=new First276AlphabetDiagnosticData(1,count,labels,headerData,
                    LegacyMathAkFeatureProfile.FirstAlphabetAdditionalPayloadBytes,
                    binding.chargedBytes-priorBytes,binding.chargedCopies,binding.chargedWork-priorWork,
                    17,7,false,false);
                return new First276StorageDiagnosticData(1,alphabet,
                    storage.InputArrayLength,storage.HiddenArrayLength,4,storage.OutputArrayLength,200,5135,5120,
                    LegacyMathAkFeatureProfile.First276ModelAdditionalPayloadBytes,
                    LegacyMathAkFeatureProfile.First276ModelAdditionalCopyBytes,
                    LegacyMathAkFeatureProfile.First276ModelAdditionalSourceUnits,
                    storage.TotalChargedBytes-priorBytes-AdditionalPayloadBytes,
                    storage.TotalChargedCopies-AdditionalCopyBytes,
                    storage.TotalChargedWork-priorWork-AdditionalSourceUnits,false,false,false);
            }
            catch{storage?.Poison();binding.state=-1;throw;}
        }
        private sealed class First276ExclusiveStorage
        {
            //Genuine private AL binding retained; neither DATA nor a count/hash
            //can mint this. Profile retains immutable matrices; no array escapes.
            private readonly First276SourceBinding binding;
            private readonly LegacyMathAkFeatureProfile profile;
            private readonly short[] input,hidden;
            private readonly int[] buffer90,buffer98,bufferA0,bufferA8;
            private readonly int thread;
            private int state;
            private readonly long chargedBytes,chargedCopies,chargedWork;
            private First276ExclusiveStorage(First276SourceBinding binding,long bytes,long copies,long work)
            {
                this.binding=binding;profile=binding.profile;
                thread=Environment.CurrentManagedThreadId;state=1;
                chargedBytes=bytes;chargedCopies=copies;chargedWork=work;
                input=new short[288];hidden=new short[208];
                buffer90=new int[258];buffer98=new int[258];
                bufferA0=new int[258];bufferA8=new int[258];
                //CLR zeros are physical allocation writes only. No prepared
                //feature/padding/bias/output values are asserted or read here.
            }
            internal static First276ExclusiveStorage CreateInside(First276SourceBinding binding,
                long bytes,long copies,long work,CancellationToken token)
            {
                binding.Require(token);binding.profile.RequireFirst276StorageModel(token);
                return new First276ExclusiveStorage(binding,bytes,copies,work);
            }
            internal void CloseAllocationForData(CancellationToken token)
            {
                binding.Require(token);
                if(thread!=Environment.CurrentManagedThreadId||state!=1||
                    !ReferenceEquals(profile,binding.profile))
                    throw new InvalidOperationException("FIRST storage binding/thread/state changed.");
                if(Interlocked.CompareExchange(ref state,2,1)!=1)
                    throw new InvalidOperationException("FIRST allocation scope cannot restart.");
            }
            //Scalar DATA reads only, sole private source-route caller. No buffer
            //view or initialized-value authority is returned by these getters.
            internal int InputArrayLength=>input.Length;
            internal int HiddenArrayLength=>hidden.Length;
            internal int OutputArrayLength=>buffer90.Length;
            internal long TotalChargedBytes=>chargedBytes;
            internal long TotalChargedCopies=>chargedCopies;
            internal long TotalChargedWork=>chargedWork;
            internal void Poison(){Interlocked.Exchange(ref state,-1);}
        }
    }
    //91 named bytes; detached diagnostic DATA, never receiver/request authority.
    internal sealed record First276StorageDiagnosticData(int Version,
        First276AlphabetDiagnosticData Alphabet,int InputArrayLength,int HiddenArrayLength,
        int OutputArrays,int OutputArrayLength,int NativeObjectBytes,int NativeScratchBytes,
        int TypedScratchBytes,long ModelPayloadBytes,long ModelCopyBytes,long ModelSourceUnits,
        long AdditionalStoragePayloadBytes,long AdditionalStorageCopyBytes,long AdditionalStorageSourceUnits,
        bool HasInitializedFeatures,bool HasPreparedReceiver,bool HasCandidateAuthority);
}
