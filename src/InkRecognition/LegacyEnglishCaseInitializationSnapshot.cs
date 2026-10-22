using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglishCaseInitialization.Probe")]
namespace InkRecognition;

/// <summary>Immutable initialization facts only. Portable IDs are not pointers
///or completed query/terminal/history authority.</summary>
public readonly record struct LegacyEnglishCaseInitializationControl(ushort Symbol,ulong Identity,bool Created);
public sealed class LegacyEnglishCaseInitializationSnapshot
{
    private readonly LegacySymbolContextNode[] initial,post;
    private readonly int[] initialPages,postPages;
    private readonly ushort[] primary,auxiliary;
    private readonly ulong[] source68;
    private readonly byte[] ordinary,alternate;
    private readonly LegacyEnglishCaseInitializationControl[] controls;
    public int SourceKind=>0x135;
    public int CoordinateMode=>2;
    public ushort CasePathVersion=>0x200;
    public uint DescriptorFlags=>0xc0e;
    public int SelectedPriority=>1024;
    public int InitialContextCapacity=>104;
    public int InitialPageCapacity{get;}
    public int InitialPageCursor{get;}
    public int PostControlPageCapacity{get;}
    public int PostControlPageCursor{get;}
    public ulong ContainerIdentity{get;}
    public ulong InitialRootIdentity{get;}
    public ulong PostControlRootIdentity{get;}
    internal LegacyEnglishCaseInitializationSnapshot(ulong container,ulong initialRoot,ulong postRoot,
        LegacySymbolContextNode[] before,LegacySymbolContextNode[] after,int[] beforePages,int[] afterPages,
        ushort[] preserved,ushort[] auxiliaryMap,ulong[] contexts,byte[] classifications,byte[] alternateClassifications,
        LegacyEnglishCaseInitializationControl[] resolved,int initialCapacity,int initialCursor,int postCapacity,int postCursor)
    {ContainerIdentity=container;InitialRootIdentity=initialRoot;PostControlRootIdentity=postRoot;initial=before;post=after;initialPages=beforePages;postPages=afterPages;primary=preserved;auxiliary=auxiliaryMap;source68=contexts;ordinary=classifications;alternate=alternateClassifications;controls=resolved;InitialPageCapacity=initialCapacity;InitialPageCursor=initialCursor;PostControlPageCapacity=postCapacity;PostControlPageCursor=postCursor;}
    internal LegacyEnglishCaseInitializationSnapshot Copy()=>new(ContainerIdentity,InitialRootIdentity,PostControlRootIdentity,
        initial.ToArray(),post.ToArray(),initialPages.ToArray(),postPages.ToArray(),primary.ToArray(),auxiliary.ToArray(),source68.ToArray(),ordinary.ToArray(),alternate.ToArray(),controls.ToArray(),InitialPageCapacity,InitialPageCursor,PostControlPageCapacity,PostControlPageCursor);
    public LegacySymbolContextNode[] CopyInitialNodes()=>initial.ToArray();
    public LegacySymbolContextNode[] CopyPostControlNodes()=>post.ToArray();
    public int[] CopyInitialPages()=>initialPages.ToArray();
    public int[] CopyPostControlPages()=>postPages.ToArray();
    public ushort[] CopyPreservedMap()=>primary.ToArray();
    public ushort[] CopyAuxiliaryMap()=>auxiliary.ToArray();
    public ulong[] CopyInitialSource68()=>source68.ToArray();
    public byte[] CopyClassifications(bool alternateTable=false)=>(alternateTable?alternate:ordinary).ToArray();
    public LegacyEnglishCaseInitializationControl[] CopyControls()=>controls.ToArray();
}
