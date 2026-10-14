using System.Buffers.Binary;
using System.Collections.ObjectModel;

namespace InkRecognition;

/// <summary>Native1b0760 insert-only3A0 confidence map, consumed before
/// network D8 lookup by1bcbd0. Does not change output symbols, case or script.
/// Wide-pair/closed-primitive services remain distinct unresolved paths.</summary>
public sealed class LegacyAcousticConfidenceLabelMap
{
    private readonly Dictionary<ushort,ushort> aliases;
    private readonly ushort closedPrimitive,closedPrototype;
    public int OutputWidth {get;}
    public int SymbolCount {get;}
    public IReadOnlyDictionary<ushort,ushort> ExplicitMappings {get;}

    public LegacyAcousticConfidenceLabelMap(LegacyInkAlphabet alphabet,ReadOnlySpan<byte> serializedHeader16,
        int outputWidth,IReadOnlyList<LegacySymbolMaskKnowledgeGroup> orderedKnowledgeGroups,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(alphabet);ArgumentNullException.ThrowIfNull(orderedKnowledgeGroups);
        cancellationToken.ThrowIfCancellationRequested();
        if(serializedHeader16.Length!=16||outputWidth is <1 or >512||outputWidth>alphabet.LabelCount||alphabet.LabelCount>65533||
            BinaryPrimitives.ReadUInt16LittleEndian(serializedHeader16[14..])!=alphabet.LabelCount||orderedKnowledgeGroups.Count>512)
            throw new ArgumentException("Matching bounded alphabet/header, actual output width and ordered groups required.");
        ushort dash=BinaryPrimitives.ReadUInt16LittleEndian(serializedHeader16[2..]);
        closedPrototype=BinaryPrimitives.ReadUInt16LittleEndian(serializedHeader16[4..]);
        foreach(ushort value in new[]{dash,closedPrototype})if(value!=ushort.MaxValue&&value>=alphabet.LabelCount)
            throw new ArgumentException("Prototype header exceeds its owned alphabet.");
        SymbolCount=alphabet.LabelCount;OutputWidth=outputWidth;aliases=[];
        int entries=0;
        foreach(var group in orderedKnowledgeGroups)
        {
            ArgumentNullException.ThrowIfNull(group);entries=checked(entries+group.Symbols.Length);
            if(!float.IsFinite(group.Weight)||entries>65535)throw new ArgumentException("Finite bounded knowledge groups required.");
            foreach(ushort label in group.Symbols.Span)if(label>=SymbolCount)throw new ArgumentException("Group label outside alphabet.");
            if(group.Weight<=0)continue;
            ushort firstOutput=ushort.MaxValue;bool extended=false;
            foreach(ushort label in group.Symbols.Span)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if(label>=outputWidth)extended=true;
                else if(firstOutput==ushort.MaxValue)firstOutput=label;
            }
            if(!extended||firstOutput==ushort.MaxValue)continue;
            foreach(ushort label in group.Symbols.Span)if(label>=outputWidth)aliases.TryAdd(label,firstOutput);
        }
        if(dash!=ushort.MaxValue)
            foreach(string text in new[]{"-","\uff0d","_","\uff3f","\u00ad","\u30fc","\u4e00","\U000f0002","\U000f0003"})
            {
                cancellationToken.ThrowIfCancellationRequested();
                if(alphabet.TryGetLabel(text,out ushort label)&&!(text=="\u30fc"&&label<OutputWidth))aliases.TryAdd(label,dash);
            }
        ushort apostrophe=alphabet.TryGetLabel("'",out ushort apostropheLabel)?apostropheLabel:ushort.MaxValue;
        aliases.TryAdd((ushort)(SymbolCount+1),apostrophe);
        ushort quotation=alphabet.TryGetLabel("\"",out ushort quotationLabel)?quotationLabel:ushort.MaxValue;
        if(SymbolCount+2!=ushort.MaxValue)aliases.TryAdd((ushort)(SymbolCount+2),quotation);
        closedPrimitive=alphabet.TryGetLabel("\U000f0001",out ushort closed)?closed:ushort.MaxValue;
        ExplicitMappings=new ReadOnlyDictionary<ushort,ushort>(aliases);
    }

    /// <summary>Resolve only to an in-bounds actual neural output. An unknown
    /// label is not assigned zero probability: native D8 itself has no bounds
    /// guard. Closed primitive without a stored prototype requires its actual
    /// sampler90/source21860 probability service.</summary>
    public bool TryResolveOutputLabel(ushort symbol,out ushort outputLabel)
    {
        if(symbol>SymbolCount+2)throw new ArgumentOutOfRangeException(nameof(symbol));
        if(aliases.TryGetValue(symbol,out outputLabel))return outputLabel<OutputWidth;
        if(closedPrimitive!=ushort.MaxValue&&symbol==closedPrimitive){outputLabel=closedPrototype;return outputLabel<OutputWidth;}
        outputLabel=symbol;return symbol<OutputWidth;
    }

    /// <summary>The checked first v110 cache reader rejects nonzero ordered
    /// knowledge-group count. This factory derives that empty branch from the
    /// actual source; it is not an invented no-group policy for other models.</summary>
    public static LegacyAcousticConfidenceLabelMap ReadFreshFirstSource(ReadOnlySpan<byte> resource,
        int sourceOffset,CancellationToken cancellationToken=default)
    {
        _=LegacyOrdinaryAcousticCacheProfile.ReadFirstSource(resource,sourceOffset,out var model,cancellationToken);
        int p=sourceOffset+20+4+56;int marker=BinaryPrimitives.ReadInt32LittleEndian(resource[p..]);p+=4;if(marker==-1)p+=4;p+=4;
        if(resource[p++]!=1||BinaryPrimitives.ReadUInt32LittleEndian(resource[p..])!=0x2b)throw new NotSupportedException("Inline acoustic alphabet required.");
        p+=4;ushort version=BinaryPrimitives.ReadUInt16LittleEndian(resource[p..]);p+=2;
        if(version!=0x103)throw new NotSupportedException("Validated first alphabet103 required.");
        var alphabet=LegacyInkAlphabet.Load(resource[p..],version);
        if(LegacyWideSymbolPairs.CreateUnrestricted(alphabet,cancellationToken).Length!=0)
            throw new NotSupportedException("This first confidence profile needs its separate alphabet wide-pair resolver.");
        return new(alphabet,resource.Slice(p,16),model.TrajectoryNetwork.OutputWidth,[],cancellationToken);
    }
}
