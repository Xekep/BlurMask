namespace InkRecognition;

// SOURCE DRAFT ONLY. Logical cumulative array payload/copy/visit accounting;
// never CLR/RSS, CPU instructions, OLD slot events, or a sufficiency certificate.
internal sealed class LegacyEnglishCaseNode0Ledger
{
    internal const long AggregateCeiling=134169957,MaximumAggregate=128L*1024*1024;
    internal const long ContinuationCeiling=9L*1024*1024,CopyCeiling=64L*1024*1024,VisitCeiling=16L*1024*1024;
    internal enum Site
    {
        Ledger,VectorOwners,Vector2528,Vector23e0,Vector2408,Vector23f0,Vector23f8,Vector2478,Vector2480,Vector2488,Vector2520,
        RowOwners,PrimaryOwners,PhysicalRows,StoredBlocks,PrimaryBlocks,CurrentRowsMask,CurrentPrimaryMask,
        Controller,ControllerMask,PageReferences,PageCopies,LanguageFirst,LanguageSecond,LanguageFirstMask,LanguageSecondMask,
        AuxiliaryOuter,AuxiliaryHelperCopy,AuxiliaryRow,AuxiliaryInjected,AuxiliaryPage,AuxiliaryBuffer,
        Phase1Working,Phase1Sort,Phase1Rebuild,Phase1Quota,FinalImages,OrdinalVectors,Export,Authentication,Graph,Reset,Cleanup,Descriptor,AuxiliaryConsumedList,TerminalIndex,IndexStackFirst,IndexStackEnd,IndexKernel,AuxiliaryMapProof,FreshSelectedField,FirstKernel,TerminalMetadata,FirstLanguage,FirstAcceptedWriter,NumericHandleMetadata,NumericHandlePrepare,Reserved,OwnerMetadata,AuxiliaryMetadata,
    }
    private const int Sites=64;
    // Three fixed arrays, all growth forbidden. Bootstrap includes1536 array
    // bytes plus48 semantic instance-field bytes before array allocation;
    // no global claim table or dictionary exists.
    private readonly long[] allocated,copied,visited;
    // Bootstrap preflight occurs in Create before this object is allocated.
    // Include48 semantic instance-field bytes plus the three fixed arrays;
    // constructor writes reserve48, and initial counter stores reserve16.
    private long payload=3L*Sites*8+48,copies=64,visits;
    internal static LegacyEnglishCaseNode0Ledger Create(long maximum)
    {
        if(maximum<AggregateCeiling||maximum>MaximumAggregate)throw new ArgumentOutOfRangeException(nameof(maximum));
        long bootstrapBytes=3L*Sites*8+48,bootstrapCopies=64;
        if(bootstrapBytes>ContinuationCeiling||bootstrapCopies>CopyCeiling)throw new InvalidDataException("Ledger bootstrap allowance unavailable.");
        return new(maximum);
    }
    private LegacyEnglishCaseNode0Ledger(long maximum)
    {
        if(maximum<AggregateCeiling||maximum>MaximumAggregate)throw new ArgumentOutOfRangeException(nameof(maximum));
        allocated=new long[Sites];copied=new long[Sites];visited=new long[Sites];allocated[(int)Site.Ledger]=payload;copied[(int)Site.Ledger]=64;
    }
    internal long Payload=>payload;
    internal long Copies=>copies;
    internal long Visits=>visits;
    private static int Index(Site site)
    {int i=(int)site;if((uint)i>=Sites)throw new ArgumentOutOfRangeException(nameof(site));return i;}
    // Cumulative precharges never refund on failure. An exhausted budget throws
    // before destination mutation/allocation. Owner catches poison entered work.
    internal void Allocate(Site site,long bytes)
    {
        int i=Index(site);if(bytes<0||bytes>ContinuationCeiling-payload)throw new InvalidDataException("Node0 cumulative payload exhausted before allocation.");
        // Include the two actual8-byte counter-array writes, without recursive
        // metering. Scalar meter arithmetic is instrumentation, not core visits.
        if(16>CopyCeiling-copies)throw new InvalidDataException("Ledger write allowance exhausted.");
        long next=checked(allocated[i]+bytes),nextCopy=checked(copied[i]+16);payload+=bytes;copies+=16;allocated[i]=next;copied[i]=nextCopy;
    }
    internal void Copy(Site site,long bytes)
    {
        int i=Index(site);if(bytes<0||bytes>CopyCeiling-copies-8)throw new InvalidDataException("Node0 copy allowance exhausted before mutation.");
        long next=checked(copied[i]+bytes+8);copies+=bytes+8;copied[i]=next;
    }
    internal void Visit(Site site,long units)
    {
        int i=Index(site);if(units<0||units>VisitCeiling-visits)throw new InvalidDataException("Node0 visit allowance exhausted before source access.");
        if(16>CopyCeiling-copies)throw new InvalidDataException("Ledger write allowance exhausted.");
        long next=checked(visited[i]+units),nextCopy=checked(copied[i]+16);visits+=units;copies+=16;visited[i]=next;copied[i]=nextCopy;
    }
    internal byte[] Bytes(Site site,int length)
    {if(length<0)throw new ArgumentOutOfRangeException(nameof(length));Allocate(site,length);return new byte[length];}
    internal int[] Ints(Site site,int length)
    {if(length<0)throw new ArgumentOutOfRangeException(nameof(length));Allocate(site,checked(4L*length));return new int[length];}
    internal byte[][] References(Site site,int length)
    {if(length<0)throw new ArgumentOutOfRangeException(nameof(length));Allocate(site,checked(8L*length));return new byte[length][];}
    internal void CopyBytes(Site site,ReadOnlySpan<byte> source,Span<byte> destination)
    {if(destination.Length<source.Length)throw new ArgumentException("Complete destination required.");Copy(site,source.Length);source.CopyTo(destination);}
    internal void Fill(Site site,Span<byte> destination,byte value)
    {Copy(site,destination.Length);destination.Fill(value);}
    internal byte[] Snapshot(Site site,ReadOnlySpan<byte> source)
    {var result=Bytes(site,source.Length);CopyBytes(site,source,result);return result;}
    // No array export here: the full receipt's one defensive export must add
    // separately charged allocation+copy sites. Audit access is scalar only.
    internal (long Allocation,long Copy,long Visit) ReadSite(Site site)
    {int i=Index(site);return(allocated[i],copied[i],visited[i]);}
}
