using System.Runtime.CompilerServices;
using System.Buffers.Binary;
[assembly:InternalsVisibleTo("LegacyEnglishMainBoundary.Probe")]
namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseNode1Geometry
{
    // Factory consumes an actual currently active LAST-publication scope. The
    // generic protocol kernel supplies its real request; no symbol-string or
    // detached winning snapshot is accepted as a replacement request issuer.
    internal LegacyMainProtocolGlyphProductionOperations CreateGraphProtocolOperations(
        LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,CancellationToken token)
    {
        RequireGraphProtocolOwner(stage,token);
        if(!Alphabet.TryGetLabel("-",out ushort hyphen))throw new InvalidDataException("Actual source hyphen required.");
        return new([0,stage.Lease.Root.DescriptorFlags],hyphen,
            (node,arc,ct)=>{RequireGraphProtocolOwner(stage,ct);source.RequireOrdinaryGroup(node,arc);},
            (request,cursor,ct)=>
            {
                RequireGraphProtocolOwner(stage,ct);
                var result=stage.PeriodicEnabled?LegacyEnglishCaseGraphDelimiterBoundary.ReadPeriodic(stage,request,cursor,source.Graph.NodeCount,
                    (node,arc)=>source.RequireOrdinaryGroup(node,arc),ct):LegacyEnglishCaseGraphDelimiterBoundary.Read(request,cursor,source.Graph.NodeCount,
                    (node,arc)=>source.RequireOrdinaryGroup(node,arc),ct);
                RequireGraphProtocolOwner(stage,ct);return result;
            });
    }
    private void RequireGraphProtocolOwner(LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,CancellationToken token)
    {
        stage.Owner.RequireGraphPublication(stage,token);source.Validate(token);
        if(!ReferenceEquals(Profile.Knowledge,stage.Lease.Root.Knowledge))
            throw new InvalidOperationException("Same live English ordinary graph/source required.");
    }
}

// Pure literal1AAE70 boundary branch, independently prepared below. This
// numerical helper alone cannot grant a graph/WORD producer authority.
internal static class LegacyEnglishCaseGraphDelimiterBoundary
{
    internal static LegacyMainProtocolDelimiterOutput Read(ReadOnlySpan<byte> request64,int cursor,int nodeCount,
        Action<int,ushort> validateOrdinaryArc,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(validateOrdinaryArc);
        if(request64.Length!=64)throw new ArgumentException("Complete actual request64 required.");
        if(cursor is<0 or>256)throw new ArgumentOutOfRangeException(nameof(cursor));
        if(nodeCount is<1 or>49)throw new ArgumentOutOfRangeException(nameof(nodeCount));
        int previous=BinaryPrimitives.ReadInt32LittleEndian(request64[8..]);
        int current=BinaryPrimitives.ReadInt32LittleEndian(request64[24..]);
        if(previous!=-1)
        {
            if((uint)previous>=(uint)nodeCount)throw new ArgumentOutOfRangeException(nameof(previous));
            validateOrdinaryArc(previous,BinaryPrimitives.ReadUInt16LittleEndian(request64[12..]));
        }
        if(current!=nodeCount)
        {
            if((uint)current>=(uint)nodeCount)throw new ArgumentOutOfRangeException(nameof(current));
            validateOrdinaryArc(current,BinaryPrimitives.ReadUInt16LittleEndian(request64[28..]));
        }
        // Native ordinary metadata=-1 and fresh fallback count208=0. FlagsC/D
        // are initialized before this return. Neither kind40 nor packet44/48
        // is consumed here; opaque caller residue is not promoted to known.
        if(cursor==0||previous==-1||current==nodeCount)
        {token.ThrowIfCancellationRequested();return new(cursor,0,[]);}
        throw new NotSupportedException("Interior English source delimiter geometry remains unclosed.");
    }
    internal static LegacyMainProtocolDelimiterOutput ReadPeriodic(LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,ReadOnlySpan<byte> request64,int cursor,int nodeCount,
        Action<int,ushort> validateOrdinaryArc,CancellationToken token)
    {
        stage.Owner.RequireGraphPublication(stage,token);
        if(!stage.PeriodicEnabled||nodeCount!=stage.Ring.ActualNodeCount)throw new InvalidOperationException("Same actual periodic LAST source graph required.");
        token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(validateOrdinaryArc);
        if(request64.Length!=64)throw new ArgumentException("Complete actual request64 required.");
        if(cursor is<0 or>256)throw new ArgumentOutOfRangeException(nameof(cursor));
        if(nodeCount is<1 or>120)throw new ArgumentOutOfRangeException(nameof(nodeCount));
        int previous=BinaryPrimitives.ReadInt32LittleEndian(request64[8..]);
        int current=BinaryPrimitives.ReadInt32LittleEndian(request64[24..]);
        if(previous!=-1)
        {
            if((uint)previous>=(uint)nodeCount)throw new ArgumentOutOfRangeException(nameof(previous));
            validateOrdinaryArc(previous,BinaryPrimitives.ReadUInt16LittleEndian(request64[12..]));
        }
        if(current!=nodeCount)
        {
            if((uint)current>=(uint)nodeCount)throw new ArgumentOutOfRangeException(nameof(current));
            validateOrdinaryArc(current,BinaryPrimitives.ReadUInt16LittleEndian(request64[28..]));
        }
        // Native ordinary metadata=-1 and fresh fallback count208=0. FlagsC/D
        // are initialized before this return. Neither kind40 nor packet44/48
        // is consumed here; opaque caller residue is not promoted to known.
        if(cursor==0||previous==-1||current==nodeCount)
        {token.ThrowIfCancellationRequested();return new(cursor,0,[]);}
        throw new NotSupportedException("Interior English source delimiter geometry remains unclosed.");
    }
}
