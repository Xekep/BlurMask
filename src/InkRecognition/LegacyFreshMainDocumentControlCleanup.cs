using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMainDocumentControlCleanup.Probe")]

namespace InkRecognition;

/// <summary>Actual023260 no-control branch after fresh020920. Every WORD48
/// row and CHAR source flag is read from its retained owner; active100000
/// processing requires022960/034D90 and is explicitly unsupported. The WORD
/// flag-clear prefix precedes the CHAR pass. Transform/prior/final2E7 handoff
/// and document acceptance remain separate.</summary>
public sealed class LegacyFreshMainDocumentControlCleanup
{
    private sealed class Entry {internal bool Claimed;}
    private static readonly ConditionalWeakTable<LegacyFreshMainDocumentSourceIndices,Entry> lifetime=new();
    private readonly object gate=new();
    private readonly List<int> characterOrder=[];
    private bool entered,succeeded,faulted,wordCleared;
    private uint wordFlags;
    private int rowsInspected;
    public LegacyFreshMainDocumentSourceIndices SourceIndices {get;}
    public LegacyFreshMainDocumentSelectedSources State=>SourceIndices.State;
    public bool Succeeded {get{lock(gate)return succeeded;}}
    public bool IsFaulted {get{lock(gate)return faulted;}}
    public bool WordMaskCleared {get{lock(gate)return wordCleared;}}
    public uint WordFlags {get{lock(gate)return wordFlags;}}
    public int WordRowsInspected {get{lock(gate)return rowsInspected;}}
    public IReadOnlyList<int> CopyCharacterVisitOrder(){lock(gate)return Array.AsReadOnly(characterOrder.ToArray());}
    private LegacyFreshMainDocumentControlCleanup(LegacyFreshMainDocumentSourceIndices indices)
    {SourceIndices=indices;wordFlags=indices.State.WordFlags;}
    public static LegacyFreshMainDocumentControlCleanup Prepare(LegacyFreshMainDocumentSourceIndices actualIndices,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualIndices);actualIndices.ValidateCurrent(token);
        var entry=lifetime.GetValue(actualIndices,_=>new());
        lock(entry){token.ThrowIfCancellationRequested();if(entry.Claimed)throw new InvalidOperationException("This actual023260 lifetime has already been claimed.");
            entry.Claimed=true;return new(actualIndices);}
    }
    public LegacyFreshMainDocumentControlCleanup Run(CancellationToken token=default)
    {
        lock(gate)
        {
            token.ThrowIfCancellationRequested();if(entered)throw new InvalidOperationException("Control cleanup is single-use; failure prefixes cannot be reset.");
            SourceIndices.ValidateCurrent(token);entered=true;
            try
            {
                //The complete current WORD source list is singleton by the
                //owning index gate. Native scans every48B choice, not only
                //the selected ALT's row, before clearing WORDpayload18.
                foreach(var row in State.OriginalWordSource.Choices)
                {
                    token.ThrowIfCancellationRequested();rowsInspected++;
                    if((row.Flags&0x100000)!=0)
                        throw new NotSupportedException("Actual WORD choice requires native022960 control expansion; its owner service is not available.");
                }
                wordFlags&=0xffefffff;wordCleared=true;
                var byId=State.Characters.ToDictionary(c=>c.OriginalSource.SourceNode.Identity);
                foreach(var node in State.Characters[0].OriginalSource.SourceNode.Owner.CopyInsertionOrder())
                {
                    token.ThrowIfCancellationRequested();characterOrder.Add(node.Identity);
                    if((byId[node.Identity].Flags&0x100000)!=0)
                        throw new NotSupportedException("Actual CHAR source requires native023260 record deletion/034D90 owner updates; no guessed source or string replacement is performed.");
                }
                SourceIndices.ValidateCurrent(token);succeeded=true;return this;
            }
            catch{faulted=true;throw;}
        }
    }
    internal void ValidateCurrent(CancellationToken token=default)
    {lock(gate){token.ThrowIfCancellationRequested();if(!succeeded||faulted)throw new InvalidOperationException("Actual023260 no-control passes must complete.");SourceIndices.ValidateCurrent(token);}}
}
