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
internal sealed partial class Math2Mode0OriginalMathFieldSource : IDisposable
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
    // Same concrete InkOwner/Selection representation as original and selected
    // Ink, but a genuinely distinct registry/C22 clone for the CB8 lifetime.
    private sealed class RetainedInk(Math2Mode0OriginalMathFieldSource sourceField, InkOwner ink)
        : Math2Mode0RetainedInk
    {
        private readonly object inkGate = new();
        private bool disposed;
        internal override object Identity => ink;
        internal override LegacyLibraryOriginalDomNameOwner Names => sourceField.SelectedNameOwner;
        internal override long SnapshotNext { get { lock (inkGate) return ink.Next; } }
        private void Parent()
        {
            if (!Monitor.IsEntered(sourceField.gate)) throw new InvalidOperationException("Retained Ink operations require the original field gate.");
            sourceField.CheckScope();
        }
        private void Live() => ObjectDisposedException.ThrowIf(disposed, this);
        private Selection Owned(object selection)
            => selection is Selection actual && ReferenceEquals(actual.Owner, ink) ? actual
                : throw new ArgumentException("C22 must belong to this exact retained Ink clone.");
        private LegacyLibraryOriginalDomName Named(object name)
            => name is LegacyLibraryOriginalDomName actual && ReferenceEquals(actual.Owner, Names) ? actual
                : throw new ArgumentException("Canonical name must belong to the retained shared DOM name owner.");
        private Math2Mode0PublishRegistry[] Rows(bool copy)
            => ink.Registry.Values.OrderBy(e => e.Id).Select(e => new Math2Mode0PublishRegistry(e.Id, e.Name,
                e.Category, copy ? e.C22.Ranges.ToArray() : e.C22.Ranges)).ToArray();
        internal override Math2Mode0PublishRegistry[] BorrowRegistry()
        { Parent(); lock (inkGate) { Live(); return Rows(false); } }
        internal override Math2Mode0PublishRegistry[] CopyRegistry()
        { lock (inkGate) return Rows(true); }
        internal override object CreateSelection(ReadOnlySpan<LegacyNativeInkPositionRange> ranges)
        {
            Parent(); lock (inkGate)
            {
                Live();
                if (ranges.IsEmpty) throw new NotSupportedException("This fresh retained clone admits nonempty produced ranges only.");
                foreach (var range in ranges)
                {
                    LegacyNativeSourceRangeUtf8.ValidatePosition(range.First); LegacyNativeSourceRangeUtf8.ValidatePosition(range.Last);
                    if (!LegacyOwnedRawInkRangeOperations.Contains(sourceField.domain, range)) throw new ArgumentException("C22 range lies outside the retained source INPUT.");
                }
                return new Selection(ink, ranges.ToArray());
            }
        }
        internal override LegacyNativeInkPositionRange[] CopySelectionRanges(object selection)
        { lock (inkGate) return Owned(selection).Ranges.ToArray(); }
        internal override long Register(LegacyLibraryOriginalDomName name, object selection)
        {
            Parent(); lock (inkGate)
            {
                Live(); var actual = Owned(selection); var canonicalName = Named(name);
                if (canonicalName.Text is not ("MATH_SYMBOL" or "MATH_NODE")) throw new NotSupportedException("Only produced Math selection names are admitted.");
                var canonical = ink.Registry.Values.Select(e => e.C22).FirstOrDefault(c =>
                    LegacyOwnedRawInkRangeOperations.Equal(c.Ranges, actual.Ranges)) ?? actual;
                long id = ink.Next++;
                ink.Registry.Add(id, new(id, canonicalName.Text, 102, ink, canonical));
                return id;
            }
        }
        public override int CompareName(object left, object right)
        { Parent(); lock (inkGate) { Live(); return string.CompareOrdinal(Named(left).Text, Named(right).Text); } }
        public override object? FindOldSelection(long id)
        { Parent(); lock (inkGate) { Live(); return ink.Registry.TryGetValue(id, out var entry) ? entry.C22 : null; } }
        public override object? FindOldName(long id)
        { Parent(); lock (inkGate) { Live(); return ink.Registry.TryGetValue(id, out var entry) ? Names.Intern(entry.Name) : null; } }
        public override int CountRanges(object selection)
        { Parent(); lock (inkGate) { Live(); return Owned(selection).Ranges.Length; } }
        public override bool EqualRanges(object oldSelection, object pendingSelection)
        { Parent(); lock (inkGate) { Live(); return LegacyOwnedRawInkRangeOperations.Equal(Owned(oldSelection).Ranges, Owned(pendingSelection).Ranges); } }
        public override bool RemoveSelection(long id)
        {
            Parent(); lock (inkGate)
            {
                Live();
                // Ink069630 returns success immediately when C51 has no ID.
                // Duplicate stale tags therefore still call removal twice,
                // with the second call succeeding as a no-op. Next never rewinds.
                ink.Registry.Remove(id); return true;
            }
        }
        public override void Dispose() { lock (inkGate) disposed = true; }
    }
    private Math2Mode0RetainedInk CloneSelectedInk()
    {
        if (!Monitor.IsEntered(gate)) throw new InvalidOperationException("Selected Ink clone requires the actual field gate.");
        CheckScope();
        if (selectedInk.Registry.Count != 1 || selectedInk.Next != 1)
            throw new NotSupportedException("This factory clones the admitted fresh one-INPUT registry only.");
        var clone = new InkOwner(Document) { Next = selectedInk.Next };
        foreach (var entry in selectedInk.Registry.Values)
        {
            if (!ReferenceEquals(entry.Owner, selectedInk) || !ReferenceEquals(entry.C22.Owner, selectedInk))
                throw new InvalidOperationException("Selected registry lost its owning C22 identity.");
            var selection = new Selection(clone, entry.C22.Ranges.ToArray(), entry.C22.Storage is null);
            clone.Registry.Add(entry.Id, new(entry.Id, entry.Name, entry.Category, clone, selection));
        }
        return new RetainedInk(this, clone);
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
    //Caller validates the produced receipt BEFORE entering this gate. Never
    //acquire its gate here: receipt validation takes receipt -> field order.
    internal T WithFreshPublicationInput<T>(Math2Mode0SelectedMathFieldCopy receipt,
        Func<Math2Mode0PublicationInput, T> action, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(receipt); ArgumentNullException.ThrowIfNull(action);
        lock (gate)
        {
            void Check(CancellationToken cancellation)
            {
                cancellation.ThrowIfCancellationRequested(); CheckScope();
                if (activeBinding is null || !sourcePrepared || !ReferenceEquals(receipt.Field, this))
                    throw new ArgumentException("Exact produced fresh Math field receipt required.");
                Validate(activeBinding, this); receipt.Copy.ValidateForOuterCopy(cancellation);
            }
            Check(token);
            if (selectedInk.Registry.Count != 1 || selectedInk.Next != 1 ||
                !selectedInk.Registry.TryGetValue(0, out var entry) || entry.Category != 102 ||
                entry.Name != "INPUT" || !ReferenceEquals(entry.Owner, selectedInk) ||
                !ReferenceEquals(entry.C22, selectedC22) || !entry.C22.Ranges.SequenceEqual(domain) ||
                !ReferenceEquals(receipt.Copy.Document, Document) || selected.Any(n => n.Name == "mathResult"))
                throw new NotSupportedException("One fresh full INPUT102, no prior result or matcher attachment required.");
            var field = selected.SingleOrDefault(n => n.Kind == 3 && n.Name == "mathField" &&
                selected.Any(a => a.Kind == 0 && a.Name == "id" && a.Parent == n.Id && a.StringValue == FieldId));
            var input = field is null ? null : selected.SingleOrDefault(n => n.Kind == 3 && n.Name == "input" && n.Parent == field.Id);
            if (field is null || input is null || !selected.Any(a => a.Kind == 0 && a.Name == "tag" &&
                a.Parent == input.Id && a.ValueTag == 4 && a.NumericValue == entry.Id))
                throw new InvalidOperationException("Actual selected field/id/input/tag identity required for publication.");
            return action(new(Document, SelectedNameOwner, selected.AsReadOnly(), field.Id, entry.Id,
                selectedInk.Next, entry.C22.Ranges, selectedInk, () => { Check(token); return CloneSelectedInk(); }, Check));
        }
    }
    //Trusted original mutation view reached only through the exact completed
    //outgoing owner while this SAME source gate is already held. No receipt
    //validation is acquired here, and selected/pre-NN state stays untouched.
    internal T WithOriginalApplicationInput<T>(Math2Mode0OutgoingTransfer actualTransfer,object expectedOriginal,
        Func<Math2Mode0OriginalInput,T> action,CancellationToken token)
    {
        if(!Monitor.IsEntered(gate)||!actualTransfer.HasOriginalApplicationAccess(expectedOriginal))throw new ArgumentException("Completed outgoing access must already hold this same original field gate.");
        lock(gate)
        {
            void Check(CancellationToken cancellation)
            {cancellation.ThrowIfCancellationRequested();CheckScope();if(!complete||!sourcePrepared||!ReferenceEquals(expectedOriginal,originalInk)||!actualTransfer.HasOriginalApplicationAccess(originalInk))throw new ArgumentException("Actual prepared original source required.");}
            Math2Mode0OriginalRegistry[] Read()=>originalInk.Registry.Values.OrderBy(e=>e.Id).Select(e=>
                new Math2Mode0OriginalRegistry(e.Id,e.Name,e.Category,e.C22.Ranges,e,e.C22)).ToArray();
            Math2Mode0OriginalSelection Clone(LegacyNativeInkPositionRange[] ranges)
            {Check(token);var selection=new Selection(originalInk,ranges.ToArray());return new(originalInk,selection,selection.Ranges);}
            Math2Mode0OriginalRegistry Register(Math2Mode0TransferCommand command,Math2Mode0OriginalSelection temporary)
            {
                Check(token);if(!actualTransfer.HasOriginalCommandAccess(command)||command.Operation!=4||command.Category!=102||command.Id<next||command.Name is not("MATH_SYMBOL"or"MATH_NODE")||!ReferenceEquals(temporary.Owner,originalInk)||temporary.Identity is not Selection candidate||!ReferenceEquals(candidate.Owner,originalInk)||!candidate.Ranges.SequenceEqual(command.Ranges)||originalInk.Registry.ContainsKey(command.Id))throw new ArgumentException("Actual fresh Math command/temporary C22 required.");
                var canonical=originalInk.Registry.Values.Select(e=>e.C22).FirstOrDefault(c=>c.Ranges.SequenceEqual(candidate.Ranges))??candidate;
                var entry=new RegistryEntry(command.Id,command.Name,102,originalInk,canonical);originalInk.Registry.Add(entry.Id,entry);
                originalInk.Next=System.Math.Max(originalInk.Next,checked(entry.Id+1));
                return new(entry.Id,entry.Name,entry.Category,entry.C22.Ranges,entry,entry.C22);
            }
            void Delete(long id,object expectedEntry)
            {Check(token);if(id<next||!originalInk.Registry.TryGetValue(id,out var entry)||!ReferenceEquals(entry,expectedEntry))throw new ArgumentException("Only exact newly registered original entry can be reversed.");originalInk.Registry.Remove(id);}
            Check(token);
            return action(new(this,originalInk,OriginalNameOwner,original.AsReadOnly(),FieldId,tag,next,
                originalC22??throw new InvalidOperationException("Original INPUT absent."),originalRanges,
                Read,()=>originalInk.Next,Clone,Register,Delete,Check));
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
