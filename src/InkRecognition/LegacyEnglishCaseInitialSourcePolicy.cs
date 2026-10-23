using System.Buffers.Binary;
namespace InkRecognition;
// Owns the declared direct fresh OUTER2B3/one13B branch, not configured/default policy.
internal sealed class LegacyEnglishCaseInitialSourcePolicy
{
    internal const long CompleteLogicalCeiling=126639369;
    internal LegacyEnglish119MainKnowledge Knowledge{get;}
    internal LegacyEnglishLkFreshNumericBinding Numeric{get;}
    internal LegacyNativeWordHeapPreparation Heaps{get;}
    internal bool SuppressHeaderFlag{get;}
    private LegacyEnglishCaseInitialQueryPass? pass;
    private LegacyEnglishCaseInitialSourcePolicy(LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding numeric,LegacyNativeWordHeapPreparation heaps,bool suppress)
    {Knowledge=knowledge;Numeric=numeric;Heaps=heaps;SuppressHeaderFlag=suppress;}
    internal static LegacyEnglishCaseInitialSourcePolicy Prepare(LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding numeric,long budget,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(knowledge);ArgumentNullException.ThrowIfNull(numeric);token.ThrowIfCancellationRequested();
        if(budget<CompleteLogicalCeiling||budget>128L*1024*1024||!ReferenceEquals(knowledge.Source,numeric.DataOwner.Source)||!knowledge.Owner.IsComplete||knowledge.Owner.IsFaulted)throw new ArgumentException("Complete logical envelope and actual same-source complete owners required before claim.");
        var source=knowledge.Source;
        byte[] Read(string name,int size,int count){var f=source.Fields.Single(x=>x.Owner=="247"&&x.Destination==name);if(f.ElementSize!=size||f.Count!=count)throw new InvalidDataException("Complete source limit metadata required.");token.ThrowIfCancellationRequested();return f.CopyBytes();}
        byte b8=Read("B8",1,1)[0],b9=Read("B9",1,1)[0];int arcs=BinaryPrimitives.ReadUInt16LittleEndian(Read("1AC",2,16).AsSpan(26));
        var descriptor=LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(numeric.DataOwner.Prefix,numeric.DataOwner.Dictionary,(uint)numeric.LanguageOwner.Order,true,token);
        if(descriptor.Flags!=0xc0e||arcs!=125||b8>1||b9>1)throw new NotSupportedException("Actual direct fresh EN one13B limits required.");
        LegacyNativeWordPreparedSource[] sources=[new(0,0,1),LegacyNativeWordPreparedSource.FromDictionary(descriptor)];
        var summary=LegacyNativeWordHeapPreparation.SummarizeFresh(sources,token);
        var refresh=LegacyNativeWordScalarRefresh.Apply(0,summary.PackedSource22e8,false,b8,0,b9,-1,0,source.Alphabet.TryGetLabel("\u202b",out _));
        var fresh=LegacyNativeWordControllerConfiguration.CreateFresh20baa0()with{PackedSource22e8=refresh.PackedSource22e8};
        var limits=LegacyNativeWordControllerLimitPreparation.Prepare(fresh,(uint)arcs,LegacyNativeWordKnowledgeLimits.CreateFresh00c860(),true);
        var heaps=LegacyNativeWordHeapPreparation.PrepareFresh(sources,limits.Configuration,0,token);
        token.ThrowIfCancellationRequested();return new(knowledge,numeric,heaps,refresh.ConsecutiveBlank22f7!=0);
    }
    internal void Bind(LegacyEnglishCaseInitialQueryPass value)
    {
        if(pass is not null||!ReferenceEquals(value.Root.Knowledge,Knowledge)||!ReferenceEquals(value.Root.NumericOwner,Numeric)||value.Root.DescriptorFlags!=0xc0e)throw new InvalidOperationException("One privately issued exact case root required.");pass=value;
    }
    internal void Require(LegacyEnglishFreshCaseMainSource.InitialQueryLease lease)
    {
        if(pass is null||!ReferenceEquals(lease.Owner,pass)||!ReferenceEquals(lease.Root,pass.Root)||!ReferenceEquals(pass.Initializer.RootForAudit,lease.Root))throw new InvalidOperationException("Same source/initializer/pass lease required.");
        lease.Root.ValidateInitialQuery(lease);
    }
}
