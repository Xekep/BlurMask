using System.Buffers.Binary;
namespace InkRecognition;

/// <summary>NEW actual MAIN-only numeric cache. Old FIRST/RU cache guards and public APIs remain unchanged.</summary>
internal sealed class LegacyEnglishMainClassCacheKernel
{
    private const int Impossible=0x4000000;
    private readonly LegacyOrdinaryAcousticCacheProfile profile;
    private readonly LegacyInkAlphabet alphabet;
    private readonly ushort[] header;
    private readonly Dictionary<string,ushort> labels=new(StringComparer.Ordinal);
    public int CacheWidth=>alphabet.LabelCount+3;
    private readonly LegacyEnglishMainOnlySourceMasks masks;
    private readonly LegacyEnglishMainAcousticModel model;
    private readonly byte[] mask;
    private readonly LegacySymbolMaskKnowledgeGroup[] groups;
    internal LegacyEnglishMainClassCacheKernel(LegacyEnglishMainAcousticModel model,LegacyEnglishMainOnlySourceMasks masks)
    {
        if(!ReferenceEquals(model.Source,masks.Source))throw new ArgumentException("Same actual MAIN source required.");
        this.model=model;this.masks=masks;mask=masks.CopyPrimaryMask();groups=masks.Knowledge.CopyGroups();
        var profile=model.CacheProfile;var alphabet=model.Source.Alphabet;ReadOnlySpan<byte> serializedAlphabetHeader=alphabet.CopySerializedHeader();
        ArgumentNullException.ThrowIfNull(profile);ArgumentNullException.ThrowIfNull(alphabet);profile.Validate();
        if(serializedAlphabetHeader.Length<16)throw new InvalidDataException("Missing eight-field alphabet header.");
        header=new ushort[8];for(int i=0;i<8;i++)header[i]=BinaryPrimitives.ReadUInt16LittleEndian(serializedAlphabetHeader[(i*2)..]);
        if(header[7]!=alphabet.LabelCount||alphabet.LabelCount>4093)throw new InvalidDataException("Alphabet/cache header mismatch.");
        foreach(ushort symbol in header.AsSpan(0,7))if(symbol!=65535&&symbol>=alphabet.LabelCount)throw new InvalidDataException("Special header index outside alphabet.");
        this.profile=profile;this.alphabet=alphabet;
        foreach(string text in new[]{"-","_","\u30fc","\u4e00","\U000f0001","\U000f0002","\U000f0003","\uFFFD","o","O","0","\u00ad",".","\'","\"",",","\u3001"})
            if(alphabet.TryGetLabel(text,out ushort id))labels[text]=id;
    }

    internal LegacyOrdinaryAcousticClassCacheResult Build(ReadOnlySpan<int> neuralCosts,
        LegacyEnglishMainGlyphFeatures feature,int span,CancellationToken cancellationToken)
    {
        if(!ReferenceEquals(masks.Source,model.Source)||span is<0 or>7||!masks.Knowledge.SizeCostsEnabled||model.FreshMode13Flags!=0x21400)
            throw new ArgumentException("Actual fresh same-source MAIN cache required.");
        IReadOnlyList<LegacyInkStroke> normalizedGlyph=feature.Merged;
        byte[] enabledSymbols=mask;
        var context=new LegacyOrdinaryAcousticCacheContext(model.PruneCandidates,false,false,feature.WholePhysicalStroke,feature.EndpointKeysZero,feature.SourceProbability,false,ReadOnlyMemory<ushort>.Empty);
        ReadOnlySpan<LegacyAcousticCachePair> nativePairs=[];
        var orderedGroups=groups;
        ArgumentNullException.ThrowIfNull(normalizedGlyph);ArgumentNullException.ThrowIfNull(context);cancellationToken.ThrowIfCancellationRequested();
        if(neuralCosts.Length<1||neuralCosts.Length>alphabet.LabelCount||enabledSymbols.Length!=alphabet.LabelCount||normalizedGlyph.Count is <1 or >256||nativePairs.Length>4096||context.ScopePenaltyLabels.Length>4096)
            throw new ArgumentException("Matching bounded neural costs, alphabet mask, glyph and explicit pair list required.");
        foreach(byte flag in enabledSymbols)if(flag>1)throw new ArgumentException("Symbol mask requires0/1.");
        LegacyAcousticPositiveGroupSmoothing.Validate(orderedGroups,alphabet.LabelCount,cancellationToken);
        foreach(int cost in neuralCosts)if(cost is <0 or >Impossible)throw new ArgumentException("Expected raw nonnegative ensemble costs.");
        int pointCount=0;foreach(var stroke in normalizedGlyph){ArgumentNullException.ThrowIfNull(stroke);pointCount+=stroke.Points.Length;if(stroke.Points.Length==0||pointCount>4096)throw new ArgumentException("Empty/oversized normalized glyph.");}
        var costs=Enumerable.Repeat(Impossible,CacheWidth).ToArray();neuralCosts.CopyTo(costs);
        for(int i=0;i<neuralCosts.Length;i++)if(enabledSymbols[i]==0)costs[i]=Impossible;
        if(header[0]!=65535)costs[header[0]]=Impossible; //1b9e20 disables raw alphabet+8.
        foreach(ushort id in context.ScopePenaltyLabels.Span){Check(id);costs[id]=checked(costs[id]+8192);}

        // Local functions cannot capture a caller span; retain the checked mask.
        byte[] enabledSymbolsCopy=enabledSymbols;
        bool Enabled(string text)=>labels.TryGetValue(text,out ushort id)&&enabledSymbolsCopy[id]!=0;
        bool IsEnabled(ushort id)=>id!=65535&&enabledSymbolsCopy[id]!=0;
        void Assign(string text,int cost){if(labels.TryGetValue(text,out ushort id)&&IsEnabled(id))costs[id]=Math.Min(Impossible,cost);}

        if(header[1]!=65535&&costs[header[1]]<Impossible)
        {
            int baseCost=costs[header[1]];Assign("-",baseCost);Assign("_",baseCost);
            if(Enabled("\u30fc"))Assign("\u30fc",AddLogWeight(baseCost,profile.LongDashWeight));
            if(Enabled("\u4e00"))Assign("\u4e00",AddLogWeight(baseCost,profile.CjkDashWeight));
            if(Enabled("\U000f0002")||Enabled("\U000f0003"))
            {
                float negative=0,positive=0;
                foreach(var stroke in normalizedGlyph)
                {
                    cancellationToken.ThrowIfCancellationRequested();var points=stroke.Points;
                    for(int i=1;i<points.Length;i++)
                    {
                        float dx=(float)points[i].X-points[i-1].X,dy=(float)points[i].Y-points[i-1].Y;
                        float weight=(float)Math.Sqrt((double)dx*dx+(double)dy*dy); //25bab0 hypotf, float output.
                        if(dx<0)negative+=weight;else if(dx>0)positive+=weight;else{negative+=weight*.5f;positive+=weight*.5f;}
                    }
                }
                float total=positive+negative,tolerance=Math.Max(Math.Abs(total)*2.3841858e-7f,1.1920929e-7f);
                if(Math.Abs(0-total)<=tolerance){negative=.5f;positive=.5f;}else{negative/=total;positive/=total;}
                Assign("\U000f0002",negative<=.1f ? Impossible:AddLogWeight(baseCost,negative/profile.NegativeDirectionDivisor));
                Assign("\U000f0003",positive<=.1f ? Impossible:AddLogWeight(baseCost,positive/profile.PositiveDirectionDivisor));
            }
            costs[header[1]]=Impossible;
        }
        if(header[2]==65535)
        {
            if(normalizedGlyph.Count==1&&context.IsWholePhysicalStroke&&Enabled("\U000f0001"))
            {
                float probability=context.SourceStrokeProbability??throw new NotSupportedException("Active whole-stroke primitive requires the supplied native source probability.");
                if(!float.IsFinite(probability)||probability<0||probability>1)throw new ArgumentException("Source stroke probability must lie in0..1.");
                Assign("\U000f0001",probability<=0 ? Impossible:RoundDouble(-Math.Log(probability)*profile.ClosedPrimitiveWeight*8192));
            }
        }
        else
        {
            if(Enabled("\U000f0001"))
            {
                float sum=Probability(costs[header[2]]);
                foreach(string text in new[]{"o","O","0"})if(labels.TryGetValue(text,out ushort id))sum+=Probability(costs[id]);
                Assign("\U000f0001",sum<=0 ? Impossible:RoundDouble(-Math.Log(sum)*8192));
            }
            costs[header[2]]=Impossible;
        }
        if(labels.TryGetValue("\uFFFD",out ushort replacement))costs[replacement]=Impossible;
        if(header[3]!=65535)
        {
            costs[alphabet.LabelCount]=context.SuppressFallback ? Impossible:checked(costs[header[3]]-(int)(profile.FallbackPenalty*-8192f));
            costs[header[3]]=Impossible;
        }
        if(!context.SkipOrdinaryKnowledgeRules)
        {
            //1BB020 positiveB8 before the whole-physical dot exception.
            int minX=short.MaxValue,minY=short.MaxValue,maxX=short.MinValue,maxY=short.MinValue;
            foreach(var stroke in normalizedGlyph)foreach(var p in stroke.Points){cancellationToken.ThrowIfCancellationRequested();minX=Math.Min(minX,p.X);minY=Math.Min(minY,p.Y);maxX=Math.Max(maxX,p.X);maxY=Math.Max(maxY,p.Y);}
            float extent=Math.Max(maxX-minX,maxY-minY);
            void SizePenalty(string text,float threshold)
            {
                if(extent>threshold&&labels.TryGetValue(text,out ushort id))
                {float value=(extent/threshold-1f)*8192f;int penalty=(int)(value<=0?value-.5f:value+.5f);costs[id]=checked(costs[id]+penalty);}
            }
            SizePenalty(".",100f*masks.Knowledge.DotSizeDivisor);
            foreach(string text in new[]{",","'","\"","\u3001"})SizePenalty(text,100f*masks.Knowledge.PunctuationSizeDivisor);
            if(normalizedGlyph.Count==1&&context.IsWholePhysicalStroke&&context.WholeStrokeEndpointCategoriesZero&&Enabled("."))
            {
                int best=costs[labels["."]];for(int i=0;i<alphabet.LabelCount;i++)best=Math.Min(best,costs[i]);costs[labels["."]]=checked(best-1);
            }
            if(context.ShareRelatedFormMinimum)
                throw new NotSupportedException("Native flagged related-form minimum requires its explicit derived relationship policy; it is not inferred here.");
            LegacyAcousticPositiveGroupSmoothing.ApplyInPlace(costs,enabledSymbolsCopy,orderedGroups,cancellationToken);
        }
        if(labels.TryGetValue("\'",out ushort apostrophe))costs[alphabet.LabelCount+1]=costs[apostrophe];
        if(labels.TryGetValue("\"",out ushort quote))costs[alphabet.LabelCount+2]=costs[quote];

        var retained=Prune(costs,context.PruneCandidates,cancellationToken);
        foreach(var pair in nativePairs)
        {
            Check(pair.First);Check(pair.Second);int a=costs[pair.First],b=costs[pair.Second];
            if(a==Impossible&&b<Impossible)costs[pair.First]=checked(b+1);
            else if(b==Impossible&&a<Impossible)costs[pair.Second]=checked(a+1);
        }
        //Actual FC=1 replaces physical source-count by actual graph span+1.
        int count=span+1;
        if(count!=1)
        {
            float factor=(float)Math.Pow(count,(double)masks.Knowledge.CachePower);
            double fallbackFactor=Math.Pow(count,1.2000000476837158d);
            for(int i=0;i<costs.Length;i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if(costs[i]>=Impossible)continue;
                if(i==alphabet.LabelCount){double value=costs[i]*fallbackFactor;costs[i]=value>=Impossible?Impossible:checked((int)value);}
                else{float value=(float)costs[i]*factor;costs[i]=value>=Impossible?Impossible:checked((int)value);}
            }
        }
        var candidates=retained.Where(c=>c.Symbol<alphabet.LabelCount)
            .Select(c=>new LegacyAcousticCacheCandidate(c.Symbol,costs[c.Symbol])).ToArray();
        Sort(candidates,cancellationToken); //1bc680/1b85b0 signed cost order, native equal swaps.
        if(labels.TryGetValue("\u00ad",out ushort hyphen)&&labels.TryGetValue("-",out ushort dash))costs[hyphen]=costs[dash];
        return new(costs,candidates,costs[alphabet.LabelCount]);

        void Check(ushort id){if(id>=alphabet.LabelCount)throw new ArgumentException("Explicit cache label outside alphabet.");}
    }

    private List<LegacyAcousticCacheCandidate> Prune(int[] costs,bool enabled,CancellationToken token)
    {
        int threshold=enabled ? Math.Min(profile.CandidateThreshold,LegacyAcousticFixedLog.ProbabilityCost(1)):Impossible;
        int capacity=enabled ? profile.CandidateLimit:150;
        var heap=Fill(threshold);
        bool rewrite=enabled;
        if(enabled&&heap.Count<profile.MinimumCandidates)
        {
            heap=Fill(checked(LegacyAcousticFixedLog.ProbabilityCost(1)*2));
            if(heap.Count<profile.MinimumCandidates)heap=Fill(Impossible);
            rewrite=heap.Count>=profile.MinimumCandidates;
        }
        if(rewrite)
        {
            int fallback=costs[alphabet.LabelCount],special=header[3]==65535 ? Impossible:costs[header[3]];
            Array.Fill(costs,Impossible);
            if(header[3]!=65535)costs[header[3]]=special;costs[alphabet.LabelCount]=fallback;
            foreach(var candidate in heap)costs[candidate.Symbol]=candidate.Cost;
        }
        return heap;

        List<LegacyAcousticCacheCandidate> Fill(int upper)
        {
            var h=new List<LegacyAcousticCacheCandidate>(capacity);
            for(int symbol=0;symbol<costs.Length;symbol++)
            {
                token.ThrowIfCancellationRequested();if(symbol==alphabet.LabelCount||costs[symbol]>=upper)continue;
                var item=new LegacyAcousticCacheCandidate((ushort)symbol,costs[symbol]);int position;
                if(h.Count==capacity)
                {
                    if(h[0].Cost<=item.Cost)continue;position=0;
                    while(position*2<h.Count-1)
                    {
                        int child=position*2+1;
                        if(child<h.Count-1&&h[child].Cost<=h[child+1].Cost)child++;
                        if(h[child].Cost<=item.Cost)break;h[position]=h[child];position=child;
                    }
                }
                else
                {
                    position=h.Count;h.Add(default);
                    while(position>0){int parent=(position-1)>>1;if(item.Cost<=h[parent].Cost)break;h[position]=h[parent];position=parent;}
                }
                h[position]=item;
            }
            return h;
        }
    }
    private static int AddLogWeight(int cost,float probability)=>probability<=0 ? Impossible:Math.Min(Impossible,checked(cost+RoundDouble(-Math.Log(probability)*8192)));
    private static float Probability(int cost)=>cost<Impossible ? (float)Math.Exp(-(float)cost*(1f/8192)):0;
    private static int RoundDouble(double value)
    {double rounded=value<=0 ? value-.5:value+.5;if(!double.IsFinite(rounded)||rounded<int.MinValue||rounded>=2147483648d)throw new ArgumentException("Native correction exceeds checked integer conversion.");return (int)rounded;}
    private static void Sort(LegacyAcousticCacheCandidate[] items,CancellationToken token)
    {
        if(items.Length<2)return;var stack=new Stack<(int,int)>();stack.Push((0,items.Length-1));
        while(stack.Count!=0)
        {
            token.ThrowIfCancellationRequested();var(first,last)=stack.Pop();int left=first,right=last,pivot=items[(first+last)/2].Cost;
            while(left<=right){while(items[left].Cost<pivot)left++;while(items[right].Cost>pivot)right--;if(left>right)break;(items[left],items[right])=(items[right],items[left]);left++;right--;}
            if(left<last)stack.Push((left,last));if(first<right)stack.Push((first,right));
        }
    }
}
