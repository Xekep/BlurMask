using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyGlobalControlCleanup.Probe")]
namespace InkRecognition;

public readonly record struct LegacyGlobalControlCleanupPrefix(string Stage,int WordRows,bool WordMaskCleared,uint WordFlags,int Characters);
internal enum LegacyGlobalControlCleanupStage { Entered,WordRow,WordCleared,Character,Complete }

/// <summary>Genuine global023260 no-control branch after both020920 passes.
/// Active WORD expansion/CHAR deletion still require their unavailable services.
/// No accepted document, input transform or native failure rollback is implied.</summary>
public sealed class LegacyFreshMainGlobalControlCleanup
{
    private sealed class Entry {internal bool Claimed;}
    private static readonly ConditionalWeakTable<LegacyFreshMainGlobalDocumentSourceIndices,Entry> lifetimes=new();
    private readonly object gate=new();
    private readonly List<int> characters=[];
    private readonly List<LegacyGlobalControlCleanupPrefix> prefixes=[];
    private bool entered,complete,faulted,wordCleared;
    private uint wordFlags;
    private int rows;
    public LegacyFreshMainGlobalDocumentSourceIndices SourceIndices{get;}
    public LegacyFreshMainGlobalDocumentSelectedSources State=>SourceIndices.State;
    public bool IsComplete{get{lock(gate)return complete;}}
    public bool IsFaulted{get{lock(gate)return faulted;}}
    public uint WordFlags{get{lock(gate)return wordFlags;}}
    public bool WordMaskCleared{get{lock(gate)return wordCleared;}}
    public int WordRowsInspected{get{lock(gate)return rows;}}
    private LegacyFreshMainGlobalControlCleanup(LegacyFreshMainGlobalDocumentSourceIndices indices)
    {SourceIndices=indices;wordFlags=indices.State.WordFlags;}
    public static LegacyFreshMainGlobalControlCleanup Prepare(LegacyFreshMainGlobalDocumentSourceIndices indices,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(indices);indices.ValidateCurrent(token);
        var entry=lifetimes.GetValue(indices,_=>new());
        lock(entry)
        {
            token.ThrowIfCancellationRequested();indices.ValidateCurrent(token);
            if(entry.Claimed)throw new InvalidOperationException("This actual global023260 owner is already claimed.");
            var result=new LegacyFreshMainGlobalControlCleanup(indices);entry.Claimed=true;return result;
        }
    }
    public LegacyFreshMainGlobalControlCleanup Run(CancellationToken token=default)=>RunCore(null,token);
    internal LegacyFreshMainGlobalControlCleanup RunDiagnostic(Action<LegacyGlobalControlCleanupStage>? observer,CancellationToken token=default)=>RunCore(observer,token);
    private LegacyFreshMainGlobalControlCleanup RunCore(Action<LegacyGlobalControlCleanupStage>? observer,CancellationToken token)
    {
        lock(gate)
        {
            token.ThrowIfCancellationRequested();if(entered)throw new InvalidOperationException("Global control cleanup is single-use, including failed prefixes.");
            SourceIndices.ValidateCurrent(token);entered=true;
            try
            {
                Record("entered");Observe(LegacyGlobalControlCleanupStage.Entered);
                //The genuine global publication admits exactly one ALT/WORD48.
                //Read its actual row8 flags, not the selected-word aggregate.
                if(State.Publication.AlternativeCount!=1)throw new NotSupportedException("Complete actual singleton WORD choice vector required.");
                byte[] word48=State.Publication.WordSource.CopyChoiceScalar48();
                if(word48.Length!=48)throw new InvalidDataException("Complete actual WORD48 row required.");
                rows++;Record("word-row");RequireNoWordControl(BinaryPrimitives.ReadUInt32LittleEndian(word48.AsSpan(8)));Observe(LegacyGlobalControlCleanupStage.WordRow);
                wordFlags&=0xffefffff;wordCleared=true;Record("word-cleared");Observe(LegacyGlobalControlCleanupStage.WordCleared);
                var byId=State.Characters.ToDictionary(c=>c.OriginalSource.Node.Identity);
                var insertion=State.Publication.Wrapper.CharacterInterner.CopyInsertionOrder();
                if(insertion.Count!=byId.Count)throw new InvalidOperationException("Complete actual CHAR insertion list required.");
                foreach(var node in insertion)
                {
                    token.ThrowIfCancellationRequested();
                    if(!byId.TryGetValue(node.Identity,out var character)||!ReferenceEquals(node,character.OriginalSource.Node))throw new InvalidOperationException("Actual CHAR owner changed.");
                    characters.Add(node.Identity);Record("character");RequireNoCharacterControl(character.Flags);Observe(LegacyGlobalControlCleanupStage.Character);
                }
                SourceIndices.ValidateCurrent(token);Observe(LegacyGlobalControlCleanupStage.Complete);SourceIndices.ValidateCurrent(token);complete=true;Record("complete");return this;
            }
            catch{faulted=true;Record("fault");throw;}
        }
        void Observe(LegacyGlobalControlCleanupStage stage){observer?.Invoke(stage);token.ThrowIfCancellationRequested();}
    }
    internal static void RequireNoWordControl(uint flags)
    {if((flags&0x100000)!=0)throw new NotSupportedException("Actual WORD row requires022960 expansion; no unavailable service is called.");}
    internal static void RequireNoCharacterControl(uint flags)
    {if((flags&0x100000)!=0)throw new NotSupportedException("Active CHAR control scanning, conditional deletion and034D90 updates are outside this bounded owner; no partial deletion is fabricated.");}
    public void ValidateCurrent(CancellationToken token=default)
    {lock(gate){token.ThrowIfCancellationRequested();if(!complete||faulted)throw new InvalidOperationException("Both genuine023260 passes must complete.");SourceIndices.ValidateCurrent(token);}}
    public int[] CopyCharacterVisitOrder(){lock(gate)return characters.ToArray();}
    public LegacyGlobalControlCleanupPrefix[] CopyPrefixes(){lock(gate)return prefixes.ToArray();}
    private void Record(string stage)=>prefixes.Add(new(stage,rows,wordCleared,wordFlags,characters.Count));
}
