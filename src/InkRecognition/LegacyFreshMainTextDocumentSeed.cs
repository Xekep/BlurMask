using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>The modeled NEW2DA fields created before shared WORD publication.
/// The retained selected INPUT/C22 and pending C4D owner are exact references;
/// this is neither an imported SDK result nor an accepted document.</summary>
public sealed class LegacyFreshMainTextDocumentSeed
{
    private readonly LegacyMainSharedTextSourceState shared;
    private readonly byte[] dafInputRows32;
    private readonly LegacyDocumentTransformVector initialTransforms=LegacyDocumentTransformVector.Fresh();
    private LegacyDocumentTransformVector currentTransforms;
    public LegacyFreshTextFieldInputContext SelectedInputContext {get;}
    public LegacyRetainedTextFieldInputSelection Input=>SelectedInputContext.Input;
    public LegacyOwnedRawInkDocument Document=>SelectedInputContext.Document;
    /// <summary>The actual pre-NN imported2DA for the adopted route; old
    /// independently constructed seeds have no processing-result owner.</summary>
    public LegacyLibraryTextProcessingResult? ProcessingResult=>shared.PublicationScope?.Context.Result;
    public long SourceTag30=>Input.RegistryId;
    public int InitialDocumentAlternativeCountD8=>0;
    public int InitialTransformCount110=>initialTransforms.Count;
    /// <summary>Result38..47 identifier using the restored native algorithm
    /// with an explicitly library-owned cryptographic initial state.</summary>
    private LegacyLibraryTextResultIdentifier? identifier;
    public LegacyLibraryTextResultIdentifier LibraryIdentifier38
    {get{shared.PublicationScope?.Validate();return identifier??throw new InvalidOperationException("Publication identifier has not yet been initialized.");}}
    internal byte[] CopyIdentifier38()=>identifier?.CopyRawStorage16()??new byte[16];
    internal void InitializePublicationIdentifier()
    {
        if(identifier is not null)throw new InvalidOperationException("Publication identifier is single-use.");
        identifier=LegacyLibraryUuidGenerator.Create().NextDistinct(new byte[16]);
    }
    internal LegacyNativeUtf8StringPool StringPool=>shared.Strings;
    internal LegacyOwnedFreshTextSourceContext PendingSelections=>shared.Selections;
    internal LegacyDocumentTransformVector InitialTransforms=>initialTransforms;
    internal LegacyDocumentTransformVector CurrentTransforms=>currentTransforms;

    internal LegacyFreshMainTextDocumentSeed(LegacyMainSharedTextSourceState actualShared,
        LegacyFreshTextFieldInputContext actualSelectedInput,bool deferIdentifier=false)
    {
        if(actualSelectedInput.Parent is null||!ReferenceEquals(actualShared.Document,actualSelectedInput.Document)||
            !ReferenceEquals(actualShared.Selections.SelectedInputContext,actualSelectedInput)||actualShared.Strings.Count!=0)
            throw new ArgumentException("A new selected INPUT result seed must precede all shared string/WORD publication.");
        shared=actualShared;SelectedInputContext=actualSelectedInput;currentTransforms=initialTransforms;
        //Native01C010 clears38..47;032550 replaces that zero identifier before
        //WORD publication. Native process-address RNG seeding is not portable.
        if(!deferIdentifier)InitializePublicationIdentifier();
        //05F200 reads DAF.V38 from the actual new INPUT before2DA.V18.
        //Its immutable span-child producer, not transform output, proves0.
        dafInputRows32=actualSelectedInput.Input.CopyDafInputRows32();
    }

    /// <summary>Assigned scalar fields only: source30, initialD8, initial110,
    /// initial114. Native pointer lanes and all other2DA fields are omitted.</summary>
    public byte[] CopyInitialScalarStorage20()
    {
        byte[] data=new byte[20];BinaryPrimitives.WriteInt64LittleEndian(data,SourceTag30);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(12),InitialTransformCount110);return data;
    }
    public byte[] CopyDafInputRows32()=>dafInputRows32.ToArray();
    internal LegacyDocumentTransformSwap SwapTransforms(LegacyDocumentTransformVector exported,bool succeeded)
        =>LegacyDocumentTransformSwap.Apply(ref currentTransforms,exported,succeeded);

    internal void Validate(LegacyFreshMainWordSourceChoices actualWords,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        shared.PublicationScope?.Validate();
        if(!ReferenceEquals(actualWords.DocumentSeed,this)||!ReferenceEquals(actualWords.Document,Document)||
            !ReferenceEquals(actualWords.SelectionContext,PendingSelections)||
            !ReferenceEquals(actualWords.SelectedInputContext,SelectedInputContext)||
            !ReferenceEquals(Input.Owner,SelectedInputContext))
            throw new ArgumentException("Exact seed, selected C20/C22, shared3E and pending C4D ownership required.");
        foreach(var word in actualWords.WordViews)word.Validate(token);
    }
}
