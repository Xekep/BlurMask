using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyTextProcessingContext.Probe")]
[assembly: InternalsVisibleTo("LegacyTextPublicationScope.Probe")]
namespace InkRecognition;

/// <summary>A NEW library-owned 2C1 construction/input-import scope. Its
/// result and strings3E are constructed before the selected input DOM. This
/// is not an imported SDK request, a completed recognizer or C20 acceptance.</summary>
public sealed class LegacyLibraryTextProcessingContext : IDisposable
{
    private readonly object gate = new();
    private readonly LegacyLibraryTextProcessingResult result;
    private bool entered, faulted, disposed;
    private bool publicationClaimed, resourcesReleased;
    private int publicationLeases;
    private uint flags;
    private LegacyLibraryTextInputRequest? request;
    internal LegacyLibrarySelectedInputBinding InputBinding { get; }
    public LegacyFreshTextFieldInputContext SelectedInput => InputBinding.LegacyOrThrow;
    public LegacyLibraryOriginalSelectedFieldContext? OriginalSelectedInput => InputBinding.OriginalSelected;
    public LegacyOwnedRawInkDocument Document => InputBinding.Document;
    public LegacyLibraryTextProcessingResult Result => result;
    public bool IsFaulted { get { lock (gate) return faulted; } }
    public uint State18 { get { lock (gate) { Check(); return flags; } } }
    public bool InitialComparison50 => true;
    internal LegacyNativeUtf8StringPool Strings => result.Strings;

    private LegacyLibraryTextProcessingContext(LegacyLibrarySelectedInputBinding input)
    {
        InputBinding = input;
        // 053C30 creates a NEW 2DA. 01C010 creates its NEW strings3E.
        // The private result is never supplied by a caller or replaced by Seed.
        result = new(this);
    }

    public static LegacyLibraryTextProcessingContext CreateFresh(
        LegacyFreshTextFieldInputContext actualSelectedInput, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualSelectedInput); token.ThrowIfCancellationRequested();
        if (actualSelectedInput.Parent is null || !ReferenceEquals(actualSelectedInput.Input.Owner, actualSelectedInput) ||
            actualSelectedInput.NativeElementKind != 0 || actualSelectedInput.ElementName != "textField" ||
            actualSelectedInput.ElementNamespace != "http://www.myscript.com/ns/myscript/page" ||
            actualSelectedInput.Input.InputSpanCount != 0 || actualSelectedInput.Input.CopyRanges().Length == 0)
            throw new NotSupportedException("Actual new full selected textField INPUT/C22 owner required.");
        return new(LegacyLibrarySelectedInputBinding.FromLegacy(actualSelectedInput));
    }

    public static LegacyLibraryTextProcessingContext CreateForOriginalSelected(
        LegacyLibraryOriginalSelectedFieldContext actualSelectedInput, CancellationToken token = default)
    { token.ThrowIfCancellationRequested(); return new(LegacyLibrarySelectedInputBinding.FromOriginal(actualSelectedInput)); }

    /// <summary>Builds an explicit library selected BCD/input DOM, then the
    /// nonempty-C22/no-prior-command 054340 input-only preparation branch.
    /// It does not create an SDK page/content/layout snapshot.</summary>
    public LegacyLibraryTextInputRequest PrepareInput(CancellationToken token = default)
        => PrepareCore(-1, -1, -1, token);

    internal LegacyLibraryTextInputRequest PrepareInputPrepared(int domFailure = -1,
        int preparationFailure = -1, int cancellationStage = -1, CancellationToken token = default)
        => PrepareCore(domFailure, preparationFailure, cancellationStage, token);

    private LegacyLibraryTextInputRequest PrepareCore(int domFailure, int preparationFailure,
        int cancellationStage, CancellationToken token)
    {
        lock (gate)
        {
            Check(); token.ThrowIfCancellationRequested();
            if (entered) throw new InvalidOperationException("Fresh request preparation is single-use; no prior/history import is supported.");
            if (preparationFailure is < -1 or > 1 || cancellationStage is < -1 or > 3)
                throw new ArgumentOutOfRangeException(nameof(preparationFailure));
            entered = true;
            var arena = new LegacyLibraryTextDomArena(Document, domFailure);
            request = new(this, arena);
            try
            {
                // The field's registered C98 semantic owner is SelectedInput;
                // its DOM kind3 must not be confused with C98 field-kind0.
                request.BuildInputDom(token);
                Stage(0); // Temporary2DB creation precedes any2DA import.
                var snapshot = new InputOnlySnapshot(request);
                try
                {
                    foreach (string name in ImportNames) arena.NamePool.Intern(name);
                    result.ResetForImport(); request.Record("result:reset");
                    result.BindRequest(request); request.Record("result:bind");
                    Stage(1); // Literal range-vector allocation refusal.
                    result.ImportInputRanges(); request.Record("result:ranges");
                    Stage(2); //Managed cancellation checkpoint, not a false
                    //callback: native050660 D8==0 returns true directly.
                    snapshot.Read(result);
                    // This constructed field has input and no textResult.
                    // Snapshot D8 is therefore zero; C22 ranges are nonempty.
                    flags = 0;
                    if (result.RangeCount210 != 0) flags = 1;
                    Stage(3);
                    request.Record("context:flags=" + flags);
                    request.Complete(); return request;
                }
                finally { snapshot.Dispose(); }
            }
            catch
            {
                // Native05472D clears flags after a failed temporary/import
                // attempt. Portable entered failures also poison retry.
                flags = 0; faulted = true; request.Record("context:fault"); throw;
            }

            void Stage(int ordinal)
            {
                token.ThrowIfCancellationRequested();
                if (ordinal == cancellationStage) throw new OperationCanceledException("Prepared request-stage cancellation prefix.");
                if (ordinal == preparationFailure) throw new InvalidOperationException("Prepared native allocation/import refusal.");
            }
        }
    }

    internal LegacyLibraryTextInputRequest? PreparedRequest => request;
    internal uint PreparedState18 => flags;
    internal bool ResourcesReleased { get { lock(gate) return resourcesReleased; } }
    internal LegacyLibraryTextProcessingLease RetainForPublication()
    {
        lock(gate)
        {
            Check();
            if(publicationClaimed || request is null || !request.Succeeded || flags!=1 || result.Strings.Count!=0)
                throw new InvalidOperationException("One successfully imported, unpublished input-only context required.");
            publicationClaimed=true;publicationLeases++;
            return new(this);
        }
    }
    internal void ReleasePublication()
    {lock(gate){if(publicationLeases<=0)throw new InvalidOperationException("Unbalanced publication lease.");publicationLeases--;ReleaseIfClosed();}}
    private void ReleaseIfClosed()
    {
        if(!disposed||publicationLeases!=0||resourcesReleased)return;
        resourcesReleased=true;result.ReleaseBindings();result.Strings.Dispose();request?.ReleaseOwnedTree();
    }
    internal T Read<T>(Func<T> snapshot) { lock (gate) return snapshot(); }
    private void Check() { ObjectDisposedException.ThrowIf(disposed, this); if (faulted) throw new InvalidOperationException("Faulted processing context."); }
    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            disposed = true; ReleaseIfClosed();
        }
    }

    // Exact02D8C0 pre-reset DOM.V18 name order, including repeated "line".
    internal static readonly string[] ImportNames = ["input", "conf", "textResult", "charSegmentList",
        "charSegment", "charCandidate", "external", "internal", "wordSegmentList", "wordSegment",
        "wordCandidate", "char", "text", "textCandidate", "word", "lines", "line", "transforms",
        "range", "tag", "line", "freeze", "forceSelected", "origin", "ref", "selected", "id",
        "x", "y", "a", "h", "b", "gen", "lang", "x-height"];

    // The genuine050660 D8==0 branch stores the actual bound input owner
    // before returning. It allocates no CHAR/WORD/TEXT snapshot arrays.
    private sealed class InputOnlySnapshot(LegacyLibraryTextInputRequest request) : IDisposable
    {
        private LegacyLibrarySelectedInputBinding? input;
        private bool released;
        private readonly LegacyLibraryTextInputRequest source = Initialize(request);
        private static LegacyLibraryTextInputRequest Initialize(LegacyLibraryTextInputRequest owner)
        { owner.Record("snapshot2DB:create"); return owner; }
        internal void Read(LegacyLibraryTextProcessingResult result)
        {
            if (released || !ReferenceEquals(result.Owner, source.Owner) ||
                !ReferenceEquals(result.BoundRequest, source) || result.TextCountD8 != 0)
                throw new NotSupportedException("Only the imported input-only2DA empty TEXT snapshot is supported.");
            input = result.BoundInputBinding ?? throw new InvalidDataException("Snapshot requires the imported actual selected C20.");
            if (!ReferenceEquals(input, source.Owner.InputBinding)) throw new InvalidDataException("Foreign snapshot input owner.");
            source.Record("snapshot2DB:read-empty-text");
        }
        public void Dispose()
        { if (released) return; input = null; released = true; source.Record("snapshot2DB:release"); }
    }
}

/// <summary>Owned newly constructed2DA input-import state. Pointer identities
/// are typed references; no synthetic native addresses or accepted result.</summary>
public sealed class LegacyLibraryTextProcessingResult
{
    private LegacyNativeInkPositionRange[] ranges = [];
    private long sourceTag30 = -1;
    private LegacyLibraryTextInputRequest? boundRequest;
    private LegacyLibrarySelectedInputBinding? boundInput;
    internal LegacyNativeUtf8StringPool Strings { get; } = new();
    private WeakReference<LegacyFreshMainTextDocumentSeed>? publicationSeed;
    private byte[] identifier38=new byte[16];
    public LegacyLibraryTextProcessingContext Owner { get; }
    public long SourceTag30 => Owner.Read(() => sourceTag30);
    public int RangeCount210 => Owner.Read(() => ranges.Length);
    public int TextCountD8 => 0;
    public LegacyLibraryTextInputRequest? BoundRequest => Owner.Read(() => boundRequest);
    public LegacyFreshTextFieldInputContext? BoundInput28 => Owner.Read(() => boundInput?.LegacyOrThrow);
    public LegacyLibraryOriginalSelectedFieldContext? OriginalSelectedInput28 => Owner.Read(() => boundInput?.OriginalSelected);
    internal LegacyLibrarySelectedInputBinding? BoundInputBinding => boundInput;
    internal LegacyLibraryTextProcessingResult(LegacyLibraryTextProcessingContext owner) { Owner = owner; }
    public byte[] CopyIdentifier38() => Owner.Read(() => identifier38.ToArray());
    public LegacyFreshMainTextDocumentSeed? PublicationSeed => Owner.Read(() =>
        publicationSeed is not null&&publicationSeed.TryGetTarget(out var seed)?seed:null);
    internal void BindPublicationSeed(LegacyFreshMainTextDocumentSeed seed)
    {
        Owner.Read(()=>
        {
            if(publicationSeed is not null||!ReferenceEquals(seed.Document,Owner.Document)||
                !ReferenceEquals(seed.InputBinding,boundInput)||!ReferenceEquals(seed.StringPool,Strings))
                throw new ArgumentException("Exact imported2DA/input/shared3E seed required before publication.");
            publicationSeed=new(seed);return 0;
        });
    }
    internal void CapturePublicationIdentifier(LegacyFreshMainTextDocumentSeed seed)
    {
        Owner.Read(()=>
        {
            if(!ReferenceEquals(PublicationSeed,seed))throw new ArgumentException("Exact adopted seed required.");
            identifier38=seed.CopyIdentifier38();return 0;
        });
    }
    public LegacyNativeInkPositionRange[] CopyInputRanges208() => Owner.Read(() => ranges.ToArray());
    public byte[] CopyInputRangeStorage() => Owner.Read(CopyInputRangeStorageCore);
    private byte[] CopyInputRangeStorageCore()
    {
        byte[] bytes = new byte[ranges.Length * 16];
        for (int i = 0; i < ranges.Length; i++)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(i * 16), ranges[i].First.Value);
            BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(i * 16 + 8), ranges[i].Last.Value);
        }
        return bytes;
    }
    internal void ResetForImport() { sourceTag30 = -1; boundRequest = null; boundInput = null; ranges = []; }
    internal void BindRequest(LegacyLibraryTextInputRequest request)
    {
        if (!ReferenceEquals(request.Owner, Owner)) throw new ArgumentException("Exact2C1 request owner required.");
        boundRequest = request; boundInput = Owner.InputBinding;
        sourceTag30 = request.ReadInputTag();
    }
    internal void ImportInputRanges()
    {
        if (boundRequest is null || !ReferenceEquals(boundInput, Owner.InputBinding) || sourceTag30 != Owner.InputBinding.RegistryId)
            throw new InvalidOperationException("Actual input.tag -> selected C20.V128 -> C22 ownership required.");
        ranges = Owner.InputBinding.CopyRanges();
    }
    internal void ReleaseBindings() { boundRequest = null; boundInput = null; }
}

internal sealed class LegacyLibraryTextProcessingLease : IDisposable
{
    private LegacyLibraryTextProcessingContext? owner;
    internal LegacyLibraryTextProcessingLease(LegacyLibraryTextProcessingContext owner){this.owner=owner;}
    internal void Validate()=>ObjectDisposedException.ThrowIf(owner is null,this);
    public void Dispose(){Interlocked.Exchange(ref owner,null)?.ReleasePublication();}
}

/// <summary>Explicit library BCD selected-field view with its actual retained
/// selected INPUT/C22. Namespace/C98 comes from that sealed source owner;
/// this is not a cloned SDK page or a supplied history/cache request.</summary>
public sealed class LegacyLibraryTextInputRequest
{
    private LegacyDomNode? root, fieldNode, input;
    private bool succeeded, released;
    private readonly List<string> trace = [];
    public LegacyLibraryTextProcessingContext Owner { get; }
    public LegacyLibraryTextDomArena Arena { get; }
    public LegacyFreshTextFieldInputContext SelectedInput => Owner.SelectedInput;
    public LegacyLibraryOriginalSelectedFieldContext? OriginalSelectedInput => Owner.OriginalSelectedInput;
    public LegacyRetainedTextFieldInputSelection Input => SelectedInput.Input;
    public string FieldNamespace => Owner.InputBinding.ElementNamespace;
    public int FieldKind => Owner.InputBinding.ElementKind;
    public bool Succeeded => Owner.Read(() => succeeded);
    public LegacyLibraryTextDomNode Field => Owner.Read(() => !released && fieldNode is not null ? Arena.Snapshot(fieldNode) : throw new InvalidOperationException("No live selected field."));
    public LegacyLibraryTextDomNode Root => Owner.Read(() => !released && root is not null ? Arena.Snapshot(root) : throw new InvalidOperationException("No live request root."));
    internal LegacyLibraryTextInputRequest(LegacyLibraryTextProcessingContext owner, LegacyLibraryTextDomArena arena) { Owner = owner; Arena = arena; }
    internal void BuildInputDom(CancellationToken token)
    {
        if (OriginalSelectedInput is {} selected)
        {
            // Read-only managed reader facade over the exact selected PAGE
            // snapshot; no second native worker clone or replacement source.
            var imported = Arena.ImportSelectedPage(selected, token);
            root=imported.Root;fieldNode=imported.Field;input=imported.Input;return;
        }
        token.ThrowIfCancellationRequested(); root = Arena.ConstructorRoot();
        fieldNode = Arena.Element(SelectedInput.ElementName) ?? throw new InvalidOperationException("Field allocation refusal.");
        Arena.Append(root, fieldNode);
        input = Arena.Element("input") ?? throw new InvalidOperationException("INPUT allocation refusal.");
        Arena.Append(fieldNode, input);
        if (!Arena.I64(input, "tag", Input.RegistryId)) throw new InvalidOperationException("INPUT tag allocation refusal.");
    }
    internal long ReadInputTag()
    {
        if (input is null || fieldNode is null || !ReferenceEquals(input.Parent, fieldNode) || input.Children.Count != 0)
            throw new NotSupportedException("Only the constructed input-only field is supported.");
        var tag = input.Attributes.Single(a => a.Name == "tag");
        if (tag.Tag != 4 || tag.Value.Length != 8) throw new InvalidDataException("Exact signed64 input.tag required.");
        return BinaryPrimitives.ReadInt64LittleEndian(tag.Value);
    }
    internal void Record(string value) => trace.Add(value);
    internal string[] CopyPreparationTrace() => trace.ToArray();
    internal void Complete() => succeeded = true;
    internal void ReleaseOwnedTree() { if (root is not null && !root.Released) Arena.Release(root); released = true; }
}
