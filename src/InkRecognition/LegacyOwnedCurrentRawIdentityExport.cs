using System.Buffers.Binary;
namespace InkRecognition;

// Immutable identity-data DTO. The owning ingress derives it inside an active
// stable-C20 receipt; this constructor/data object is not an authority token.
// A data snapshot alone confers no current-input/NN authority.
internal sealed class LegacyOwnedCurrentRawIdentityExport
{
    private readonly LegacyNativeInkPositionRange[] ranges;
    private readonly LegacyNativeSelectedInkRange[] fragments;
    private readonly int[] matches;
    internal LegacyOwnedRawInkDocument Document {get;}
    internal int MatricesResolved=>fragments.Length;
    internal int NativeNonidentityTransformCount=>0;
    internal LegacyOwnedCurrentRawIdentityExport(LegacyOwnedRawInkDocument document,
        LegacyNativeInkPositionRange[] ranges,LegacyNativeSelectedInkRange[] fragments,int[] matches)
    {Document=document;this.ranges=ranges;this.fragments=fragments;this.matches=matches;}
    internal LegacyNativeInkPositionRange[] CopyRanges()=>ranges.ToArray();
    internal LegacyNativeSelectedInkRange[] CopyFragments()=>fragments.ToArray();
    internal int[] CopyMatchedViewIndices()=>matches.ToArray();
    internal byte[] CopyResolvedMatrices()
    {
        var bytes=new byte[checked(fragments.Length*48)];
        for(int i=0;i<fragments.Length;i++)
        {BinaryPrimitives.WriteDoubleLittleEndian(bytes.AsSpan(i*48),1);BinaryPrimitives.WriteDoubleLittleEndian(bytes.AsSpan(i*48+32),1);}
        return bytes;
    }
}
internal sealed partial class LegacyOwnedRawInkAppendState
{
    internal sealed partial class Borrow
    {
        internal LegacyOwnedCurrentRawIdentityExport ExportIdentityTransforms(
            ReadOnlySpan<LegacyNativeInkPositionRange> ranges,CancellationToken cancellation=default)
        {
            Check();cancellation.ThrowIfCancellationRequested();var frame=owner.current;var document=frame.Projection;
            var domain=new LegacyNativeInkPositionRange(
                LegacyNativeInkPosition.Pack(0,0,3,frame.Curves[0].PointCount),
                LegacyNativeInkPosition.Pack((uint)(frame.Curves.Length-1),frame.Curves[^1].PointCount-1,5,frame.Curves[^1].PointCount));
            if(ranges.Length!=1||ranges[0]!=domain)
                throw new NotSupportedException("Current ingress admits the actual whole ordinary INPUT only.");
            var objects=new LegacyNativeSourceInkObject[frame.Curves.Length];
            var descriptors=new LegacyNativeSelectedInkRange[frame.Views.Length];
            for(int i=0;i<objects.Length;i++)
            {
                cancellation.ThrowIfCancellationRequested();Check();
                var curve=frame.Curves[i];var view=frame.Views[i];var descriptor=view.Descriptor;
                if(curve.NativeConcreteType!=0xc49||curve.OriginalSourceIndex!=i||!ReferenceEquals(view.Curve,curve)||
                    view.CurrentViewIndex!=i||descriptor.SourceIndex!=(uint)i||descriptor.PointCount!=curve.PointCount||
                    descriptor.FirstPoint!=0||descriptor.LastPoint!=curve.PointCount-1||descriptor.FirstFraction!=0||descriptor.LastFraction!=0||
                    !descriptor.FirstClosed||!descriptor.LastClosed||descriptor.IsKnownText||document.Strokes[i].Points.Count!=curve.PointCount)
                    throw new InvalidOperationException("Private current C49/view/projection lineage lost.");
                objects[i]=new(frame.Curves[i].PointCount);descriptors[i]=frame.Views[i].Descriptor;
            }
            var selected=LegacyNativeSelectedInkProducer.Select(ranges,objects,cancellation);var matches=new int[selected.Length];
            for(int i=0;i<selected.Length;i++)
            {cancellation.ThrowIfCancellationRequested();Check();matches[i]=LegacyRawInkCurrentViewMatching.RequireCover(descriptors,selected[i]);}
            Check();cancellation.ThrowIfCancellationRequested();return new(document,ranges.ToArray(),selected,matches);
        }
    }
}
