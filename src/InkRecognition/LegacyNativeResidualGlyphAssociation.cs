using System.Buffers.Binary;

namespace InkRecognition;

public readonly record struct LegacyNativeResidualSource(int Node,ushort Arc)
{
    public static LegacyNativeResidualSource Decode(ulong reference)
    {
        int node=(int)(reference&0xffffff);
        if((node&0x800000)!=0)node-=0x1000000;
        return new(node,(ushort)(reference>>24));
    }
}
public readonly record struct LegacyNativeResidualBounds(float MinimumX,float MinimumY,float MaximumX,float MaximumY);
public readonly record struct LegacyNativeResidualGuide(float Baseline,float Height);
public readonly record struct LegacyNativeResidualMerge(bool Succeeded,ulong Reference);
public delegate LegacyNativeResidualMerge LegacyNativeResidualMergeSources(ReadOnlySpan<ulong> references,CancellationToken token);

/// <summary>Explicit source services required by217DB0. Binding them to an
/// actual source/218 guide/group-merge owner is separate from this controller.</summary>
public sealed record LegacyNativeResidualAssociationOperations(
    Func<LegacyNativeResidualSource,byte> ReadKind,
    Func<LegacyNativeResidualSource,int> ReadLine,
    Func<LegacyNativeResidualSource,LegacyNativeResidualBounds> ReadBounds,
    Func<int,LegacyNativeResidualGuide> ReadGuide,
    Func<ushort,bool> ReadAlphabetFlag,
    Func<LegacyNativeResidualSource,bool> IsTopMark,
    LegacyNativeResidualMergeSources MergeSources);

public sealed class LegacyNativeResidualAssociationResult
{
    private readonly byte[] rows;private readonly ulong[] residual;
    public bool Succeeded {get;}
    public int RemainingCount {get;}
    public byte[] CopyRows()=>rows.ToArray();
    public ulong[] CopyResidualStorage()=>residual.ToArray();
    internal LegacyNativeResidualAssociationResult(bool succeeded,byte[] rows,List<ulong> residual,int remaining)
    {Succeeded=succeeded;this.rows=rows;this.residual=residual.ToArray();RemainingCount=remaining;}
}

/// <summary>217DB0 residual insertion, nearest-row assignment and reverse
/// source merging. Rows and services are explicitly prepared; this kernel
/// alone does not establish source ownership or actual21FFD0 segmentation.</summary>
public static class LegacyNativeResidualGlyphAssociation
{
    public static LegacyNativeResidualAssociationResult Apply(ReadOnlySpan<byte> actualRows72,
        ReadOnlySpan<ulong> actualResidualReferences,bool knowledge99,bool collectionC1,ushort alphabetCount,
        LegacyNativeResidualAssociationOperations operations,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(operations);ArgumentNullException.ThrowIfNull(operations.ReadKind);
        ArgumentNullException.ThrowIfNull(operations.ReadLine);ArgumentNullException.ThrowIfNull(operations.ReadBounds);
        ArgumentNullException.ThrowIfNull(operations.ReadGuide);ArgumentNullException.ThrowIfNull(operations.ReadAlphabetFlag);
        ArgumentNullException.ThrowIfNull(operations.IsTopMark);ArgumentNullException.ThrowIfNull(operations.MergeSources);
        token.ThrowIfCancellationRequested();
        if(actualRows72.Length%72!=0||actualRows72.Length/72>65536||actualResidualReferences.Length>65536)
            throw new ArgumentException("Bounded complete72-byte rows and residual source vector required.");
        byte[] rows=actualRows72.ToArray();var residual=actualResidualReferences.ToArray().ToList();
        int count=residual.Count,rowCount=rows.Length/72;
        if(count==0||knowledge99)return new(true,rows,residual,count);
        int processed=-1;
        for(int r=0;r<count;r++,processed++)
        {
            token.ThrowIfCancellationRequested();ulong reference=residual[r];var source=LegacyNativeResidualSource.Decode(reference);
            byte kind=operations.ReadKind(source);int selected=-1;
            if(kind==0)
            {
                var box=operations.ReadBounds(source);float best=float.MaxValue;
                for(int i=0;i<rowCount;i++)
                {
                    token.ThrowIfCancellationRequested();int offset=i*72;
                    if(rows[offset+60]!=0)continue;
                    var candidate=Source(offset);
                    if(operations.ReadKind(candidate)!=0)continue;
                    int line=operations.ReadLine(candidate);
                    if(line!=operations.ReadLine(source))continue;
                    var candidateBox=operations.ReadBounds(candidate);float distance;
                    if(collectionC1)
                        distance=candidate.Node<source.Node?(float)unchecked(source.Node-candidate.Node):float.MaxValue;
                    else
                    {
                        distance=((candidateBox.MaximumX+candidateBox.MinimumX)-(box.MaximumX+box.MinimumX))*.5f;
                        bool corrected=false;
                        if(line!=-1)
                        {
                            ushort label=U16(offset+58);var guide=operations.ReadGuide(line);
                            if(label<alphabetCount&&operations.ReadAlphabetFlag(label)&&operations.IsTopMark(source)&&
                                distance<0&&(box.MinimumY+box.MaximumY)*.5f<guide.Baseline-guide.Height*.75f)
                            {distance=MathF.Abs(distance)-(guide.Height+guide.Height);corrected=true;}
                        }
                        if(!corrected)distance=MathF.Abs(distance);
                    }
                    if(distance<best){best=distance;selected=i;}
                }
            }
            else if(kind==1)
            {
                int best=int.MaxValue;
                for(int i=0;i<rowCount;i++)
                {
                    token.ThrowIfCancellationRequested();int offset=i*72;
                    if(rows[offset+60]!=0)continue;
                    var candidate=Source(offset);if(operations.ReadKind(candidate)!=1)continue;
                    int distance=unchecked(source.Node-candidate.Node-1);
                    if(distance<0)distance=unchecked(candidate.Node-source.Node+1);
                    if(distance<best){best=distance;selected=i;}
                }
            }
            else return new(false,rows,residual,count);
            if(selected!=-1)
            {
                int insertion=I32(selected*72+64);
                if(insertion<0||insertion>=count)throw new InvalidDataException("Native residual insertion would escape its storage.");
                for(int i=processed;i>=insertion;i--)residual[i+1]=residual[i];
                //An unassociated preceding reference may leave insertion ahead
                //of the processed prefix. Preserve the actual indexed store.
                residual[insertion]=reference;
                for(int i=selected;i<rowCount;i++)W32(i*72+64,unchecked(I32(i*72+64)+1));
            }
        }
        for(int i=rowCount-1;i>=0;i--)
        {
            token.ThrowIfCancellationRequested();int offset=i*72;
            int start=i==0?0:I32(offset-8);
            int difference=unchecked(I32(offset+64)-start);
            if(difference==0)continue;
            if(count<residual.Count)residual[count]=U64(offset+48);else residual.Add(U64(offset+48));count++;
            int length=unchecked(difference+1);
            if(start<0||length<0||start>count-length)
                throw new InvalidDataException("Native residual merge would escape its storage.");
            ulong[] selected=residual.GetRange(start,length).ToArray();
            var merge=operations.MergeSources(selected,token);
            if(!merge.Succeeded)return new(false,rows,residual,0);
            BinaryPrimitives.WriteUInt64LittleEndian(rows.AsSpan(offset+48),merge.Reference);rows[offset+68]=1;
            if(start<count&&length!=0)
            {
                if(length<count-start)
                {
                    int remainder=count-length-start;
                    for(int j=0;j<remainder;j++)residual[start+j]=residual[start+length+j];
                    count-=length;
                }
                else count=start;
            }
        }
        return new(true,rows,residual,0);

        ushort U16(int at)=>BinaryPrimitives.ReadUInt16LittleEndian(rows.AsSpan(at));
        int I32(int at)=>BinaryPrimitives.ReadInt32LittleEndian(rows.AsSpan(at));
        ulong U64(int at)=>BinaryPrimitives.ReadUInt64LittleEndian(rows.AsSpan(at));
        void W32(int at,int value)=>BinaryPrimitives.WriteInt32LittleEndian(rows.AsSpan(at),value);
        LegacyNativeResidualSource Source(int at)=>new(I32(at),U16(at+4));
    }
}
