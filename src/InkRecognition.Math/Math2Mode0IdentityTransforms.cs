namespace InkRecognition.Math;
// Data adapter leaves both the old fresh owner and new receipt owner distinct.
internal abstract class Math2Mode0IdentityTransforms
{
    internal abstract int MatricesResolved {get;}
    internal abstract int NativeNonidentityTransformCount {get;}
    internal abstract byte[] CopyResolvedMatrices();
    internal static Math2Mode0IdentityTransforms Fresh(LegacyOwnedRawInkIdentityExport actual)=>new FreshData(actual);
    internal static Math2Mode0IdentityTransforms Current(LegacyOwnedCurrentRawIdentityExport actual)=>new CurrentData(actual);
    private sealed class FreshData(LegacyOwnedRawInkIdentityExport actual):Math2Mode0IdentityTransforms
    {internal override int MatricesResolved=>actual.MatricesResolved;internal override int NativeNonidentityTransformCount=>actual.NativeNonidentityTransformCount;internal override byte[] CopyResolvedMatrices()=>actual.CopyResolvedMatrices();}
    private sealed class CurrentData(LegacyOwnedCurrentRawIdentityExport actual):Math2Mode0IdentityTransforms
    {internal override int MatricesResolved=>actual.MatricesResolved;internal override int NativeNonidentityTransformCount=>actual.NativeNonidentityTransformCount;internal override byte[] CopyResolvedMatrices()=>actual.CopyResolvedMatrices();}
}
