using System.Security.Cryptography;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyFst28CNumberTrueLanguage.Probe")]
namespace InkRecognition;

internal sealed partial class LegacyFst28CNumberTrueLanguageSourceDraft
{
    private static ReadOnlySpan<byte> AcousticDigest=>
    [0x7a,0x5f,0x08,0x80,0x9e,0x81,0xfb,0x4a,0x2d,0x4c,0x78,0xb3,0xef,0xd2,0x51,0xd0,
     0x98,0x65,0x03,0xb0,0xd3,0x0d,0x21,0xde,0x36,0x95,0xda,0xb1,0xf8,0xa8,0x99,0x78];
    private readonly record struct GraphAdmission(bool TrueBoundary,int RootRows,int FollowingRows,int RootOrdinals);
    private static GraphAdmission PreflightOriginalGraph(LegacyFst28CNumberInitialD0 d0,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(d0);
        if(d0.CurrentNode!=0||d0.CurrentSpan!=0||d0.ReusedPreviousCache||d0.AcousticEvaluations is <1 or >64||
            !ReferenceEquals(d0.Graph,d0.Bootstrap.Graph)||!ReferenceEquals(d0.Source,d0.Policy.PrimaryMap.Source)||
            !ReferenceEquals(d0.Policy,d0.Bootstrap.InitialState.TargetPreparation)||
            !d0.Source.HasActualFreshMain135Authority||d0.Source.ActualAcousticSourceKind!=0x135||
            !StringComparer.OrdinalIgnoreCase.Equals(d0.Source.ActualAcousticFingerprint,
                "7A5F08809E81FB4A2D4C78B3EFD251D0986503B0D30D21DE3695DAB1F8A89978")||
            d0.InitialState.DecodedSourceName!="ru_RU-lk-text.number"||
            d0.Bootstrap.ReadSourceMetadata(token)!=new LegacyFst28CNumberSourceMetadata(1,0,1,0,1,uint.MaxValue,1))
            throw new ArgumentException("Exact actual direct-number initial D0/source/bootstrap is required.");
        var graph=d0.Graph.State;
        if(graph.MaximumAtoms!=8||graph.NodeCount is <2 or >65535||graph.Root.SelectorCount!=9)
            throw new NotSupportedException("Bounded actual ordinary cap8 graph required before any prefix claim.");
        // Each ReadOnlyCollection getter is hoisted ONCE per row. Inspect count
        // before enumerating. No hidden unbounded first/following scan remains.
        var first=graph.GetNode(0);
        if(first.Count is <1 or >256)throw new NotSupportedException("Root row bound256 required before growth.");
        var ends=graph.Root.CopyCumulativeEnds(); // EXACT9*2 via private immutable constructor.
        if(ends.Length!=9||ends[1]<ends[0]||ends[1]-ends[0]>256)
            throw new NotSupportedException("Actual root selector1 count bound256 required before growth.");
        bool boundary=false;int following=0;
        for(int n=1;n<3&&n<graph.NodeCount;n++)
        {
            token.ThrowIfCancellationRequested();var row=graph.GetNode(n);
            if(row.Count>256)throw new NotSupportedException("Each following row must fit256 before enumeration.");
            following=checked(following+row.Count);
            foreach(var arc in row)
            {token.ThrowIfCancellationRequested();if(n-arc.Span-1==0&&(arc.Flags&4)!=0)boundary=true;}
        }
        return new(boundary,first.Count,following,ends[1]-ends[0]);
    }
    private static void RequireFactoryAdmission(WholeBudgetProposal proposed,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(proposed.AllocationPayload<=0||proposed.PrivateStrongPeak<=0||proposed.Work<=0)
            throw new InvalidDataException("Complete positive SOURCE proposal required.");
        // The concrete source path exists below. It MUST NOT execute until
        // whole B/W/retained-lifetime/helper/oracle proof is reviewed.
        throw new NotSupportedException("Whole TRUE factory admission remains closed pending complete source budget/oracle review.");
    }
    internal static LegacyFst28CNumberTrueLanguageSourceDraft CreateFreshForOwnedProbe(
        LegacyFst28CNumberInitialD0 d0,ReadOnlySpan<byte> sameAcousticResource,
        CancellationToken token=default,Action<string>? checkpoint=null)
        =>CreateFreshBoundedSource(d0,sameAcousticResource,token,checkpoint);
    private static LegacyFst28CNumberTrueLanguageSourceDraft CreateFreshBoundedSource(
        LegacyFst28CNumberInitialD0 d0,ReadOnlySpan<byte> sameAcousticResource,
        CancellationToken token,Action<string>? checkpoint)
    {
        token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(d0);
        if(sameAcousticResource.Length!=572763)throw new ArgumentException("Whole actual MAIN135 resource length required.");
        var graph=PreflightOriginalGraph(d0,token);
        if(!graph.TrueBoundary)throw new NotSupportedException("Actual graph selects skipped-language branch; no TRUE prefix claimed.");
        RequireFactoryAdmission(DeriveCombinedProposal(),token); // Unconditional; no allocation/model/prefix construction follows today.
        void Step(string name){token.ThrowIfCancellationRequested();checkpoint?.Invoke(name);token.ThrowIfCancellationRequested();}
        var receiver=new LegacyFst28CNumberTrueLanguageSourceDraft(d0);
        receiver.RequireWholeAdmission(token); // Independently closed same-owner gate.
        Step("before-factory-snapshot");var snapshot=new byte[572763];
        try
        {
            for(int at=0;at<snapshot.Length;at+=65536)
            {token.ThrowIfCancellationRequested();sameAcousticResource.Slice(at,Math.Min(65536,snapshot.Length-at)).CopyTo(snapshot.AsSpan(at));}
            Span<byte> digest=stackalloc byte[32];SHA256.HashData(snapshot,digest);
            if(!CryptographicOperations.FixedTimeEquals(digest,AcousticDigest))
                throw new ArgumentException("Authenticated private same MAIN135 snapshot required before prefix claim.");
            Step("after-factory-authentication");
            // SAME genuine retained context producer, old prefix/header claim,
            // actual oldEngine one-way close and private source issuance.
            var context=LegacyFst28CNumberInitialConsumerContext.CreateRetainedForTrueLanguage(receiver,token);
            Step("after-retained-context");
            var consumer=LegacyFst28CNumberInitialConsumer.CreateActualDirectNumberInitialConsumer(context,snapshot,token);
            receiver.BindCompletedConsumer(consumer,token);
            Step("after-original-consumer");receiver.ExecuteConstructedTrueConsumer(token,checkpoint);
            if(Volatile.Read(ref receiver.stage)!=5||receiver.completedNumeric is null)
                throw new InvalidDataException("Private actual TRUE node0 body did not complete.");
            return receiver; // Internal completed storage only, never query/accepted-text authority.
        }
        catch
        {
            // Only this unpublished receiving construction is poisoned. Old
            // producer/header/consumer claims are never cleared or fabricated.
            Volatile.Write(ref receiver.stage,6);
            receiver.pool?.Poison();receiver.source?.PoisonAfterClaim(receiver);
            throw;
        }
        finally{CryptographicOperations.ZeroMemory(snapshot);}
    }
    internal LegacyFst28CNumberTrueSourceDiagnostic CopyOwnedDiagnostic(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(Volatile.Read(ref stage)!=5||completedNumeric is null||consumer is null||pool is null||page is null||source is null||
            !consumer.ReadSkippedLanguageCompletionClaim()||!ReferenceEquals(consumer.Context,context)||!ReferenceEquals(consumer.D0,d0))
            throw new InvalidOperationException("No private completed TRUE storage diagnostic.");
        // Scalar only; no live source/Page/Binding/heap/context identities escape.
        return completedNumeric.ReadClosedDiagnostic(this,token);
    }
    private sealed partial class NumericContinuation
    {
        internal LegacyFst28CNumberTrueSourceDiagnostic ReadClosedDiagnostic(
            LegacyFst28CNumberTrueLanguageSourceDraft expected,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(!ReferenceEquals(receiver,expected)||!ReferenceEquals(receiver.completedNumeric,this)||
                Volatile.Read(ref receiver.stage)!=5||Volatile.Read(ref finalization)!=2)
                throw new InvalidDataException("Exact closed private numeric diagnostic required.");
            return new(sourceEnd,retainedEnd,count,cleanup,cumulative2520,backward2528,157,125,false,false,false);
        }
    }
}
internal readonly record struct LegacyFst28CNumberTrueSourceDiagnostic(int SourceEnd,int RetainedEnd,
    int LanguageHeapCount,int Cleanup,int Cumulative2520,int Backward2528,int PhysicalStoredRows,
    int PrimaryRows,bool HasQueryAuthority,bool HasParentHistoryAuthority,bool HasAcceptedText);
