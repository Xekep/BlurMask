using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("LegacyMathOriginalFieldSource.Probe")]

namespace InkRecognition.Math;

internal enum Math2Mode0MathFieldStage { OriginalScaffold, OriginalField, InputInserted, Registered, Tagged,
    SelectedScaffold, SelectedField, SelectedRegistry, C23Constructed, C23Selected, Ready }
internal enum Math2Mode0PreparedMathInput { Ordinary, MissingInput, MissingTag, MissingRegistry, ForeignTag, EmptyNull, EmptyAllocated, Partial }
internal sealed record Math2Mode0MathFieldNode(int Id, byte Kind, string Name, int Parent, int OriginalNode,
    byte ValueTag = 0, string? StringValue = null, long? NumericValue = null,
    string? NamespaceUri = null, string? NamespacePrefix = null);
internal sealed record Math2Mode0MathFieldSnapshot(bool Complete, bool Failed, bool InferenceClaimed, bool SourcePrepared,
    bool Closed, bool CanInfer, long Tag, long Next, int Category, long PayloadReservation,
    Math2Mode0MathFieldNode[] OriginalNodes, Math2Mode0MathFieldNode[] SelectedNodes,
    LegacyNativeInkPositionRange[] OriginalRanges, LegacyNativeInkPositionRange[] SelectedRanges,
    LegacyNativeInkPositionRange[] C23Ranges, string[] Trace);

/// <summary>One library original mathField/INPUT102, its selected clone and
/// actual empty-first C23. No CBB/store, native history receiver or DOM acceptance.</summary>
internal sealed class Math2Mode0OriginalMathFieldSource : IDisposable
{
    internal const long DefaultPayloadBudget = 16 * 1024 * 1024;
    private const string PageNamespace = "http://www.myscript.com/ns/myscript/page";
    private readonly object gate = new(), fieldIdentity = new(), c23Identity = new();
    private readonly InkOwner originalInk, selectedInk;
    private sealed class InkOwner
    {
        internal LegacyOwnedRawInkDocument Document { get; }
        internal readonly Dictionary<long, RegistryEntry> Registry = [];
        internal long Next;
        internal InkOwner(LegacyOwnedRawInkDocument document) { Document = document; }
    }
    private sealed record RegistryEntry(long Id, string Name, int Category, InkOwner Owner, Selection C22);
    private sealed class Selection
    {
        internal InkOwner Owner { get; }
        internal LegacyNativeInkPositionRange[] Ranges { get; }
        internal object? Storage { get; }
        internal Selection(InkOwner owner, LegacyNativeInkPositionRange[] ranges, bool nullEmpty = false)
        { Owner = owner; Ranges = ranges; Storage = nullEmpty ? null : new object(); }
    }
    private readonly List<Math2Mode0MathFieldNode> original = [], selected = [];
    private readonly List<string> trace = [];
    private readonly LegacyNativeInkPositionRange[] domain;
    private LegacyNativeInkPositionRange[] originalRanges = [], selectedRanges = [], c23 = [];
    private Selection? originalC22, selectedC22;
    private object? c23Storage;
    private Binding? activeBinding;
    private long tag = -1, next;
    private bool complete, failed, closed, claimed, sourcePrepared, canInfer;
    internal LegacyFreshRawInkCurrentView CurrentView { get; }
    internal LegacyOwnedRawInkDocument Document => CurrentView.Document;
    internal string FieldId { get; }
    //Generic library BEE tokens, not text-field bootstrap ownership.
    internal LegacyLibraryOriginalDomNameOwner OriginalNameOwner { get; } = new();
    internal LegacyLibraryOriginalDomNameOwner SelectedNameOwner { get; } = new();
    private readonly Dictionary<int, LegacyLibraryOriginalDomName> originalNames = [], selectedNames = [];
    internal LegacyLibraryOriginalDomName ReadCanonicalName(bool workerSelected, int nodeId)
    {
        lock (gate)
        {
            CheckScope(); var names = workerSelected ? selectedNames : originalNames;
            return names.TryGetValue(nodeId, out var name) ? name : throw new ArgumentException("Actual named node identity required.");
        }
    }
    //Additional retained payload only: Document/map XY is reserved by the
    //source-position producer; the view's XY copy and this DOM/registry are extra.
    internal long AdditionalPayloadReservation { get; }
    internal object OriginalInkIdentity => originalInk;
    internal object SelectedInkIdentity => selectedInk;
    internal object C23Identity => c23Identity;
    internal object FieldIdentity => fieldIdentity;
    internal object OriginalSelectionIdentity => originalC22 ?? throw new InvalidOperationException("No original INPUT.");
    internal object SelectedSelectionIdentity => selectedC22 ?? throw new InvalidOperationException("No selected INPUT.");

    private Math2Mode0OriginalMathFieldSource(LegacyFreshRawInkCurrentView view, string id, long payload)
    {
        CurrentView = view; FieldId = id; AdditionalPayloadReservation = payload;
        originalInk = new(view.Document); selectedInk = new(view.Document);
        domain = [new(LegacyNativeInkPosition.Pack(0, 0, 3, Document.Strokes[0].Points.Count),
            LegacyNativeInkPosition.Pack((uint)(Document.ObjectCount - 1), Document.Strokes[^1].Points.Count - 1,
                5, Document.Strokes[^1].Points.Count))];
    }
    internal static Math2Mode0OriginalMathFieldSource Prepare(LegacyFreshRawInkCurrentView actualView,
        string fieldId, CancellationToken token = default)
        => PrepareCore(actualView, fieldId, DefaultPayloadBudget, Math2Mode0PreparedMathInput.Ordinary, null, token);
    internal static Math2Mode0OriginalMathFieldSource PrepareDiagnostic(LegacyFreshRawInkCurrentView actualView,
        string fieldId, long budget, Math2Mode0PreparedMathInput input,
        Action<Math2Mode0MathFieldStage, Math2Mode0OriginalMathFieldSource>? observer, CancellationToken token = default)
        => PrepareCore(actualView, fieldId, budget, input, observer, token);
    private static Math2Mode0OriginalMathFieldSource PrepareCore(LegacyFreshRawInkCurrentView view,
        string id, long budget, Math2Mode0PreparedMathInput input,
        Action<Math2Mode0MathFieldStage, Math2Mode0OriginalMathFieldSource>? observer, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(view); ArgumentNullException.ThrowIfNull(id); token.ThrowIfCancellationRequested();
        if (!Enum.IsDefined(input)) throw new ArgumentOutOfRangeException(nameof(input));
        if (budget < 0 || budget > DefaultPayloadBudget) throw new ArgumentOutOfRangeException(nameof(budget));
        if (id.Length is < 1 or > 64 || id.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not ('_' or '-')))
            throw new NotSupportedException("One fresh single-component Math field id required.");
        Math2Mode0RawSourcePositions.ValidateDocument(view.Document, token);
        if (view.Curves.Count != view.Document.ObjectCount || view.Views.Count != view.Document.ObjectCount)
            throw new ArgumentException("Complete actual raw current view required.");
        for (int i = 0; i < view.Curves.Count; i++)
            if (!ReferenceEquals(view.Curves[i].Document, view.Document) || view.Curves[i].OriginalSourceIndex != i ||
                view.Views[i].Descriptor.SourceIndex != (uint)i)
                throw new ArgumentException("Original source slots and selected view must coincide.");
        long points = view.Document.Strokes.Sum(s => (long)s.Points.Count);
        long payload = checked(points * 8 + 2 * 12 * 64 + Encoding.UTF8.GetByteCount(id) * 2L +
            Encoding.UTF8.GetByteCount(PageNamespace) * 2L + 4 * 16 + 2 * 160);
        if (payload > budget) throw new NotSupportedException("Math field retained-payload envelope exceeds budget before construction.");
        var owner = new Math2Mode0OriginalMathFieldSource(view, id, payload);
        try { owner.Build(input, observer, token); return owner; }
        catch { owner.failed = true; throw; }
    }
    private void Build(Math2Mode0PreparedMathInput input,
        Action<Math2Mode0MathFieldStage, Math2Mode0OriginalMathFieldSource>? observer, CancellationToken token)
    {
        void Observe(Math2Mode0MathFieldStage stage)
        {
            foreach (var n in original.Where(n => n.Kind is 0 or 3)) originalNames.TryAdd(n.Id, OriginalNameOwner.Intern(n.Name));
            foreach (var n in selected.Where(n => n.Kind is 0 or 3)) selectedNames.TryAdd(n.Id, SelectedNameOwner.Intern(n.Name));
            OriginalNameOwner.Intern(PageNamespace); OriginalNameOwner.Intern("");
            SelectedNameOwner.Intern(PageNamespace); SelectedNameOwner.Intern("");
            observer?.Invoke(stage, this); CheckScope(); token.ThrowIfCancellationRequested();
        }
        static void Scaffold(List<Math2Mode0MathFieldNode> nodes)
        {
            nodes.Add(new(1, 4, "", 0, 0)); nodes.Add(new(2, 3, "page", 1, 0));
            nodes.Add(new(3, 2, "", 2, 0, NamespaceUri: PageNamespace, NamespacePrefix: ""));
            nodes.Add(new(4, 0, "version", 2, 0, 7, null, 5));
            nodes.Add(new(5, 3, "content", 2, 0)); nodes.Add(new(6, 3, "layout", 2, 0));
        }
        Scaffold(original); trace.Add("original:CB5/PAGE:no-reader-receiver"); Observe(Math2Mode0MathFieldStage.OriginalScaffold);
        original.Add(new(7, 3, "mathField", 5, 0)); original.Add(new(8, 0, "id", 7, 0,
            FieldId.Length < 16 ? (byte)2 : (byte)1, FieldId));
        trace.Add("13B9:page-namespace:mathField-marker:C98.kind2"); Observe(Math2Mode0MathFieldStage.OriginalField);
        if (input != Math2Mode0PreparedMathInput.MissingInput)
        { original.Add(new(9, 3, "input", 7, 0)); trace.Add("005380:input-before-registration"); }
        Observe(Math2Mode0MathFieldStage.InputInserted);
        originalRanges = input is Math2Mode0PreparedMathInput.EmptyNull or Math2Mode0PreparedMathInput.EmptyAllocated ? [] : domain.ToArray();
        if (input == Math2Mode0PreparedMathInput.Partial)
            originalRanges = [new(domain[0].First, LegacyNativeInkPosition.Pack(0, 0, 5, Document.Strokes[0].Points.Count))];
        if (input != Math2Mode0PreparedMathInput.MissingRegistry)
        {
            originalC22 = new(originalInk, originalRanges, input == Math2Mode0PreparedMathInput.EmptyNull);
            tag = originalInk.Next; originalInk.Registry.Add(tag, new(tag, "INPUT", 102, originalInk, originalC22));
            originalInk.Next++; next = originalInk.Next; trace.Add("C51:INPUT:category102:assigned0:Next1");
        }
        Observe(Math2Mode0MathFieldStage.Registered);
        if (input is not (Math2Mode0PreparedMathInput.MissingInput or Math2Mode0PreparedMathInput.MissingTag))
            original.Add(new(10, 0, "tag", 9, 0, 4, null, input == Math2Mode0PreparedMathInput.ForeignTag ? 0x100000000L : 0));
        Observe(Math2Mode0MathFieldStage.Tagged);
        Scaffold(selected); trace.Add("selected:C20-history0:new-BCC"); Observe(Math2Mode0MathFieldStage.SelectedScaffold);
        foreach (var node in original.Where(n => n.Id >= 7)) selected.Add(node with { OriginalNode = node.Id });
        trace.Add("clone:attributes-before-children:same-original-node-identity"); Observe(Math2Mode0MathFieldStage.SelectedField);
        foreach (var entry in originalInk.Registry.Values.OrderBy(e => e.Id))
        {
            selectedRanges = entry.C22.Ranges.ToArray();
            selectedC22 = new(selectedInk, selectedRanges, entry.C22.Storage is null);
            selectedInk.Registry.Add(entry.Id, new(entry.Id, entry.Name, entry.Category, selectedInk, selectedC22));
        }
        selectedInk.Next = originalInk.Next;
        if (originalC22 != null) trace.Add("06D2C0:preserve-tag/category/Next:new-C22");
        Observe(Math2Mode0MathFieldStage.SelectedRegistry);
        c23 = []; c23Storage = null; trace.Add("C23:retain-same-selected-C20:empty:C43.V0-noop");
        Observe(Math2Mode0MathFieldStage.C23Constructed);
        var selectedInput = selected.SingleOrDefault(n => n.Name == "input" && n.Parent == 7);
        var inputTag = selectedInput == null ? null : selected.SingleOrDefault(n => n.Name == "tag" && n.Parent == selectedInput.Id);
        RegistryEntry? resolved = null;
        if (inputTag?.NumericValue is long id && id != -1)
            selectedInk.Registry.TryGetValue(id, out resolved); //Complete signed64-bit key.
        if (resolved != null)
        {
            if (!ReferenceEquals(resolved.Owner, selectedInk) || !ReferenceEquals(resolved.C22.Owner, selectedInk) ||
                resolved.Category != 102 || resolved.Name != "INPUT")
                throw new ArgumentException("The exact selected Math INPUT102/C22 owner is required.");
            object? incomingStorage = resolved.C22.Storage;
            if (ReferenceEquals(c23Storage, incomingStorage) && c23.Length == resolved.C22.Ranges.Length)
                trace.Add("086C65:equal-ptr/count:no-op");
            else
            {
                c23 = domain.ToArray(); c23Storage = new(); trace.Add("086E26:same-C20-domain-copy");
                c23 = LegacyOwnedRawInkRangeOperations.Prepare(Document).Intersect(c23, resolved.C22.Ranges, token);
                trace.Add("086E6E:VC0-intersect-actual-INPUT:0860A0-ordinary-no-C25");
            }
        }
        else trace.Add(inputTag?.NumericValue is long value && value != -1 ? "DAF:missing-registry:reported-null" : "DAF:no-input-tag:null");
        Observe(Math2Mode0MathFieldStage.C23Selected);
        //Production owns complete ordinary raw objects. A diagnostic partial
        //C23 remains inspectable but cannot authorize the full-source NN route.
        canInfer = input == Math2Mode0PreparedMathInput.Ordinary && resolved != null && c23.SequenceEqual(domain);
        if (canInfer) CurrentView.ExportIdentityTransforms(c23, token);
        trace.Add(c23.Length == 0 ? "C31.V40:empty:no-NN" : "C31.V40:nonempty");
        complete = true; Observe(Math2Mode0MathFieldStage.Ready);
    }
    internal Math2Mode0MathFieldSnapshot CopySnapshot()
    {
        lock (gate) return new(complete, failed, claimed, sourcePrepared, closed, canInfer, tag, next, 102,
            AdditionalPayloadReservation, original.ToArray(), selected.ToArray(), originalRanges.ToArray(),
            selectedRanges.ToArray(), c23.ToArray(), trace.ToArray());
    }
    internal Binding ClaimForInference(CancellationToken token)
    {
        lock (gate)
        {
            CheckScope(); token.ThrowIfCancellationRequested();
            if (!complete || claimed || !canInfer) throw new InvalidOperationException("Complete nonempty actual Math INPUT102/C23 and unconsumed source required.");
            claimed = true;
            try { activeBinding = new(this); return activeBinding; }
            catch { failed = true; throw; }
        }
    }
    private void CheckScope()
    { ObjectDisposedException.ThrowIf(closed, this); if (failed) throw new InvalidOperationException("Math source retains a failed prefix."); }
    internal void Fail(Binding binding)
    { lock (gate) { if (!ReferenceEquals(activeBinding, binding)) throw new ArgumentException("Foreign Math inference binding."); failed = true; } }
    internal void SourcePrepared(Binding binding)
    { lock (gate) { Validate(binding, this); sourcePrepared = true; } }
    private void Validate(Binding binding, Math2Mode0OriginalMathFieldSource expected)
    {
        CheckScope();
        if (!ReferenceEquals(this, expected) || !complete || !claimed || !canInfer || !ReferenceEquals(activeBinding, binding))
            throw new ArgumentException("Exact original/selected Math field and C23 inference claim required.");
    }
    internal sealed class Binding
    {
        internal Math2Mode0OriginalMathFieldSource Owner { get; }
        internal LegacyOwnedRawInkDocument Document => Owner.Document;
        internal object C23Identity => Owner.C23Identity;
        internal long AdditionalPayloadReservation => Owner.AdditionalPayloadReservation;
        internal Binding(Math2Mode0OriginalMathFieldSource owner) { Owner = owner; }
        internal void Validate(Math2Mode0OriginalMathFieldSource expected, CancellationToken token = default)
        { lock (Owner.gate) { token.ThrowIfCancellationRequested(); Owner.Validate(this, expected); } }
        internal void ValidatePrepared(CancellationToken token = default)
        { lock (Owner.gate) { Validate(Owner, token); if (!Owner.sourcePrepared) throw new InvalidOperationException("Math source preparation incomplete."); } }
    }
    public void Dispose()
    { lock (gate) { if (closed) return; trace.Add("C23:detach-same-C20:close-library-scope"); closed = true; } }
}
