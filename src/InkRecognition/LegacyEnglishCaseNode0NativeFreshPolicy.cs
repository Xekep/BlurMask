using System.Buffers.Binary;
namespace InkRecognition;

// Library-owned NEW fresh initialization, never invented fields on OLD119.
// Native constructors/writers are pinned in frozen31 budget-native.c. This
// models only the declared direct fresh one13B/variant2 branch, not selection
// of arbitrary configured sources, and accepts no scalar snapshot/flags input.
internal sealed class LegacyEnglishCaseNode0NativeFreshPolicy
{
    private readonly LegacyEnglish119MainKnowledge knowledge;
    private readonly LegacyEnglishLkFreshNumericBinding numeric;
    private readonly LegacyEnglishCaseNode0Ledger ledger;
    private readonly int selected28,current50,current54,current58,current68;
    private readonly bool ignoreCaseC4,knowledgeF5;
    private LegacyEnglishFreshCaseMainSource? root;
    private LegacyEnglishCaseQueryInitialization? initializer;
    private LegacyNativeCaseRecordExpansion? expansion;
    private int bound;
    private LegacyEnglishCaseNode0NativeFreshPolicy(LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding numeric,LegacyEnglishCaseNode0Ledger ledger,int selected)
    {
        this.knowledge=knowledge;this.numeric=numeric;this.ledger=ledger;
        //00C860: offset28=-1,50/54=-1,58=-1,68=-1,C4/F5=0.
        selected28=-1;current50=-1;current54=-1;current58=-1;current68=-1;ignoreCaseC4=false;knowledgeF5=false;
        //1D60D0: selected247.214[variant2] -> offset28 (20 decimal).
        selected28=selected;
        //151BF0: first sourceV50 ->54=120,58=selected28.
        current54=120;current58=selected28;
        //20C8C0: fresh3C=-1,mode0,packed3 ->68=120.
        current68=120;
    }
    internal static LegacyEnglishCaseNode0NativeFreshPolicy Prepare(LegacyEnglish119MainKnowledge knowledge,
        LegacyEnglishLkFreshNumericBinding numeric,LegacyEnglishCaseNode0Ledger ledger,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FreshSelectedField,32);var source=knowledge.Source;
        // Additional exact Source247 ASCII64 fingerprint comparison. Same
        // reviewed .NET10.0.12 conditional4096 scope as auxiliary validation;
        // charge before the actual comparison, separate from field iteration.
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FreshSelectedField,4096);
        if(!ReferenceEquals(source,numeric.DataOwner.Source)||source.SourceKind!=0x135||source.CoordinateMode!=2||numeric.Profile!=2||
            !source.Fingerprint.Equals("ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657",StringComparison.OrdinalIgnoreCase)||source.Fields.Count>512)
            throw new NotSupportedException("Declared direct fresh English variant2 source required.");
        var fields=source.Fields;int fieldCount=fields.Count;
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FreshSelectedField,checked(8L*fieldCount+32));
        LegacyResourceWireField? selected=null;
        // The exact private source owns an immutable List.AsReadOnly view.
        // Indexed access preserves its order and avoids boxing a List<T>
        // enumerator plus its repeated managed field writes. The8-per-field
        // helper coefficient remains unproved; this does not certify it.
        for(int fieldIndex=0;fieldIndex<fieldCount;fieldIndex++)
        {
            var field=fields[fieldIndex];
            if(field.Owner=="247"&&field.Destination=="214")
            {if(selected is not null)throw new InvalidDataException("Unique selected-window field required.");selected=field;}
        }
        if(selected is null||selected.ElementSize!=2||selected.Count!=6)throw new InvalidDataException("Complete actual field214[6] required.");
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FreshSelectedField,12);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FreshSelectedField,12);
        var bytes=selected.CopyBytes();if(bytes.Length!=12)throw new InvalidDataException("Complete source-selected window payload required.");
        const int selectedVariant=2;int value=BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(2*selectedVariant));
        if(value!=20)throw new NotSupportedException("Actual native selected variant2 distance20 required.");
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,80);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,128);
        return new(knowledge,numeric,ledger,value);
    }
    internal void BindInitialization(LegacyEnglishCaseNode0Completion full,LegacyEnglishCasePrimaryCompletion primary,
        LegacyEnglishCaseInitialConsumer consumer,LegacyEnglishCaseInitialQueryPass pass,
        LegacyEnglishFreshCaseMainSource.InitializationLease lease,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FreshSelectedField,512);full.RequireNativeFreshPolicy(this);primary.RequireNode0Initialization(full,consumer,pass,lease);
        var binding=pass.Initializer.PlanForAudit.BindingForAudit;var actual=binding.Normal.Expansion;
        var configuration=primary.ReadNode0InitializationConfiguration(full,consumer,pass,lease);
        if(!ReferenceEquals(pass.Root.Knowledge,knowledge)||!ReferenceEquals(pass.Root.NumericOwner,numeric)||pass.Root.DescriptorFlags!=0xc0e||
            !ReferenceEquals(pass.Initializer.NumericForAudit.Numeric,numeric)||!ReferenceEquals(actual.Rewrite.Contexts,binding.Contexts)||
            !ReferenceEquals(actual.Preparation.Alphabet,knowledge.Source.Alphabet)||actual.Policy.Version!=0x200||binding.Normal.ExtraCostSideTable||
            configuration.HeaderCount2400!=2||configuration.PrimaryAlternativeCount2402!=2||configuration.SecondaryAlternativeCount2404!=1||
            configuration.PrimaryArcLimit230a!=125||configuration.ExpansionFactor230c!=0.25f||configuration.Mode22f6!=0||configuration.PackedSource22e8!=3||
            configuration.Trace2368!=0||configuration.SuppressPreviousContext2609!=0||configuration.OverrideHeap2458!=uint.MaxValue||
            selected28!=20||current50!=-1||current54!=120||current58!=selected28||current68!=120||ignoreCaseC4||knowledgeF5)
            throw new InvalidOperationException("Exact prepared source/init/Expansion and native current scalars required.");
        if(Interlocked.CompareExchange(ref bound,-1,0)!=0)throw new InvalidOperationException("One source-coupled fresh policy binding required.");
        root=pass.Root;initializer=pass.Initializer;expansion=actual;Volatile.Write(ref bound,1);
    }
    internal int Retention
    {
        get
        {
            ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FreshSelectedField,16);if(Volatile.Read(ref bound)!=1)throw new InvalidOperationException("Fresh policy not bound to actual initialization.");
            int result=Math.Min(checked(Math.Max(current50,Math.Max(current58,current68))+current54),Math.Max(current68,checked(current50+current58)));
            if(result!=120)throw new InvalidDataException("Pinned native retention arithmetic differs.");return result;
        }
    }
    internal void RequireStage(LegacyEnglishCaseNode0Completion.Stage stage)
    {
        stage.Require();ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FreshSelectedField,32);stage.RequireNativeFreshPolicy(this);
        if(Volatile.Read(ref bound)!=1||!ReferenceEquals(stage.Lease.Root,root)||!ReferenceEquals(stage.Lease.Owner.Initializer,initializer)||
            !ReferenceEquals(initializer!.PlanForAudit.BindingForAudit.Normal.Expansion,expansion)||!ReferenceEquals(root!.NumericOwner,numeric)||
            !ReferenceEquals(root.Knowledge,knowledge)||ignoreCaseC4||knowledgeF5)
            throw new InvalidOperationException("Exact still-live same-root fresh language policy required.");
    }
    internal bool IgnoreCaseFor(LegacyEnglishCaseNode0Completion.Stage stage){RequireStage(stage);return ignoreCaseC4;}
    internal bool KnowledgeF5For(LegacyEnglishCaseNode0Completion.Stage stage){RequireStage(stage);return knowledgeF5;}
    internal int ReadCurrentCaseCategory(LegacyEnglishCaseNode0Completion.Stage stage,ulong tag,ushort ending)
    {
        RequireStage(stage);ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.TerminalMetadata,32);
        if(expansion is null||ending>=expansion.Preparation.Alphabet.LabelCount)
            throw new NotSupportedException("Actual ordinary ending AL label required before classification.");
        byte kind=expansion.Preparation.ReadClassification(ending);
        int state=(int)(tag>>58)&31;
        return ignoreCaseC4?0:(tag&(1UL<<40))!=0&&kind==8?7:kind==9?8:expansion.Policy.ReadWordCategory(state);
    }
}
