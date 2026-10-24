namespace InkRecognition;

public sealed partial class LegacyFreshMainGlobalWordAugmentation
{
    internal void RequireResidualScope(LegacyFreshMainGlobalSourceBatchCompletion.SpanScope actual,
        LegacyFreshMainGlobalResidualSource overlay,CancellationToken token)
    {
        if(!ReferenceEquals(actual,batchScope))throw new ArgumentException("Exact private augmentation span scope required.");
        actual.RequireSource(this,token);
        if(!ReferenceEquals(overlay,session.Associations.SourceOverlay)||!ReferenceEquals(overlay.Geometry,session.Ownership.Geometry)||
            !ReferenceEquals(overlay.Geometry,callbacks.Geometry)||!ReferenceEquals(overlay.Geometry.Policy,policy))
            throw new ArgumentException("Same actual completed session/residual source/ordinary producer required.");
    }
    private byte[] AssociateBatchResiduals(byte[] rows,ulong[] residual,CancellationToken token)
    {
        Validate(token,true);
        if(batchScope is null)throw new NotSupportedException("Actual residual217DB0 augmentation association remains separate.");
        var overlay=session.Associations.SourceOverlay;RequireResidualScope(batchScope,overlay,token);
        byte[] associated=overlay.AssociateAugmentation(this,batchScope,rows,residual,token);
        Validate(token,true);
        if(associated.Length!=rows.Length)throw new InvalidDataException("Residual association changed actual emitted row count.");
        return associated;
    }
    private LegacyNativeResidualSource ResolveAugmentationBoundary(int node,ushort arc,CancellationToken token)
    {
        //Base and standalone callbacks retain their original direct path.
        if(batchScope is null||arc<policy.Owner.Graph.State.GetNode(node).Count)return new(node,arc);
        var overlay=session.Associations.SourceOverlay;RequireResidualScope(batchScope,overlay,token);
        return LegacyNativeResidualSource.Decode(overlay.ResolveBaseReference(((uint)node&0xffffffUL)|((ulong)arc<<24),token));
    }
}
