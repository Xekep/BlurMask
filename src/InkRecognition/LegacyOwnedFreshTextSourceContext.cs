using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyFreshTextSourceContext.Probe")]

namespace InkRecognition;

/// <summary>One pending C4D selection-registration command. Its identity and
/// links are portable transaction identities, never native addresses. The
/// command has not been applied to an external document or SDK page.</summary>
public sealed class LegacyOwnedFreshTextSelection
{
    private readonly LegacyNativeInkPositionRange[] ranges;
    public LegacyOwnedFreshTextSourceContext Owner { get; }
    public long SelectionId { get; }
    public string Name { get; }
    public int CommandIdentity { get; }
    public byte OperationCode => 4;
    public byte Enabled => 1;
    public int Priority => 100;
    public int PreviousCommandIdentity => CommandIdentity - 1;
    internal LegacyOwnedFreshTextSelection(LegacyOwnedFreshTextSourceContext owner,long id,
        string name,int commandIdentity,LegacyNativeInkPositionRange[] ranges)
    {Owner=owner;SelectionId=id;Name=name;CommandIdentity=commandIdentity;this.ranges=ranges;}
    public LegacyNativeInkPositionRange[] CopyRanges() => ranges.ToArray();
}

/// <summary>Fresh noncached0210C0 document-selection context. The original
/// raw object IDs belong to this library's NEW immutable root. Native C51
/// insertion derives Next=N from those IDs; C4D's empty command list is a
/// separate owner. Existing/prior/cached document branches are not accepted.</summary>
public sealed class LegacyOwnedFreshTextSourceContext
{
    private readonly object gate=new();
    private readonly List<LegacyOwnedFreshTextSelection> commands=[];
    private long next;
    private bool failed;
    public LegacyOwnedRawInkDocument Document { get; }
    public long NextSelectionId {get{lock(gate)return next;}}
    public bool Failed {get{lock(gate)return failed;}}
    private LegacyOwnedFreshTextSourceContext(LegacyOwnedRawInkDocument document)
    {
        Document=document;
        //09E4C0 starts the C51 registry at zero.09EE20 NEW explicit key K
        //updates Next to K+1 iff Next<=K. Preserve the real original IDs,
        //rather than sampled/current physical ordinals or an assumed zero.
        for(long id=0;id<document.ObjectCount;id++)if(next<=id)next=id+1;
    }
    public static LegacyOwnedFreshTextSourceContext Prepare(LegacyOwnedRawInkDocument actualDocument,
        CancellationToken token=default)
    {ArgumentNullException.ThrowIfNull(actualDocument);token.ThrowIfCancellationRequested();return new(actualDocument);}

    public LegacyOwnedFreshTextSelection CreateCharacterSelection(LegacyOwnedMainGlyphDocumentPositions actualPositions,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualPositions);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(actualPositions.Document,Document)||
            !ReferenceEquals(actualPositions.Result.Owner.SourceDocument,Document))
            throw new ArgumentException("Positions from this identical NEW raw root required.");
        long output=-1;
        return CreateCore(actualPositions.Ranges.ToArray(),0,ref output,0,token) ??
            throw new InvalidOperationException("Fresh source selection registration failed.");
    }

    public IReadOnlyList<LegacyOwnedFreshTextSelection> CopyPendingSelections()
    {lock(gate)return Array.AsReadOnly(commands.ToArray());}

    //Explicit allocator-stage failures and arbitrary already-prepared keys
    //are numeric controls only; production accepts actual owned positions.
    internal LegacyOwnedFreshTextSelection? CreatePrepared(ReadOnlySpan<LegacyNativeInkPositionRange> ranges,
        int kind,ref long outputId,int failureStage=0,CancellationToken token=default)
        =>CreateCore(ranges.ToArray(),kind,ref outputId,failureStage,token);

    private LegacyOwnedFreshTextSelection? CreateCore(LegacyNativeInkPositionRange[] ranges,int kind,
        ref long outputId,int failureStage,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(ranges.Length>65536||kind is <0 or >2||failureStage is <0 or >4)
            throw new ArgumentOutOfRangeException(nameof(ranges));
        lock(gate)
        {
            if(failed)throw new InvalidOperationException("Failed native prefix retained; portable retry is unsupported.");
            if(commands.Count==65536)throw new NotSupportedException("Portable transaction budget exceeded.");
            token.ThrowIfCancellationRequested();
            //0210C0 creates the temporary C22 before this point, but consumes
            //the counter even if that allocation failed. Caller output is
            //written only after the registration command succeeds.
            long id=next;next=unchecked(next+1);
            if(failureStage!=0){failed=true;return null;}
            try
            {
                string name=kind==2?"TEXT":kind==1?"WORD":"CHAR";
                var command=new LegacyOwnedFreshTextSelection(this,id,name,commands.Count+1,ranges);
                commands.Add(command);outputId=id;return command;
            }
            catch {failed=true;throw;}
        }
    }
}
