using System.Buffers.Binary;
namespace InkRecognition;

internal readonly record struct LegacyFullEnglishFirstPlainCostObservation(int Group,ushort Symbol,int Category,int Prototype,ushort Ordinal,bool Top,bool Primitive,int Cost);

//Only canonical full terminal cells can issue this private literal factory.
internal sealed class LegacyFullEnglishFirstPlainResultFactory
{
    internal LegacyFullEnglishFirstTerminalChild Terminal{get;}
    internal LegacyFullEnglishFirstControllerChild Initializer=>Terminal.Context.Primary.Initializer;
    internal LegacyFullEnglishFirstCandidates Candidates=>Initializer.ActualCache.Owner;
    internal LegacyFullEnglishFirstPreprocessing Preparation=>Candidates.Preparation;
    internal LegacyOrdinaryCursiveCandidateGraph Graph=>Candidates.SourceRanges.Graph;
    internal LegacyCursiveCategoryProfile Categories{get;}
    internal LegacyOrdinaryRawInputDescriptor AssignedDescriptor{get;}
    internal IReadOnlyList<LegacyFirstSelectedGlyphCell> Cells=>Terminal.Cells;
    private readonly ushort headerE;
    private readonly Dictionary<int,LegacyFirstPlainGlyphCache> caches=[];
    private readonly Action<LegacyFullEnglishFirstPlainCostObservation>? observation;
    internal LegacyFullEnglishFirstPlainResultFactory(LegacyFullEnglishFirstTerminalChild child,LegacyCursiveCategoryProfile categories,Action<LegacyFullEnglishFirstPlainCostObservation>? observation,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();Terminal=child;Categories=categories;this.observation=observation;
        var model=Initializer.ActualCache.Model;var source=model.Source;var raw=Preparation.RawDescriptor;
        if(!ReferenceEquals(Preparation.Model,model)||!ReferenceEquals(Preparation.Source,source)||!ReferenceEquals(Candidates.SourceRanges.OriginalSource,Preparation.OriginalSource)||raw.Flags!=0x10||raw.CurrentLineId!=-1||raw.ScopeId!=-1||raw.FirstPhysicalStroke!=0||raw.LastPhysicalStroke!=Preparation.OriginalSource.Strokes.Count-1||child.Owner.Invocation.Configuration99||child.Owner.Invocation.ConfigurationA8)
            throw new ArgumentException("Same fresh full FIRST original/cut/assigned guide required.");
        AssignedDescriptor=raw with{CurrentLineId=0};headerE=BinaryPrimitives.ReadUInt16LittleEndian(source.Alphabet.CopySerializedHeader().AsSpan(6));
        foreach(var cell in Cells){token.ThrowIfCancellationRequested();if(cell.CopyRecord().Span[32]!=0||cell.Symbol>219)throw new NotSupportedException("Fresh full zero-trait literal symbol required.");}
    }
    internal LegacyFirstPlainTextStorage CreateStorage(CancellationToken token)
    {var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false,token);return new(pool,new LegacyNativePlainWordPublisher(pool,false,false,false));}
    internal bool ShouldAllocate(ushort symbol,CancellationToken token)
    {token.ThrowIfCancellationRequested();if(symbol>219)throw new ArgumentOutOfRangeException(nameof(symbol));return symbol!=217&&symbol!=211;}
    internal LegacyFirstLiteralGlyphSymbolPlan CreatePlan(ushort selected,byte composite,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(selected>219||composite>63)throw new ArgumentOutOfRangeException(nameof(selected));ushort published=selected==217?headerE:selected;
        if(published>=Categories.StartCompositeMap.Length)throw new InvalidDataException("Own literal category required.");
        int category=Categories.StartCompositeMap.Span[published];byte geometry=category<0?(byte)0:checked((byte)Categories.CompositeCategories.Span[category].GeometryIndex);
        return new(selected,published,category,geometry,published==headerE||category==composite?(ushort)0:(ushort)8,published==209?(ushort)0x48:(ushort)0x40);
    }
    internal void ApplySymbol(LegacyNativeGlyphRecordPool pool,ulong reference,LegacyFirstLiteralGlyphSymbolPlan plan,byte composite,CancellationToken token)
    {if(plan!=CreatePlan(plan.SelectedLabel,composite,token))throw new ArgumentException("Foreign literal plan.");LegacyNativeFirstLiteralGlyphKernel.Apply(pool,reference,plan,token);}
    internal LegacyFirstPlainGlyphCache Cache(int group,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(caches.TryGetValue(group,out var found))return found;
        var actual=Initializer.ActualCache.ScoreGroup(group);var confidence=Initializer.Owner.Owner.ReadGroup(actual);var ranked=Initializer.ReadGroup(actual);
        if(!ReferenceEquals(actual.Owner,Initializer.ActualCache)||!ReferenceEquals(confidence.ActualCache,actual)||!ReferenceEquals(ranked.ActualCache,actual))throw new ArgumentException("Same full cache/confidence/ranked group required.");
        var candidates=actual.CopyCandidates();var values=confidence.CopyValues();if(candidates.Length!=values.Length||values.Length>10)throw new InvalidDataException("Bounded full confidence required.");
        var scores=new Dictionary<ushort,float>();for(int i=0;i<candidates.Length;i++){token.ThrowIfCancellationRequested();scores.TryAdd(candidates[i].Symbol,values[i]);}
        float first=values.Length==0?0:values[0];
        float Score(ushort symbol,int channel,CancellationToken t){t.ThrowIfCancellationRequested();if(channel!=0)throw new NotSupportedException("Fresh owner88 channel0 required.");return scores.GetValueOrDefault(symbol);}
        int Cost(ReadOnlySpan<byte> request,CancellationToken t)
        {
            t.ThrowIfCancellationRequested();if(request.Length!=16)throw new ArgumentException("Owned literal request16 required.");
            ushort symbol=BinaryPrimitives.ReadUInt16LittleEndian(request[8..]),ordinal=BinaryPrimitives.ReadUInt16LittleEndian(request[6..]);int category=Categories.StartCompositeMap.Span[symbol];int prototype=category<0?0:Categories.CompositeCategories.Span[category].GeometryIndex;
            if(ordinal!=65535||(sbyte)request[10]!=prototype||request[11]!=1||request[12]!=0)throw new ArgumentException("Actual published-category/prototype literal request required.");
            int cost=ranked.PreparedClassQuery(symbol,ordinal,prototype,true,false,t);observation?.Invoke(new(group,symbol,category,prototype,ordinal,true,false,cost));return cost;
        }
        found=new(Score,Cost,t=>{t.ThrowIfCancellationRequested();return first;});caches.Add(group,found);return found;
    }
}
