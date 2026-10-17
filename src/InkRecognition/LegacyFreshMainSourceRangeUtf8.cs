using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("LegacyMainSourceRangeUtf8.Probe")]
[assembly: InternalsVisibleTo("LegacyMainDetachedTextDom.Probe")]

namespace InkRecognition;

/// <summary>One native C21.V180 range string and its separate Text caller
/// first-bracket rewrite. The original source key and normalized key are
/// retained independently; no selection-registry ID replaces an input ID.</summary>
public sealed class LegacyOwnedMainSourceRangeUtf8
{
    private readonly LegacyNativeInkPositionRange[] original,normalized;
    private readonly byte[] utf8,callerUtf8;
    public LegacyFreshMainSourceRangeUtf8 Owner {get;}
    public string Kind {get;}
    public int SourceNodeIdentity {get;}
    public uint SourceFlags {get;}
    public LegacyOwnedMainTextSourcePayload? CharacterSource {get;}
    public LegacyOwnedMainWordSourcePayload? WordSource {get;}
    public LegacyFreshMainDocumentTextSelection? TextSource {get;}
    internal LegacyOwnedMainSourceRangeUtf8(LegacyFreshMainSourceRangeUtf8 owner,string kind,int identity,uint flags,
        LegacyNativeInkPositionRange[] key,LegacyPreparedRangeUtf8 formatted,LegacyOwnedMainTextSourcePayload? character=null,
        LegacyOwnedMainWordSourcePayload? word=null,LegacyFreshMainDocumentTextSelection? text=null)
    {
        Owner=owner;Kind=kind;SourceNodeIdentity=identity;SourceFlags=flags;original=key.ToArray();
        normalized=formatted.Ranges.ToArray();utf8=formatted.Bytes.ToArray();
        callerUtf8=LegacyNativeSourceRangeUtf8.CallerFirstBrace(utf8,flags);
        CharacterSource=character;WordSource=word;TextSource=text;
    }
    public LegacyNativeInkPositionRange[] CopyOriginalRanges()=>original.ToArray();
    public LegacyNativeInkPositionRange[] CopyNormalizedRanges()=>normalized.ToArray();
    public byte[] CopyUtf8()=>utf8.ToArray();
    public byte[] CopyCallerUtf8()=>callerUtf8.ToArray();
}

/// <summary>Actual retained CHAR insertion list, WORD and TEXT source keys
/// after pending counter handoff. Formatting does not construct DOM nodes,
/// execute pending commands or accept a document.</summary>
public sealed class LegacyFreshMainSourceRangeUtf8
{
    public LegacyFreshMainDocumentCounterHandoff CounterHandoff {get;}
    public LegacyFreshMainTextDocumentSeed Seed=>CounterHandoff.Transforms.Seed;
    public LegacyOwnedRawInkDocument Document=>Seed.Document;
    public IReadOnlyList<LegacyOwnedMainSourceRangeUtf8> Sources {get;private set;}=Array.Empty<LegacyOwnedMainSourceRangeUtf8>();
    private LegacyFreshMainSourceRangeUtf8(LegacyFreshMainDocumentCounterHandoff handoff){CounterHandoff=handoff;}
    public static LegacyFreshMainSourceRangeUtf8 Prepare(LegacyFreshMainDocumentCounterHandoff actualHandoff,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualHandoff);
        LegacyFreshMainDocumentCounterHandoff.Validate(actualHandoff.Transforms,token);
        var owner=new LegacyFreshMainSourceRangeUtf8(actualHandoff);var state=actualHandoff.Transforms.Cleanup.State;
        if(!ReferenceEquals(state.Text.Document,owner.Document)||!ReferenceEquals(state.Text.Selection.Owner,actualHandoff.TransactionOwner))
            throw new ArgumentException("Same retained document and selection transaction required.");
        var operations=LegacyOwnedRawInkRangeOperations.Prepare(owner.Document);
        var output=new List<LegacyOwnedMainSourceRangeUtf8>();var byId=state.Characters.ToDictionary(c=>c.OriginalSource.SourceNode.Identity);
        foreach(var node in state.Characters[0].OriginalSource.SourceNode.Owner.CopyInsertionOrder())
        {
            token.ThrowIfCancellationRequested();var character=byId[node.Identity];var source=character.OriginalSource;
            if(!ReferenceEquals(node.Owner.Document,owner.Document))throw new ArgumentException("Foreign CHAR source document.");
            Add("CHAR",node.Identity,character.Flags,source.SourceNode.FirstProcessedSource!.CopyRanges(),source);
        }
        Add("WORD",state.OriginalWordSource.SourceNode.Identity,actualHandoff.Transforms.Cleanup.WordFlags,
            state.OriginalWordSource.CopySourceKeyRanges(),word:state.OriginalWordSource);
        //031880's TEXT source formatting has no CHAR/WORD bit400000 rewrite.
        Add("TEXT",0,0,state.Text.CopyRanges(),text:state.Text);
        LegacyFreshMainDocumentCounterHandoff.Validate(actualHandoff.Transforms,token);
        owner.Sources=Array.AsReadOnly(output.ToArray());return owner;

        void Add(string kind,int identity,uint flags,LegacyNativeInkPositionRange[] ranges,
            LegacyOwnedMainTextSourcePayload? character=null,LegacyOwnedMainWordSourcePayload? word=null,
            LegacyFreshMainDocumentTextSelection? text=null)
        {
            foreach(var range in ranges){ValidatePosition(range.First);ValidatePosition(range.Last);}
            var formatted=LegacyNativeSourceRangeUtf8.Format(ranges,operations,token:token);
            if(!formatted.Succeeded)throw new InvalidOperationException("Unexpected portable formatting allocation failure.");
            output.Add(new(owner,kind,identity,flags,ranges,formatted,character,word,text));
        }
        void ValidatePosition(LegacyNativeInkPosition position)
        {
            if(position.SourceIndex>=(uint)owner.Document.ObjectCount||position.PointIndex>=owner.Document.Strokes[(int)position.SourceIndex].Points.Count)
                throw new NotSupportedException("Actual retained original raw-object position required; registry IDs are not source coordinates.");
            LegacyNativeSourceRangeUtf8.ValidatePosition(position);
        }
    }
}

internal sealed record LegacyPreparedRangeUtf8(bool Succeeded,byte[] Bytes,LegacyNativeInkPositionRange[] Ranges,
    bool WasCanonical,int AllocationAttempts,int Capacity);

/// <summary>Bounded83DF0/080430/07F690 with literal07FAF0 normalization.
/// Fault injection is internal test scaffolding, not a public caller flag.</summary>
internal static class LegacyNativeSourceRangeUtf8
{
    internal static void ValidatePosition(LegacyNativeInkPosition p)
    {
        if(p.SourceIndex>int.MaxValue||p.Fraction is >=200 and <255)
            throw new NotSupportedException("07F690 requires nonnegative signed source IDs and fractions0..199 orFF.");
    }
    internal static bool IsCanonical(ReadOnlySpan<LegacyNativeInkPositionRange> rows)
    {
        for(int i=1;i<rows.Length;i++)
        {
            ulong last=rows[i-1].Last.Value,first=rows[i].First.Value;
            if((byte)last==4)break;
            if(first<=last||unchecked((uint)(last>>32)+1)==(uint)(first>>32)&&
                (unchecked((uint)(short)last+0xfdu)&0xfffdu)==0&&(uint)first is 3 or 0xff03)return false;
        }
        return true;
    }
    internal static LegacyPreparedRangeUtf8 Format(ReadOnlySpan<LegacyNativeInkPositionRange> input,
        LegacyOwnedRawInkRangeOperations operations,int failAllocation=-1,bool failFinalize=false,CancellationToken token=default)
        =>FormatCore(input,operations,0,failAllocation,failFinalize,null,token);
    internal static LegacyPreparedRangeUtf8 FormatIntoScratch(ReadOnlySpan<LegacyNativeInkPositionRange> input,
        LegacyOwnedRawInkRangeOperations operations,int previousCapacity,Func<string,bool> allowAllocation,CancellationToken token=default)
        =>FormatCore(input,operations,previousCapacity,-1,false,allowAllocation,token);
    private static LegacyPreparedRangeUtf8 FormatCore(ReadOnlySpan<LegacyNativeInkPositionRange> input,
        LegacyOwnedRawInkRangeOperations operations,int previousCapacity,int failAllocation,bool failFinalize,
        Func<string,bool>? allowAllocation,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(operations);token.ThrowIfCancellationRequested();
        if(input.Length>65536)throw new NotSupportedException("Portable source-range budget exceeded.");
        foreach(var range in input){ValidatePosition(range.First);ValidatePosition(range.Last);}
        if(previousCapacity<0)throw new ArgumentOutOfRangeException(nameof(previousCapacity));
        bool canonical=IsCanonical(input);var rows=input.ToArray();int allocations=0,capacity=previousCapacity;var bytes=new List<byte>();
        bool Allocate(int required,bool copy=false)
        {
            if(allocations++==failAllocation||allowAllocation is not null&&!allowAllocation(copy?"range-copy":"range-bytes"))return false;
            if(!copy)capacity=Math.Max(required,(int)(capacity*1.5));return true;
        }
        LegacyPreparedRangeUtf8 Result(bool succeeded)=>new(succeeded,bytes.ToArray(),rows,canonical,allocations,capacity);
        if(!canonical)
        {
            //07F830 copies the input before any string reset/reserve. Its
            //allocation is distinct from later string growth allocations.
            if(!Allocate(rows.Length,true))return Result(false);
            var normalized=new[]{rows[0]};
            for(int i=1;i<rows.Length;i++)
            {token.ThrowIfCancellationRequested();if(rows[i].First.Mode!=4&&normalized[0].First.Mode!=4)normalized=operations.Union(normalized,[rows[i]],token);}
            rows=normalized;
        }
        int initial=checked(rows.Length*18+1);if(capacity<initial&&!Allocate(initial))return Result(false);
        bool Reserve(int length)=>capacity>=length+1||Allocate(length+1);
        for(int i=0;i<rows.Length;i++)
        {
            token.ThrowIfCancellationRequested();
            if(i!=0){if(!Reserve(bytes.Count+1))return Result(false);bytes.Add((byte)'U');}
            var range=rows[i];string part=(range.First.Mode==3?"[":"]")+Endpoint(range.First)+","+Endpoint(range.Last)+(range.Last.Mode==5?"]":"[");
            var encoded=Encoding.ASCII.GetBytes(part);if(!Reserve(checked(bytes.Count+encoded.Length)))return Result(false);
            bytes.AddRange(encoded);
        }
        return Result(!failFinalize);
    }
    private static string Endpoint(LegacyNativeInkPosition p)
    {
        string suffix=p.Fraction==255?"$":p.Fraction==0?"":"."+(p.Fraction*5).ToString("D3",CultureInfo.InvariantCulture).TrimEnd('0');
        return p.SourceIndex.ToString(CultureInfo.InvariantCulture)+":"+p.PointIndex.ToString(CultureInfo.InvariantCulture)+suffix;
    }
    internal static byte[] CallerFirstBrace(ReadOnlySpan<byte> input,uint sourceFlags)
    {
        byte[] output=input.ToArray();
        if((sourceFlags&0x400000)!=0)
        {
            if(output.Length==0)throw new NotSupportedException("Native caller first-byte write on an empty string has no supported owned byte.");
            output[0]=output[0]=='['?(byte)'{':(byte)'}';
        }
        return output;
    }
}
