using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("LegacyOriginalTextFieldBootstrap.Probe")]
namespace InkRecognition;

/// <summary>A library-owned original PAGE/field/INPUT bootstrap over an existing
/// immutable ordinary raw source. Restores fresh CB5/11D3C0/addItems ordering
/// and records its attached DOM and C51 mutations. The retained library journal
/// is not a native history manager, store, revision or SDK page receiver.</summary>
public sealed class LegacyLibraryOriginalTextFieldBootstrap
{
    private readonly object gate = new();
    private readonly string[] ids;
    private readonly List<Node> nodes = [];
    private readonly List<LegacyLibraryOriginalHistoryMutation> history = [];
    private readonly List<LegacyLibraryOriginalFieldSelection> selections = [];
    private readonly List<LegacyLibraryOriginalSourceRegistryEvent> sourceEvents = [];
    private readonly List<string> trace = [];
    private readonly List<LegacyLibraryOriginalObserverState> observer = [];
    //The fresh parameterized BBD has no nodes. This owner has no backend-build
    //entrypoint, so an actual canonical-key lookup can only miss in this scope.
    private readonly Dictionary<string, Node> fieldBackends = new(StringComparer.Ordinal);
    private readonly LegacyNativeInkPositionRange[] ranges;
    private bool entered;
    private long next, maximum = long.MinValue;
    private int attempts, failAt = -1, cancelAt = -1, nesting, pending130, retainedF0;
    private Node? root, page, content, layout;
    private Node? pendingField, retainedField;
    private LegacyLibraryOriginalInkSelection? retainedSelection;
    private LegacyLibraryOriginalTextFieldBootstrapState state;
    public LegacyFreshRawInkCurrentView CurrentView { get; }
    public LegacyOwnedRawInkDocument Document => CurrentView.Document;
    public LegacyLibraryOriginalDomNameOwner NameOwner { get; } = new();
    public LegacyLibraryOriginalTextFieldBootstrapState State { get { lock (gate) return state; } }
    public IReadOnlyList<string> FieldIds => Array.AsReadOnly(ids.ToArray());
    private LegacyLibraryOriginalTextFieldBootstrap(LegacyFreshRawInkCurrentView view, string[] ids)
    {
        CurrentView = view; this.ids = ids;
        if (view.Curves.Count != view.Document.ObjectCount || view.Curves.Count == 0)
            throw new NotSupportedException("Complete original raw object view required.");
        for (int i = 0; i < view.Curves.Count; i++)
            if (view.Curves[i].OriginalSourceIndex != i || !ReferenceEquals(view.Curves[i].Document, view.Document))
                throw new NotSupportedException("Original raw slot identity required.");
        ranges = [new(LegacyNativeInkPosition.Pack(0, 0, 3, view.Curves[0].PointCount),
            LegacyNativeInkPosition.Pack((uint)(view.Curves.Count - 1), view.Curves[^1].PointCount - 1, 5, view.Curves[^1].PointCount))];
        state = Snapshot(false, false);
    }
    public static LegacyLibraryOriginalTextFieldBootstrap Prepare(LegacyFreshRawInkCurrentView actualRaw,
        IReadOnlyList<string> nativeFieldIds, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualRaw); ArgumentNullException.ThrowIfNull(nativeFieldIds);
        token.ThrowIfCancellationRequested();
        if (nativeFieldIds.Count is < 1 or > 16) throw new ArgumentException("One to16 fresh fields required.");
        string[] copy = nativeFieldIds.ToArray(); var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (string id in copy)
            if (id is null || id.Length is < 1 or > 64 || id.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not ('_' or '-')) || !seen.Add(id))
                throw new NotSupportedException("Unique bounded single-component literal field IDs required; no path/imported field branch.");
        return new(actualRaw, copy);
    }
    public LegacyLibraryOriginalTextFieldBootstrapState Run(CancellationToken token = default) => RunCore(-1, -1, token);
    /// <summary>Resolve this exact retained original INPUT/C22 against its raw
    /// owner. This does not construct a selected clone or recognition Seed.</summary>
    public LegacyOwnedRawInkIdentityExport ExportInputGeometry(LegacyLibraryOriginalFieldSelection actualInput,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualInput); token.ThrowIfCancellationRequested();
        lock (gate)
        {
            if (!ReferenceEquals(actualInput.Owner, this) || !selections.Any(s => ReferenceEquals(s, actualInput)))
                throw new ArgumentException("Exact original registry/C22 owner required.");
            if (!state.Succeeded) throw new InvalidOperationException("A complete bootstrap is required; failure prefixes cannot authorize recognition.");
            return CurrentView.ExportIdentityTransforms(actualInput.CopyRanges(), token);
        }
    }
    internal LegacyLibraryOriginalTextFieldBootstrapState RunPrepared(int failureAttempt = -1, int cancellationAttempt = -1,
        CancellationToken token = default) => RunCore(failureAttempt, cancellationAttempt, token);
    private LegacyLibraryOriginalTextFieldBootstrapState RunCore(int failure, int cancellation, CancellationToken token)
    {
        lock (gate)
        {
            if (failure < -1 || cancellation < -1) throw new ArgumentOutOfRangeException(nameof(failure));
            if (entered) throw new InvalidOperationException("Bootstrap retains its mutation prefix and is single-use.");
            token.ThrowIfCancellationRequested(); entered = true; failAt = failure; cancelAt = cancellation;
            try
            {
                //1243C0: disconnected page values/children do not notify BE2.
                root = New(4, "", token); page = New(3, "page", token);
                var ns = New(2, "", token); ns.NamespacePrefix = NameOwner.Intern(""); ns.NamespaceUri = NameOwner.Intern(PageNamespace);
                Append(page, ns, token);
                Attribute(page, "version", 7, I32(5), token);
                content = New(3, "content", token); layout = New(3, "layout", token);
                Append(page, content, token); Append(page, layout, token); Append(root, page, token);
                trace.Add("attach:same-CB7:original-Ink+DOM"); Observe("attached");
                foreach (string id in ids)
                {
                    Begin("field", token);
                    var field = New(3, "textField", token);
                    Attribute(field, "id", 2, Encoding.UTF8.GetBytes(id), token);
                    Append(content, field, token);
                    End("field", token);
                    //005380 / Text0416C0: input insertion precedes C51/tag.
                    Begin("addItems", token);
                    var input = New(3, "input", token); Append(field, input, token);
                    Reserve("C51-register", token);
                    //0797F0 clones/freezes the incoming range value for its
                    //history record; this is not the C51-interned C22 instance.
                    var historySelection = new LegacyLibraryOriginalInkSelection(this, ranges);
                    long actual = next; maximum = Math.Max(maximum, actual); next = checked(actual + 1);
                    //09C380 interns equal range contents; separate registry IDs
                    //retain the same C22, rather than fabricating new sources.
                    retainedSelection ??= new(this, ranges);
                    var selection = new LegacyLibraryOriginalFieldSelection(this, id, field.Id, input.Id, actual, retainedSelection);
                    selections.Add(selection);
                    history.Add(new(this, "C4D", 4, 0, 0, 0, 0, actual, 100, selection, historySelection));
                    trace.Add("register:INPUT:" + actual + ":100");
                    //065480 ->13CB50 creates a state0 event for a NEW tag.
                    //13ACB0 deletes state0 before any DOM/tag rewrite traversal.
                    sourceEvents.Add(new(selection, 0));
                    trace.Add("CB7:new-tag-state0:" + actual + ":discard-before-rewrite");
                    Attribute(input, "tag", 4, I64(actual), token);
                    End("addItems", token);
                }
                state = Snapshot(true, false); return state;
            }
            catch (BootstrapRefusal)
            { state = Snapshot(false, true); return state; }
            catch
            { state = Snapshot(false, true); throw; }
        }
    }
    public const string PageNamespace = "http://www.myscript.com/ns/myscript/page";
    private Node New(byte kind, string name, CancellationToken token)
    {
        var canonical = kind is 0 or 3 ? NameOwner.Intern(name) : null;
        Reserve("node:" + kind + ":" + name, token);
        var n = new Node(nodes.Count + 1, kind, canonical, NameOwner); nodes.Add(n); return n;
    }
    private bool Attached(Node n)
    { for (Node? p = n; p is not null; p = p.Parent) if (ReferenceEquals(p, root)) return true; return false; }
    private void Append(Node parent, Node child, CancellationToken token)
    {
        bool observed = Attached(parent);
        if (observed) Reserve("BE2-record:1", token);
        int previous = parent.Children.Count == 0 ? 0 : parent.Children[^1].Id;
        child.Parent = parent; parent.Children.Add(child); trace.Add("append:" + parent.Id + ":" + child.Id);
        if (observed) history.Add(new(this, "BE2", 1, parent.Id, child.Id, previous, 0, -1, 0, null, null));
        if (content is not null && Attached(content) && child.Name == "input") QueueInput(parent);
    }
    private void Attribute(Node parent, string name, byte tag, byte[] value, CancellationToken token)
    {
        var attr = New(0, name, token);
        //String value is assigned while the new attribute is unattached.
        if (tag == 2 && value.Length >= 16) Reserve("attribute-value", token);
        attr.Tag = tag == 2 ? value.Length == 0 ? (byte)3 : value.Length < 16 ? (byte)2 : (byte)1 : tag;
        attr.Value = value.ToArray(); bool observed = Attached(parent);
        if (observed) Reserve("BE2-record:5", token);
        attr.Parent = parent; parent.Attributes.Add(attr); trace.Add("attribute:" + parent.Id + ":" + attr.Id);
        if (observed) history.Add(new(this, "BE2", 5, parent.Id, attr.Id, 0, 0, -1, 0, null, null));
        if (parent.Name == "input") QueueInput(parent.Parent!);
    }
    private void QueueInput(Node field)
    {
        //1387A0 ->138140: same field appears once in130, not once per callback.
        if (nesting != 1 || field.Name != "textField") throw new InvalidOperationException("Fresh original transaction/field required.");
        if (pendingField is not null && !ReferenceEquals(pendingField, field))
            throw new NotSupportedException("One actual changed field per original transaction required.");
        pendingField = field; pending130 = 1; Observe("input-change");
    }
    private void Begin(string operation, CancellationToken token)
    { Reserve("history-begin:" + operation, token); if (nesting != 0) throw new InvalidOperationException(); nesting = 1; Observe("begin:" + operation); }
    private void End(string operation, CancellationToken token)
    {
        Reserve("history-end:" + operation, token);
        //13FD40 ->13BD90: actual empty BBD lookup1165C0; no116010 call.
        if (pending130 != 0)
        {
            var field = pendingField ?? throw new InvalidOperationException("Pending field identity missing.");
            string id = Encoding.UTF8.GetString(field.Attributes.Single(a => a.Name == "id").Value);
            //008D30's direct-field key is id alone. The source40 XPath built
            //by00D0C0 is a different owner/key and must not be substituted.
            if (fieldBackends.TryGetValue(id, out _))
                throw new NotSupportedException("An existing field backend requires its real update producer.");
            trace.Add("1165C0:missing-fresh-BBD"); Observe("before-end");
            retainedField = field; retainedF0 = 1; pending130 = 0; pendingField = null;
        }
        nesting = 0; Observe("merged-end:" + operation);
        //13FD40 ->136F20 drains retainedF0; registered CBB has no external listeners.
        if (retainedF0 != 0) { trace.Add("dispatch:C9D:field-input-changed"); retainedF0 = 0; retainedField = null; }
        Observe("end:" + operation);
    }
    private void Observe(string point) => observer.Add(new(point, nesting, pending130, retainedF0, pendingField?.Id ?? 0, retainedField?.Id ?? 0));
    private void Reserve(string operation, CancellationToken token)
    {
        token.ThrowIfCancellationRequested(); int ordinal = attempts++; trace.Add("try:" + operation);
        if (ordinal == cancelAt) throw new OperationCanceledException("Prepared native-service cancellation prefix.");
        if (ordinal == failAt) { trace.Add("refused:" + operation); throw new BootstrapRefusal(); }
    }
    private LegacyLibraryOriginalTextFieldBootstrapState Snapshot(bool succeeded, bool faulted) => new(this, succeeded, faulted,
        attempts, next, maximum, root?.Id ?? 0, nodes.Select(n => n.Snapshot()).ToArray(), selections.ToArray(), history.ToArray(), sourceEvents.ToArray(), observer.ToArray(), trace.ToArray());
    private static byte[] I32(int n) { byte[] b = new byte[4]; BinaryPrimitives.WriteInt32LittleEndian(b, n); return b; }
    private static byte[] I64(long n) { byte[] b = new byte[8]; BinaryPrimitives.WriteInt64LittleEndian(b, n); return b; }
    private sealed class BootstrapRefusal : Exception;
    private sealed class Node(int id, byte kind, LegacyLibraryOriginalDomName? canonicalName, LegacyLibraryOriginalDomNameOwner names)
    {
        internal readonly int Id = id; internal readonly byte Kind = kind;
        internal string Name => canonicalName?.Text ?? "";
        internal byte Tag = kind == 0 ? (byte)3 : (byte)0; internal byte[] Value = [];
        internal LegacyLibraryOriginalDomName? NamespaceUri, NamespacePrefix; internal Node? Parent;
        internal readonly List<Node> Children = [], Attributes = [];
        internal LegacyLibraryOriginalDomNode Snapshot() => new(Id, Kind, canonicalName, names, Tag, Value, NamespaceUri, NamespacePrefix, Parent?.Id ?? 0,
            Children.Select(n => n.Id).ToArray(), Attributes.Select(n => n.Id).ToArray());
    }
}

public sealed class LegacyLibraryOriginalDomNode
{
    private readonly byte[] value;
    public int Identity { get; }
    public byte Kind { get; }
    public string Name => CanonicalName?.Text ?? "";
    public LegacyLibraryOriginalDomNameOwner NameOwner { get; }
    public LegacyLibraryOriginalDomName? CanonicalName { get; }
    public byte NativeValueTag { get; }
    public string? NamespaceUri => NamespaceUriName?.Text;
    public LegacyLibraryOriginalDomName? NamespaceUriName { get; }
    public LegacyLibraryOriginalDomName? NamespacePrefixName { get; }
    public int ParentIdentity { get; }
    public IReadOnlyList<int> Children { get; }
    public IReadOnlyList<int> Attributes { get; }
    internal LegacyLibraryOriginalDomNode(int id, byte kind, LegacyLibraryOriginalDomName? name, LegacyLibraryOriginalDomNameOwner names,
        byte tag, byte[] value, LegacyLibraryOriginalDomName? uri, LegacyLibraryOriginalDomName? prefix, int parent, int[] children, int[] attributes)
    { Identity = id; Kind = kind; CanonicalName = name; NameOwner = names; NativeValueTag = tag; this.value = value.ToArray(); NamespaceUriName = uri; NamespacePrefixName = prefix; ParentIdentity = parent;
      Children = Array.AsReadOnly(children); Attributes = Array.AsReadOnly(attributes); }
    public byte[] CopyValueBytes() => value.ToArray();
}
/// <summary>Retained library BEE identity adaptation. Tokens are canonical only
/// within this original DOM owner; they are not native addresses or Seed names.</summary>
public sealed class LegacyLibraryOriginalDomNameOwner
{
    private readonly Dictionary<string, LegacyLibraryOriginalDomName> names = new(StringComparer.Ordinal);
    internal LegacyLibraryOriginalDomNameOwner() { }
    internal LegacyLibraryOriginalDomName Intern(string text)
    { if (!names.TryGetValue(text, out var name)) { name = new(this, text); names.Add(text, name); } return name; }
}
public sealed class LegacyLibraryOriginalDomName
{
    public LegacyLibraryOriginalDomNameOwner Owner { get; }
    public string Text { get; }
    internal LegacyLibraryOriginalDomName(LegacyLibraryOriginalDomNameOwner owner, string text) { Owner = owner; Text = text; }
}
public sealed class LegacyLibraryOriginalFieldSelection
{
    public LegacyLibraryOriginalTextFieldBootstrap Owner { get; }
    public LegacyLibraryOriginalInkSelection RetainedSelection { get; }
    public string FieldId { get; }
    public int FieldNodeIdentity { get; }
    public int InputNodeIdentity { get; }
    public long RegistryId { get; }
    public uint Category => 100;
    public string Name => "INPUT";
    internal LegacyLibraryOriginalFieldSelection(LegacyLibraryOriginalTextFieldBootstrap owner, string id, int field, int input, long registryId,
        LegacyLibraryOriginalInkSelection selection)
    { Owner = owner; FieldId = id; FieldNodeIdentity = field; InputNodeIdentity = input; RegistryId = registryId; RetainedSelection = selection; }
    public LegacyNativeInkPositionRange[] CopyRanges() => RetainedSelection.CopyRanges();
}
/// <summary>Retained range-content C22 identity. Equal full-domain INPUT entries
/// in this fresh owner share it, while their registry IDs and DOM nodes differ.</summary>
public sealed class LegacyLibraryOriginalInkSelection
{
    private readonly LegacyNativeInkPositionRange[] ranges;
    public LegacyLibraryOriginalTextFieldBootstrap Owner { get; }
    internal LegacyLibraryOriginalInkSelection(LegacyLibraryOriginalTextFieldBootstrap owner, LegacyNativeInkPositionRange[] ranges)
    { Owner = owner; this.ranges = ranges.ToArray(); }
    public LegacyNativeInkPositionRange[] CopyRanges() => ranges.ToArray();
}
public sealed record LegacyLibraryOriginalObserverState(string Point, int TransactionDepth, int Pending130, int RetainedF0,
    int PendingFieldIdentity, int RetainedFieldIdentity);
/// <summary>Actual newly inserted tag event; state0 is removed by13ACB0 before
/// rewrite traversal. Dirty-rectangle/style notification geometry is not copied
/// as a fabricated empty rectangle by this scoped state.</summary>
public sealed record LegacyLibraryOriginalSourceRegistryEvent(LegacyLibraryOriginalFieldSelection Selection, int NativeState);
public sealed record LegacyLibraryOriginalHistoryMutation(LegacyLibraryOriginalTextFieldBootstrap Owner, string Source, byte NativeKind,
    int ParentIdentity, int NodeIdentity, int PreviousNodeIdentity, int NextNodeIdentity, long RegistryId, uint Category,
    LegacyLibraryOriginalFieldSelection? Selection, LegacyLibraryOriginalInkSelection? CopiedSelection);
public sealed class LegacyLibraryOriginalTextFieldBootstrapState
{
    public LegacyLibraryOriginalTextFieldBootstrap Owner { get; }
    public bool Succeeded { get; }
    public bool IsFaulted { get; }
    public int Attempts { get; }
    public long NextRegistryId { get; }
    public long MaximumRegistryId { get; }
    public int RootIdentity { get; }
    public IReadOnlyList<LegacyLibraryOriginalDomNode> Nodes { get; }
    public IReadOnlyList<LegacyLibraryOriginalFieldSelection> Inputs { get; }
    public IReadOnlyList<LegacyLibraryOriginalHistoryMutation> History { get; }
    public IReadOnlyList<LegacyLibraryOriginalSourceRegistryEvent> SourceRegistryEvents { get; }
    public IReadOnlyList<LegacyLibraryOriginalObserverState> ObserverStates { get; }
    public IReadOnlyList<string> Trace { get; }
    /// <summary>Assigned C51 fields18..28 only: Next, maximum, dirty0. Pointer,
    /// allocator and hash-table capacities are not invented native bytes.</summary>
    public byte[] CopyRegistryCounterStorage()
    {
        byte[] result = new byte[17];
        BinaryPrimitives.WriteInt64LittleEndian(result, NextRegistryId);
        BinaryPrimitives.WriteInt64LittleEndian(result.AsSpan(8), MaximumRegistryId);
        return result;
    }
    internal LegacyLibraryOriginalTextFieldBootstrapState(LegacyLibraryOriginalTextFieldBootstrap owner, bool succeeded, bool faulted, int attempts,
        long next, long maximum, int root, LegacyLibraryOriginalDomNode[] nodes, LegacyLibraryOriginalFieldSelection[] inputs,
        LegacyLibraryOriginalHistoryMutation[] history, LegacyLibraryOriginalSourceRegistryEvent[] events, LegacyLibraryOriginalObserverState[] observer, string[] trace)
    { Owner = owner; Succeeded = succeeded; IsFaulted = faulted; Attempts = attempts; NextRegistryId = next; MaximumRegistryId = maximum; RootIdentity = root;
      Nodes = Array.AsReadOnly(nodes); Inputs = Array.AsReadOnly(inputs); History = Array.AsReadOnly(history); SourceRegistryEvents = Array.AsReadOnly(events); ObserverStates = Array.AsReadOnly(observer); Trace = Array.AsReadOnly(trace); }
}
