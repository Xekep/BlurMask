namespace InkRecognition;
internal sealed partial class LegacyMathAkOriginalInputContext
{
    internal static void PrepareFreshFirstNormalizedBoundary(LegacyMathAkSourceGeometry original,
        CancellationToken token=default)=>PrepareFreshFirstInputCore(original,2,token);
    private sealed partial class First276SourceBinding
    {
        private sealed partial class First276ExclusiveStorage
        {
            internal void NormalizeInside(First276FreshInputProducer actual,CancellationToken token)
            {
                actual.BeginNormalization(this,binding,token);RequireInputProducer(binding,token);
                int sum=0,maximum=0;bool overflow=false;
                for(int i=1;i<258;i++)
                {
                    token.ThrowIfCancellationRequested();int value=buffer90[i];
                    if(value<0||value>1641499228)throw new InvalidOperationException("Known native EXP domain required.");
                    sum=unchecked(sum+value);if(sum<0)overflow=true;if(value>maximum)maximum=value;
                }
                // Full-pass negative-sum flag is equivalent to native early
                // first overflow followed by complete repeated in-place SAR1.
                int halvings=0;
                while(overflow)
                {
                    if(halvings++>=8)throw new InvalidOperationException("Actual nonnegative EXP overflow bound changed.");
                    sum=0;maximum=0;overflow=false;
                    for(int i=1;i<258;i++)
                    {
                        token.ThrowIfCancellationRequested();int value=buffer90[i]>>1;buffer90[i]=value;
                        sum=unchecked(sum+value);if(sum<0)overflow=true;if(value>maximum)maximum=value;
                    }
                }
                if(maximum==0){actual.CompleteNormalization(this,binding,false,0,token);return;}
                int sumShift=0;uint bits=(uint)sum;
                while(bits>127){bits>>=1;sumShift++;}
                int divisor=sum>>sumShift,valueShift=0,shiftedMaximum=maximum;
                while(shiftedMaximum<0x40000000){shiftedMaximum<<=1;valueShift++;}
                for(int i=1;i<258;i++)
                {
                    token.ThrowIfCancellationRequested();
                    uint fraction=unchecked((uint)buffer90[i]<<valueShift)/(uint)divisor;
                    fraction>>=(valueShift+sumShift-15)&31;
                    int q15=fraction>32767?32767:(int)fraction;
                    bufferA0[i]=q15;buffer90[i]=q15>>5;
                }
                actual.CompleteNormalization(this,binding,true,divisor<<sumShift,token);
            }
            internal First276NormalizedDiagnosticData CopyNormalizedData(First276FreshInputProducer actual,CancellationToken token)
            {
                int phase=actual.RequireNormalizedData(this,binding,token);RequireInputProducer(binding,token);
                int c0=actual.RequireOwnedNumericalData(this,binding,token);
                if(phase==3)return new(c0,null,false,true,null,null);
                var q10=new short[257];var probability=new int[257];
                for(int i=0;i<257;i++)
                {
                    token.ThrowIfCancellationRequested();q10[i]=checked((short)buffer90[i+1]);probability[i]=bufferA0[i+1];
                }
                return new(c0,actual.ReadNormalizedC4(this,binding,token),true,false,q10,probability);
            }
        }
        private sealed partial class First276FreshInputProducer
        {
            private int normalizationPhase,normalizationC4;
            private const long NormalizationSourceUpper=32768+9*257*64+257*96+64*16;
            private void NormalizeNumericalPrefix(CancellationToken token)
            {
                Charge(0,10292,NormalizationSourceUpper,token);RequireProducer(token);
                storage.NormalizeInside(this,token);
            }
            internal void BeginNormalization(First276ExclusiveStorage actual,First276SourceBinding source,CancellationToken token)
            {
                RequireOwnedNumericalData(actual,source,token);
                if(Interlocked.CompareExchange(ref normalizationPhase,1,0)!=0)
                    throw new InvalidOperationException("Same FIRST normalization cannot restart.");
            }
            internal void CompleteNormalization(First276ExclusiveStorage actual,First276SourceBinding source,bool written,int c4,CancellationToken token)
            {
                RequireOwnedNumericalData(actual,source,token);
                if(written)normalizationC4=c4;
                if(Interlocked.CompareExchange(ref normalizationPhase,written?2:3,1)!=1)
                    throw new InvalidOperationException("FIRST normalization write chronology changed.");
            }
            internal int RequireNormalizedData(First276ExclusiveStorage actual,First276SourceBinding source,CancellationToken token)
            {
                RequireOwnedNumericalData(actual,source,token);
                if(normalizationPhase is not(2 or 3))throw new InvalidOperationException("Completed native normalization/no-op required.");
                return normalizationPhase;
            }
            internal int ReadNormalizedC4(First276ExclusiveStorage actual,First276SourceBinding source,CancellationToken token)
            {
                if(RequireNormalizedData(actual,source,token)!=2)
                    throw new InvalidOperationException("Native C4 is unknown on maximum0 no-op.");
                return normalizationC4;
            }
            private void StopNormalizedBoundary(int end,int count,CancellationToken token)
            {
                Charge(1572,82+3114,40960+64*257+BoundarySourceUpper,token);RequireProducer(token);
                var normalized=storage.CopyNormalizedData(this,token);
                // Do not allocate duplicate input/hidden/EXP snapshots here.
                var prefix=new First276InputBoundaryData(end,count,selectedCount,rangeCount,w,cached,descriptorCount,
                    binding.owner.descriptorCount,binding.owner.resultCount,featureCount,bytes,copies,work,true,false,null);
                throw new First276NormalizedBoundaryException(prefix,normalized);
            }
        }
    }
    // NativeC4 is readable ONLY when HasNormalizedOutputs is true. On the
    // maximum0 no-op NativeC4 stays null and no C4 knownness is asserted.
    internal sealed record First276NormalizedDiagnosticData(int NativeC0,int? NativeC4,bool HasNormalizedOutputs,
        bool ZeroMaximumNoOp,short[]? Quantized90,int[]? ProbabilityA0);
    internal sealed class First276NormalizedBoundaryException:NotSupportedException
    {
        internal First276InputBoundaryData Prefix {get;}
        internal First276NormalizedDiagnosticData Normalized {get;}
        internal First276NormalizedBoundaryException(First276InputBoundaryData prefix,First276NormalizedDiagnosticData normalized)
            :base("Actual FIRST normalization boundary; AL scoring/shortlist continuation remains unported."){Prefix=prefix;Normalized=normalized;}
    }
}
