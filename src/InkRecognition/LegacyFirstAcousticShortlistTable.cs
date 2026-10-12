using System.Buffers.Binary;

namespace InkRecognition;

public readonly record struct LegacyFirstAcousticShortlistCandidate(ushort Symbol,float Likelihood);
public readonly record struct LegacyFirstAcousticShortlistCell(int TotalCost,int AcousticCost,byte Likelihood,
    ushort Symbol,ushort ArcOrdinal);

/// <summary>Fresh alias-fold-disabled A8 producer inside1ef070. Groups follow
/// first finite primary arc encounter; labels come from its ranked top20.
/// Only fields0/4/8/C/10 are written. Other40-byte fields remain opaque.
/// This is not either B0/B8 context pass or terminal273 selection.</summary>
public sealed class LegacyFirstAcousticShortlistTable
{
    private readonly byte[] storage;
    private readonly int[] primaryOrdinals,counts;
    public int BeamWidth {get;}
    public int MaximumAlternatives {get;}
    public int GroupCount {get;}
    private LegacyFirstAcousticShortlistTable(byte[] storage,int[] primaryOrdinals,int[] counts,int width,int cap,int groups)
    {this.storage=storage;this.primaryOrdinals=primaryOrdinals;this.counts=counts;BeamWidth=width;MaximumAlternatives=cap;GroupCount=groups;}

    /// <summary>Caller establishes fresh23f aliasfold88=0 and its A0 cap.
    /// Provider must return an owned native ordered top20 shortlist. Snapshot
    /// primary and output storage prevents callbacks changing retained ownership.</summary>
    public static LegacyFirstAcousticShortlistTable Build(LegacyFirstAcousticRecordTable primary,
        int maximumAlternatives,ushort fallbackSymbol,ushort closedShapeSymbol,
        Func<ushort,ReadOnlyMemory<LegacyFirstAcousticShortlistCandidate>> candidates,
        ReadOnlySpan<byte> initialStorage=default,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(primary);ArgumentNullException.ThrowIfNull(candidates);
        cancellationToken.ThrowIfCancellationRequested();
        if(maximumAlternatives is <1 or >20)throw new ArgumentOutOfRangeException(nameof(maximumAlternatives));
        int bytes=checked(primary.Capacity*maximumAlternatives*40);
        if(initialStorage.Length!=0&&initialStorage.Length!=bytes)throw new ArgumentException("FixedF8*A0 forty-byte storage required.");
        byte[] storage=initialStorage.Length==0?new byte[bytes]:initialStorage.ToArray();
        byte[] source=primary.CopyStorage();int sourceCount=primary.Count;
        var seen=new HashSet<ushort>();int groups=0;var owners=Enumerable.Repeat(-1,primary.Capacity).ToArray();var counts=new int[primary.Capacity];
        for(int ordinal=0;ordinal<sourceCount;ordinal++)
        {
            cancellationToken.ThrowIfCancellationRequested();var row=source.AsSpan(ordinal*40,40);
            int total=BinaryPrimitives.ReadInt32LittleEndian(row);if(total>=0x4000000)break;
            ushort arc=BinaryPrimitives.ReadUInt16LittleEndian(row[16..]);if(!seen.Add(arc))continue;
            cancellationToken.ThrowIfCancellationRequested();var supplied=candidates(arc);
            cancellationToken.ThrowIfCancellationRequested();
            if(supplied.Length>20)throw new ArgumentException("Native A8 provider is bounded to top20.");
            var shortlist=supplied.ToArray();var symbols=new HashSet<ushort>();int alternative=0;
            foreach(var entry in shortlist)
            {
                cancellationToken.ThrowIfCancellationRequested();if(alternative==maximumAlternatives||entry.Symbol==ushort.MaxValue)break;
                if(!float.IsFinite(entry.Likelihood)||entry.Likelihood is <0 or >1||!symbols.Add(entry.Symbol))
                    throw new ArgumentException("Distinct finite native normalized shortlist probabilities required.");
                if(entry.Symbol==fallbackSymbol||entry.Symbol==closedShapeSymbol)continue;
                var cell=storage.AsSpan((groups*maximumAlternatives+alternative)*40,40);
                BinaryPrimitives.WriteInt32LittleEndian(cell,total);
                BinaryPrimitives.WriteInt32LittleEndian(cell[4..],BinaryPrimitives.ReadInt32LittleEndian(row[4..]));
                cell[8]=(byte)Math.Clamp((int)(entry.Likelihood*255f),1,255);
                BinaryPrimitives.WriteUInt16LittleEndian(cell[12..],entry.Symbol);
                BinaryPrimitives.WriteUInt16LittleEndian(cell[16..],arc);alternative++;
            }
            owners[groups]=ordinal;counts[groups]=alternative;groups++;
        }
        for(int group=0;group<primary.Capacity;group++)for(int alternative=counts[group];alternative<maximumAlternatives;alternative++)
        {
            cancellationToken.ThrowIfCancellationRequested();BinaryPrimitives.WriteInt32LittleEndian(storage.AsSpan((group*maximumAlternatives+alternative)*40),0x4000000);
        }
        return new(storage,owners,counts,primary.Capacity,maximumAlternatives,groups);
    }

    public static LegacyFirstAcousticShortlistTable BuildFromRanked(LegacyFirstAcousticRecordTable primary,
        int maximumAlternatives,ushort fallbackSymbol,ushort closedShapeSymbol,
        Func<ushort,LegacyOrdinaryRankedAcousticGroup> ranked,ReadOnlySpan<byte> initialStorage=default,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(ranked);
        return Build(primary,maximumAlternatives,fallbackSymbol,closedShapeSymbol,arc=>
        {
            var group=ranked(arc)??throw new ArgumentException("Ranked provider returned null.");
            cancellationToken.ThrowIfCancellationRequested();return group.Entries.Span[..Math.Min(20,group.Entries.Length)].ToArray()
                .Select(e=>new LegacyFirstAcousticShortlistCandidate(e.Symbol,e.NormalizedLikelihood)).ToArray();
        },initialStorage,cancellationToken);
    }

    public int GetPrimaryOrdinal(int group)
    {if((uint)group>=(uint)GroupCount)throw new ArgumentOutOfRangeException(nameof(group));return primaryOrdinals[group];}
    public int GetFiniteAlternativeCount(int group)
    {if((uint)group>=(uint)BeamWidth)throw new ArgumentOutOfRangeException(nameof(group));return counts[group];}
    public LegacyFirstAcousticShortlistCell GetCell(int group,int alternative)
    {
        if((uint)group>=(uint)BeamWidth||(uint)alternative>=(uint)MaximumAlternatives)throw new ArgumentOutOfRangeException(nameof(group));
        var cell=storage.AsSpan((group*MaximumAlternatives+alternative)*40,40);
        return new(BinaryPrimitives.ReadInt32LittleEndian(cell),BinaryPrimitives.ReadInt32LittleEndian(cell[4..]),cell[8],
            BinaryPrimitives.ReadUInt16LittleEndian(cell[12..]),BinaryPrimitives.ReadUInt16LittleEndian(cell[16..]));
    }
    public byte[] CopyStorage()=>storage.ToArray();
}
