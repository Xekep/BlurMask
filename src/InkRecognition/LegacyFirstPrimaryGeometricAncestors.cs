using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Original acoustic parent and separately resolved geometric parent
/// in1EE3B0. A skipped root remains the geometric parent; its predecessor is
/// not read. Original cost/alternative never changes during this walk.</summary>
public readonly record struct LegacyFirstGeometricAncestor(int OriginalNode,int OriginalAlternative,
    LegacyFirstAcousticRecordFields OriginalRecord,int GeometryNode,int GeometryAlternative,
    LegacyFirstAcousticRecordFields GeometryRecord,int SkippedCount);

/// <summary>Owned fresh first zero-trait1EE3B0 previous-row walk. Produces only
/// the geometric ancestor, not native F0/F8 compatibility, proposals or winner.</summary>
public sealed class LegacyFirstPrimaryGeometricAncestors
{
    private readonly LegacyFirstAcousticRecordTable[] rows;
    private readonly ushort blank,shadda,newline,space,nonbreakingSpace;
    private readonly bool modeE0;
    public LegacyOrdinaryGraphSourceRanges SourceRanges {get;}
    private LegacyFirstPrimaryGeometricAncestors(LegacyFirstAcousticRecordTable[] rows,
        LegacyOrdinaryGraphSourceRanges source,ushort blank,ushort shadda,ushort newline,ushort space,ushort nbsp,bool mode)
    {this.rows=rows;SourceRanges=source;this.blank=blank;this.shadda=shadda;this.newline=newline;this.space=space;nonbreakingSpace=nbsp;modeE0=mode;}

    public static LegacyFirstPrimaryGeometricAncestors Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshFirstResultOwnershipFactory factory,LegacySmoothedRowCutResult cuts,
        LegacyFreshFirstCandidateGraphPreparation prepared,LegacyFirstOrdinaryAcousticRecurrence primary,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(factory);ArgumentNullException.ThrowIfNull(cuts);ArgumentNullException.ThrowIfNull(prepared);
        ArgumentNullException.ThrowIfNull(primary);cancellationToken.ThrowIfCancellationRequested();
        var literal=LegacyFreshFirstLiteralContextPolicy.ReadSource(resource,factory.InvocationPolicy.Binding.SourceOffset,factory,cancellationToken);
        var source=prepared.SourceRanges;var graph=source.Graph;
        if(!ReferenceEquals(source.PreparedChronology,cuts.Chronology)||!ReferenceEquals(source.OriginalSource,factory.Frame.Row.OriginalSource)||
            !ReferenceEquals(prepared.Pruning.Graph,graph)||!ReferenceEquals(cuts.Owner,factory.Frame.Row)||primary.NodeCount!=graph.NodeCount||
            primary.BeamWidth!=factory.PrimaryBeamWidthF8)
            throw new ArgumentException("Matching fresh first primary dimensions and owned pruned source graph required.");
        int p=checked(factory.InvocationPolicy.Binding.SourceOffset+20+4+56);int marker=BinaryPrimitives.ReadInt32LittleEndian(resource[p..]);
        p+=4;if(marker==-1)p+=4;p+=11;
        var rows=new LegacyFirstAcousticRecordTable[primary.NodeCount];
        for(int n=0;n<rows.Length;n++)
        {
            cancellationToken.ThrowIfCancellationRequested();rows[n]=primary.GetRowCopy(n);
            for(int i=0;i<rows[n].Count;i++)
            {
                var r=rows[n].Get(i);var arcs=graph.GetNode(n);
                if(r.Traits!=0||r.Symbol>literal.Binding.Alphabet.LabelCount+2||r.ArcOrdinal>=arcs.Count||arcs[r.ArcOrdinal].Span!=r.Span||r.TotalCost>=0x4000000)
                    throw new ArgumentException("Finite owned zero-trait first records and matching graph arcs required.");
                int parent=n-r.Span-1;
                if(parent<-1||parent==-1&&r.PreviousAlternative!=255||parent>=0&&r.PreviousAlternative>=rows[parent].Count)
                    throw new InvalidDataException("First primary predecessor identity is invalid.");
                if(parent>=0&&!arcs[r.ArcOrdinal].PredecessorArcIndices.Span.Contains((int)rows[parent].Get(r.PreviousAlternative).ArcOrdinal))
                    throw new InvalidDataException("First primary predecessor is not in the supplied native graph.");
            }
        }
        return new(rows,source,checked((ushort)literal.Binding.Alphabet.LabelCount),literal.Binding.TrailingSymbolE6,
            BinaryPrimitives.ReadUInt16LittleEndian(resource[(p+8)..]),BinaryPrimitives.ReadUInt16LittleEndian(resource[(p+10)..]),
            BinaryPrimitives.ReadUInt16LittleEndian(resource[(p+12)..]),literal.Binding.NativeModeE0);
    }

    public LegacyFirstGeometricAncestor Resolve(int originalNode,int originalAlternative,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if((uint)originalNode>=(uint)rows.Length)throw new ArgumentOutOfRangeException(nameof(originalNode));
        var original=rows[originalNode].Get(originalAlternative);int n=originalNode,a=originalAlternative,skipped=0;
        var current=original;
        while(current.Symbol==blank||current.Symbol==shadda||!modeE0&&(current.Symbol==newline||current.Symbol==space||current.Symbol==nonbreakingSpace))
        {
            cancellationToken.ThrowIfCancellationRequested();int previous=n-current.Span-1;
            //Native OR condition stops at root before reading byte9's255.
            if(previous==-1)break;
            a=current.PreviousAlternative;n=previous;current=rows[n].Get(a);skipped++;
        }
        return new(originalNode,originalAlternative,original,n,a,current,skipped);
    }
}
