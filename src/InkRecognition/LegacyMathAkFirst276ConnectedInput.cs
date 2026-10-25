namespace InkRecognition;

internal sealed partial class LegacyMathAkOriginalInputContext
{
    //NEW owning route; it refuses at the first unported evaluator/shortlist
    //boundary after feature initialization. No historical Context resumes.
    internal static void PrepareFreshFirstInputBoundary(LegacyMathAkSourceGeometry original,
        CancellationToken token=default)=>PrepareFreshFirstInputCore(original,false,token);
    private static void PrepareFreshFirstInputCore(LegacyMathAkSourceGeometry original,bool numerical,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(original);token.ThrowIfCancellationRequested();Require13A0Admission();
        int s=original.StrokeCount,n=original.PointCount,p=checked(n+4*s);
        if(s is<1 or>64||n is<1 or>LegacyMathAkSourceGeometry.MaximumPoints||
            p>LegacyMathAkProcessedStrokePrefix.MaximumExpandedPoints)
            throw new InvalidOperationException("Original source cardinality mismatch.");
        var budget=new Source13A0Budget();int w=(s>>5)+1,a=(s+31)>>5;
        budget.Reserve(checked(32L*n+100L*p+228L*s+6L*s*(s+1)+8L*s*w+4L*(a+w)+216));
        budget.Reserve(checked(316L*p+156L*s+68));budget.Reserve(FreshCommonWorkspace.MaximumPayloadBytes);
        budget.Reserve(First276SourceBinding.AdditionalPayloadBytes);
        budget.Reserve(First276SourceBinding.StorageAdditionalPayloadBytes);
        var frame=LegacyMathAkOriginalInputFrame.CreateFresh(original,token);
        var context=new LegacyMathAkOriginalInputContext(frame);FreshCommonWorkspace? header=null;
        try
        {
            header=FreshCommonWorkspace.Seed(context,frame,token);
            context.prefix=LegacyMathAkProcessedStrokePrefix.PrepareOriginalInput(frame,token);
            context.Preflight(token);context.RunFresh13B6(token);header.JoinAfterActual13B6(token);
            context.Run13A0(budget,null,null,token);var sampled=context.Run13A2Sampler(token);
            var segments=context.Build13A2Segments(sampled,token);header.BindWords(sampled,segments,token);
            header.RunFirstInputInside(token,numerical);
            throw new InvalidOperationException("Unimplemented FIRST boundary unexpectedly returned.");
        }
        catch{header?.Poison();context.phase=-1;throw;}
    }
    private sealed partial class FreshCommonWorkspace
    {
        internal void RunFirstInputInside(CancellationToken token,bool numerical)
        {
            RequireFirstAlphabet(owner,profile,sampled!,segments!,token);
            First276SourceBinding.RunInputInside(owner,this,profile,sampled!,segments!,words,numerical,token);
        }
        internal void RequireFreshInputSeed(CancellationToken token)
        {
            RequireFirstAlphabet(owner,profile,sampled!,segments!,token);
            if(descriptorCount!=0||tempHeapCount!=0||resultSlotCount!=0||hypothesisCount!=0||freeSlotCount!=0)
                throw new InvalidOperationException("Fresh FIRST common pool is not empty.");
        }
    }
    private sealed partial class First276SourceBinding
    {
        private const long InputEntrySourceUpper=1760; //SOURCE upper pending feature-helper closure; unadmitted.
        internal static void RunInputInside(LegacyMathAkOriginalInputContext owner,FreshCommonWorkspace header,
            LegacyMathAkFeatureProfile profile,Source13A2Output sampled,Source13A2SegmentFrame segments,
            int words,bool numerical,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            long priorBytes=segments.ChargedBytes,priorWork=segments.ChargedWork;
            long fixedBytes=checked(AdditionalPayloadBytes+StorageAdditionalPayloadBytes);
            long fixedCopies=checked(AdditionalCopyBytes+StorageAdditionalCopyBytes+16+15);
            long fixedWork=checked(AdditionalSourceUnits+StorageAdditionalSourceUnits+InputEntrySourceUpper);
            if(priorBytes<0||priorWork<0||priorBytes>Source13A2SegmentPayloadLimit-fixedBytes||
                priorWork>Source13A2SegmentWorkLimit-fixedWork||fixedCopies>Source13A2SegmentWorkLimit)
                throw new NotSupportedException("FIRST input fixed remaining reservation exhausted.");
            var binding=new First276SourceBinding(owner,header,profile,sampled,segments,words,
                checked(priorBytes+AdditionalPayloadBytes),checked(priorWork+AdditionalSourceUnits));
            First276ExclusiveStorage? storage=null;First276FreshInputProducer? producer=null;
            try
            {
                storage=First276ExclusiveStorage.CreateInside(binding,checked(priorBytes+fixedBytes),
                    fixedCopies,checked(priorWork+fixedWork),token);
                producer=First276FreshInputProducer.Create(binding,storage,token);
                producer.Run(token,numerical);
                throw new InvalidOperationException("Missing FIRST continuation unexpectedly returned.");
            }
            catch{producer?.Poison();storage?.Poison();binding.state=-1;throw;}
        }
        private sealed partial class First276ExclusiveStorage
        {
            internal void RequireInputProducer(First276SourceBinding actual,CancellationToken token)
            {
                binding.Require(token);
                if(!ReferenceEquals(binding,actual)||!ReferenceEquals(profile,actual.profile)||
                    thread!=Environment.CurrentManagedThreadId||state!=1||
                    input.Length!=288||hidden.Length!=208||buffer90.Length!=258||
                    buffer98.Length!=258||bufferA0.Length!=258||bufferA8.Length!=258)
                    throw new InvalidOperationException("Exclusive FIRST input storage is stale or foreign.");
            }
        }
        private sealed partial class First276FreshInputProducer
        {
            //180 named field bytes; native descriptor108 is a typed adapter,
            //NOT this class layout. Native68..6B are never read/promoted.
            private const int FieldBytes=188,FailureDataBytes=634;
            private const long CreateSourceUpper=5954,ConstructorTokenUpper=339,ChargeSourceUpper=136;
            private const long RequireSourceUpper=1496,RawReadSourceUpper=2002,BeginSourceUpper=104;
            private const long AddSourceUpper=1660,CacheSourceUpper=180,QuerySourceUpper=294;
            private const long SortSourceUpper=11972,GatherSourceUpper=309,BoundarySourceUpper=24457,RunSourceUpper=205;
            private readonly First276SourceBinding binding;
            private readonly First276ExclusiveStorage storage;
            private readonly int n,w,m,rangeLimit,segmentLimit,thread,keyWordOffset;
            private int state,requests,cached,selectedCount,rangeCount,descriptorCount,featureCount,featurePreparedCount;
            private long bytes,copies,work;
            private readonly int[] candidates,selected,offsets,results;
            private readonly uint[] key,keys;
            private readonly byte[] valid;
            private readonly Source13A2NodeRange[] ranges;
            private float minX,minY,maxX,maxY;

            internal static First276FreshInputProducer Create(First276SourceBinding binding,
                First276ExclusiveStorage storage,CancellationToken token)
            {
                binding.Require(token);storage.RequireInputProducer(binding,token);
                binding.header.RequireFreshInputSeed(token);
                var policy=binding.profile.ReadFirst276InputPolicy(token);
                int n=binding.nodeCount,w=binding.words,m=policy.SuffixLimit;
                if(n<0||w!=(n>>5)+1||m is<1 or>8||policy.RangeLimit!=4||policy.SegmentLimit!=8)
                    throw new InvalidOperationException("Actual FIRST input policy/cardinality changed.");
                long entries=0;for(int k=1;k<=m;k++)entries=checked(entries+(n>=k?n-k+1:0));
                long keyWords=checked(entries*w);
                if(entries>int.MaxValue||keyWords>int.MaxValue)
                    throw new NotSupportedException("FIRST input flat-key extent exceeds array domain.");
                long arrays=checked(80+4L*m+4L*(m+1)+4*entries+4L*w+4*keyWords+entries+12L*m);
                long addedBytes=checked(FieldBytes+FailureDataBytes+arrays);
                long addedCopies=checked(FieldBytes+arrays+80+entries+4L*(m+1)+24);
                long setupWork=checked(CreateSourceUpper+ConstructorTokenUpper*(entries+m+24));
                long oldBytes=storage.TotalChargedBytes,oldCopies=checked(storage.TotalChargedCopies+2*binding.segments.ChargedBytes);
                long oldWork=storage.TotalChargedWork;
                if(oldBytes<0||oldCopies<0||oldWork<0||oldBytes>Source13A2SegmentPayloadLimit-addedBytes||
                    oldCopies>Source13A2SegmentWorkLimit-addedCopies||oldWork>Source13A2SegmentWorkLimit-setupWork)
                    throw new NotSupportedException("FIRST input arrays/setup exceed remaining stage reservation.");
                //All variable extents and setup are prepaid BEFORE object/arrays.
                return new(binding,storage,n,w,m,policy.RangeLimit,policy.SegmentLimit,
                    (int)entries,(int)keyWords,checked(oldBytes+addedBytes),
                    checked(oldCopies+addedCopies),checked(oldWork+setupWork));
            }
            private First276FreshInputProducer(First276SourceBinding binding,First276ExclusiveStorage storage,
                int n,int w,int m,int rangeLimit,int segmentLimit,int entries,int keyWords,
                long bytes,long copies,long work)
            {
                this.binding=binding;this.storage=storage;this.n=n;this.w=w;this.m=m;
                this.rangeLimit=rangeLimit;this.segmentLimit=segmentLimit;
                thread=Environment.CurrentManagedThreadId;keyWordOffset=0;state=0;requests=0;cached=0;
                selectedCount=0;rangeCount=0;descriptorCount=1;featureCount=0;featurePreparedCount=0;numericalPhase=0;numericalC0=0;minX=0;minY=0;maxX=0;maxY=0;
                this.bytes=bytes;this.copies=copies;this.work=work;
                candidates=new int[20];selected=new int[m];offsets=new int[m+1];results=new int[entries];
                key=new uint[w];keys=new uint[keyWords];valid=new byte[entries];ranges=new Source13A2NodeRange[m];
                //Actual fresh108 sentinel stores; never infer values from calloc.
                for(int i=0;i<20;i++)candidates[i]=-1;
                //Private cache control: no key/result is readable until valid1.
                for(int i=0;i<entries;i++)valid[i]=0;
                int at=0;for(int k=1;k<=m;k++){offsets[k-1]=at;at=checked(at+(n>=k?n-k+1:0));}offsets[m]=at;
            }
            private void Charge(long addedBytes,long addedCopies,long addedWork,CancellationToken token)
            {
                token.ThrowIfCancellationRequested();
                if(thread!=Environment.CurrentManagedThreadId||state!=1)
                    throw new InvalidOperationException("FIRST input meter thread/state mismatch.");
                long c=checked(addedCopies+24),u=checked(addedWork+ChargeSourceUpper);
                if(addedBytes<0||addedCopies<0||addedWork<0||addedBytes>Source13A2SegmentPayloadLimit-bytes||
                    c>Source13A2SegmentWorkLimit-copies||u>Source13A2SegmentWorkLimit-work)
                    throw new NotSupportedException("FIRST input cumulative reservation exhausted.");
                bytes=checked(bytes+addedBytes);copies=checked(copies+c);work=checked(work+u);
            }
            private void RequireProducer(CancellationToken token)
            {
                binding.Require(token);storage.RequireInputProducer(binding,token);
                if(thread!=Environment.CurrentManagedThreadId||state!=1||descriptorCount!=1||
                    n!=binding.nodeCount||w!=binding.words||keyWordOffset!=0)
                    throw new InvalidOperationException("FIRST input producer lineage changed.");
            }
            private Source13A2SegmentRecord ReadRawSegment(int index,CancellationToken token)
            {
                //4 conservative named37B value transfers; counter stores separate.
                Charge(0,4*37,RawReadSourceUpper,token);RequireProducer(token);
                var row=binding.segments.Read(index,token);int id=row.Range.OriginalId;
                var input=binding.owner.input;int count=input.ItemCount;
                if(count!=input.StrokeCount||count is<1 or>64||(uint)id>=(uint)count||
                    input.ReadStrokeOrdinal(id)!=id||input.ReadItemKind(id)!=-1||
                    input.ReadStrokeId(id)!=id||!input.IsActive(id))
                    throw new NotSupportedException("FIRST input requires the genuine fresh raw-stroke branch.");
                return row;
            }
            internal void Run(CancellationToken token,bool numerical)
            {
                token.ThrowIfCancellationRequested();
                if(thread!=Environment.CurrentManagedThreadId||Interlocked.CompareExchange(ref state,1,0)!=0)
                    throw new InvalidOperationException("FIRST input producer cannot restart.");
                try
                {
                    Charge(0,4,checked(RunSourceUpper*((long)n+1)*(m+1)+RequireSourceUpper),token);RequireProducer(token);
                    for(int end=0;end<n;end++)
                    {
                        BeginEndpoint(token);int limit=m<end+1?m:end+1;
                        for(int k=1;k<=limit;k++){AddSegment(end-k+1,k,token);Query(end,k,token,numerical);}
                    }
                    //Native pops the temporary108 before the subsequent search.
                    Charge(0,4,RunSourceUpper,token);descriptorCount=0;
                    StopAtBoundary(-1,0,"FIRST post-loop search continuation remains unported.",token);
                }
                catch{Poison();throw;}
            }
            private void BeginEndpoint(CancellationToken token)
            {
                Charge(0,checked(4L*w+16+8),checked(BeginSourceUpper*((long)w+1)+RequireSourceUpper),token);RequireProducer(token);
                for(int i=0;i<w;i++)key[i]=0;
                minX=float.MaxValue;minY=float.MaxValue;maxX=-float.MaxValue;maxY=-float.MaxValue;
                selectedCount=0;rangeCount=0;
            }
            private void AddSegment(int index,int count,CancellationToken token)
            {
                Charge(0,85,AddSourceUpper,token);RequireProducer(token);
                var row=ReadRawSegment(index,token);var box=row.Metric;
                key[index>>5]|=1u<<(index&31);
                if(box.MinimumX<minX)minX=box.MinimumX;if(box.MinimumY<minY)minY=box.MinimumY;
                if(maxX<=box.MaximumX&&box.MaximumX!=maxX)maxX=box.MaximumX;
                if(maxY<=box.MaximumY&&box.MaximumY!=maxY)maxY=box.MaximumY;
                selectedCount=count;
            }
            private int GetOrInsert(int end,int count,CancellationToken token)
            {
                Charge(0,checked(8L*w+9),checked(CacheSourceUpper*(2L*w+1)+RequireSourceUpper),token);RequireProducer(token);
                int slot=checked(offsets[count-1]+end-count+1),at=checked(slot*w);
                if(valid[slot]!=0)
                {
                    for(int i=0;i<w;i++)if(keys[at+i]!=key[i])
                        throw new InvalidOperationException("Actual interval/full-key cache identity changed.");
                    return slot;
                }
                for(int i=0;i<w;i++)keys[at+i]=key[i];
                results[slot]=-1;valid[slot]=1;cached++;return slot;
            }
            private void Query(int end,int count,CancellationToken token,bool numerical)
            {
                Charge(0,checked(4L*n+4L*m+84),checked(QuerySourceUpper*((long)n+21)+RequireSourceUpper),token);RequireProducer(token);
                if(count>segmentLimit)throw new NotSupportedException("FIRST segment-limit result continuation remains unported.");
                int slot=GetOrInsert(end,count,token);
                if(results[slot]!=-1)throw new NotSupportedException("FIRST cached result continuation remains unported.");
                for(int i=0;i<20;i++)if(candidates[i]!=-1)
                    throw new NotSupportedException("FIRST existing-descriptor alternative branch remains separate.");
                int found=0;for(int id=0;id<n;id++)if((key[id>>5]&(1u<<(id&31)))!=0)
                {if(found>=selected.Length)throw new InvalidOperationException("Actual suffix exceeds selected scratch.");selected[found++]=id;}
                if(found!=count||selectedCount!=count)throw new InvalidOperationException("Actual FIRST suffix count/key mismatch.");
                Charge(0,(67+22)*4,SortSourceUpper,token);SortSelected(0,count-1);
                GatherRanges(count,token);requests++;
                //Native gathers up toM BEFORE testing receiver30; never truncate4.
                if(rangeCount>rangeLimit)throw new NotSupportedException("FIRST range-cap result continuation remains unported.");
                PrepareActualFirstFeatures(token);
                if(numerical)FillNumericalPrefix(token);
                StopAtBoundary(end,count,"Actual FIRST evaluator/shortlist continuation remains unported.",token);
            }
            private void SortSelected(int left,int right)
            {
                do
                {
                    int pivot=selected[(left+right)/2],i=left,j=right;
                    do
                    {
                        while(selected[i]<pivot)i++;while(pivot<selected[j])j--;
                        if(j<i)break;int value=selected[j];selected[j]=selected[i];selected[i]=value;i++;j--;
                    }while(i<=j);
                    if(left<j)SortSelected(left,j);left=i;
                    if(right<=i)return;
                }while(true);
            }
            private void GatherRanges(int count,CancellationToken token)
            {
                Charge(0,checked(128L*count+4),checked(GatherSourceUpper*((long)count+1)+RequireSourceUpper),token);RequireProducer(token);
                rangeCount=0;
                for(int i=0;i<count;i++)
                {
                    var value=ReadRawSegment(selected[i],token).Range;
                    if(rangeCount!=0&&ranges[rangeCount-1].OriginalId==value.OriginalId&&
                        ranges[rangeCount-1].InclusiveEnd==value.Start)
                    {
                        var prior=ranges[rangeCount-1];ranges[rangeCount-1]=new(prior.OriginalId,prior.Start,value.InclusiveEnd);
                    }
                    else ranges[rangeCount++]=value;
                }
                //Literal source68 remap AFTER gather; fresh raw map is identity,
                //but read/authenticate the actual SAME private map, do not guess.
                for(int i=0;i<rangeCount;i++)
                {
                    var value=ranges[i];int ordinal=binding.owner.input.ReadStrokeOrdinal(value.OriginalId);
                    if(ordinal!=value.OriginalId)throw new InvalidOperationException("Actual original map changed.");
                    ranges[i]=new(ordinal,value.Start,value.InclusiveEnd);
                }
            }
            private void StopAtBoundary(int end,int count,string message,CancellationToken token)
            {
                Charge(0,82+2*552,BoundarySourceUpper,token);
                short[]? initialized=featureCount==276?storage.CopyFirst276Data(this,token):null;
                First276NumericalDiagnosticData? numerical=numericalPhase==2?CopyNumericalData(token):null;
                var data=new First276InputBoundaryData(end,count,selectedCount,rangeCount,w,cached,descriptorCount,
                    binding.owner.descriptorCount,binding.owner.resultCount,featureCount,bytes,copies,work,featureCount==276,false,initialized);
                if(numerical is not null)throw new First276PartialNumericalBoundaryException(data,numerical);
                throw new First276InputBoundaryException(message,data);
            }
            internal void Poison(){Interlocked.Exchange(ref state,-1);}
        }
    }
    //Detached prefix DATA only. InitializedInput, when present, is a fresh276
    //short copy after the complete private write; no padding/storage ref escapes.
    internal sealed record First276InputBoundaryData(int EndNode,int SuffixCount,int SelectedCount,int RangeCount,
        int WorkWords,int CacheEntries,int Descriptor108Count,int PositiveDescriptor8Count,int Positive1390Count,int InitializedInputCount,
        long ChargedPayloadBytes,long ChargedCopyBytes,long ChargedSourceWork,bool HasInitializedFeatures,bool HasEvaluatorOutput,short[]? InitializedInput);
    internal sealed class First276InputBoundaryException:NotSupportedException
    {
        internal First276InputBoundaryData Prefix {get;}
        internal First276InputBoundaryException(string message,First276InputBoundaryData prefix):base(message){Prefix=prefix;}
    }
}
