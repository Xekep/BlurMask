using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyOriginalSelectedField.Probe")]
namespace InkRecognition;

/// <summary>Library-owned current worker C20/BCD selected from an exact
/// completed original field bootstrap. It retains the original separately;
/// the fresh bare BCC has no V200 context. No SDK CBB or history acceptance.</summary>
public sealed class LegacyLibraryOriginalSelectedFieldContext
{
    private readonly object gate = new();
    private readonly LegacyLibraryOriginalTextFieldBootstrapState originalState;
    private readonly List<LegacyLibrarySelectedFieldRegistryEntry> registry = [];
    private readonly List<Node> nodes = [];
    private readonly List<string> trace = [];
    private readonly List<byte[]> raw = [];
    private readonly Dictionary<int, Node> cloned = [];
    private LegacyLibrarySelectedFieldInkSelection? rangesOwner;
    private LegacyLibraryOriginalSelectedFieldState state;
    private bool entered;
    private int attempts, failAt = -1, cancelAt = -1;
    private long next, maximum = long.MinValue;
    private Node? root, selectedField, selectedInput;

    public LegacyLibraryOriginalTextFieldBootstrap Original { get; }
    public LegacyLibraryOriginalFieldSelection OriginalInput { get; }
    public LegacyFreshRawInkCurrentView CurrentView => Original.CurrentView;
    public LegacyOwnedRawInkDocument Document => Original.Document;
    public LegacyLibraryOriginalDomNameOwner NameOwner { get; } = new();
    public LegacyLibraryOriginalSelectedFieldState State { get { lock (gate) return state; } }
    public bool HasNativeReaderContext => false;
    public bool NativeHistoryEnabled => false;

    private LegacyLibraryOriginalSelectedFieldContext(LegacyLibraryOriginalTextFieldBootstrap owner,
        LegacyLibraryOriginalFieldSelection chosen, LegacyLibraryOriginalTextFieldBootstrapState snapshot)
    { Original = owner; OriginalInput = chosen; originalState = snapshot; state = Snapshot(false, false); }

    public static LegacyLibraryOriginalSelectedFieldContext Prepare(
        LegacyLibraryOriginalTextFieldBootstrap actualOriginal,
        LegacyLibraryOriginalFieldSelection actualChosenInput, CancellationToken token = default)
        =>PrepareCore(actualOriginal,actualChosenInput,out _,token);
    internal static LegacyLibraryOriginalSelectedFieldContext PrepareRetainingActualGeometry(
        LegacyLibraryOriginalTextFieldBootstrap actualOriginal,LegacyLibraryOriginalFieldSelection actualChosenInput,
        out LegacyOwnedRawInkIdentityExport verifiedGeometry,CancellationToken token)
        =>PrepareCore(actualOriginal,actualChosenInput,out verifiedGeometry,token);
    private static LegacyLibraryOriginalSelectedFieldContext PrepareCore(
        LegacyLibraryOriginalTextFieldBootstrap actualOriginal,LegacyLibraryOriginalFieldSelection actualChosenInput,
        out LegacyOwnedRawInkIdentityExport verifiedGeometry,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(actualOriginal); ArgumentNullException.ThrowIfNull(actualChosenInput);
        token.ThrowIfCancellationRequested(); var s = actualOriginal.State;
        if (!s.Succeeded || s.IsFaulted) throw new InvalidOperationException("Complete original bootstrap required.");
        if (!ReferenceEquals(actualChosenInput.Owner, actualOriginal) || !s.Inputs.Any(i => ReferenceEquals(i, actualChosenInput)))
            throw new ArgumentException("Exact original field/tag registry entry required.");
        if (s.Inputs.Count is < 1 or > 16 || s.Nodes.Count > 128 || actualOriginal.CurrentView.Curves.Count > 65536)
            throw new NotSupportedException("Bounded full fresh field bootstrap required.");
        var selected = actualChosenInput.CopyRanges();
        if (selected.Length != 1 || !ReferenceEquals(actualChosenInput.RetainedSelection.Owner, actualOriginal))
            throw new NotSupportedException("Same-original full C22 required.");
        foreach (var input in s.Inputs)
            if (!ReferenceEquals(input.Owner, actualOriginal) || !ReferenceEquals(input.RetainedSelection, actualChosenInput.RetainedSelection) ||
                input.Name != "INPUT" || input.Category != 100 || !input.CopyRanges().AsSpan().SequenceEqual(selected))
                throw new NotSupportedException("Only native fresh equal full-domain INPUTs without auxiliary payload are supported.");
        // Resolves actual current-view membership, never a supplied no-transform flag.
        verifiedGeometry = actualOriginal.ExportInputGeometry(actualChosenInput, token);
        return new(actualOriginal, actualChosenInput, s);
    }

    public LegacyLibraryOriginalSelectedFieldState Run(CancellationToken token = default) => RunCore(-1, -1, token);
    internal LegacyLibraryOriginalSelectedFieldState RunPrepared(int failure = -1, int cancellation = -1,
        CancellationToken token = default) => RunCore(failure, cancellation, token);

    private LegacyLibraryOriginalSelectedFieldState RunCore(int failure, int cancellation, CancellationToken token)
    {
        lock (gate)
        {
            if (failure < -1 || cancellation < -1) throw new ArgumentOutOfRangeException(nameof(failure));
            if (entered) throw new InvalidOperationException("Selected worker copy is single-use and retains failed prefixes.");
            token.ThrowIfCancellationRequested(); entered = true; failAt = failure; cancelAt = cancellation;
            try
            {
                Reserve("C20:create-history0", token);
                for (int i = 0; i < CurrentView.Curves.Count; i++)
                {
                    Reserve("raw-clone:" + i, token); raw.Add(CurrentView.Curves[i].CopyPointStorage());
                }
                //06D2C0: the query matches all equal nonempty C22s. C57's
                //signed-key AVL iterator returns IDs in increasing order.
                Reserve("query:all-mode2", token); Reserve("query:remove-empty", token);
                foreach (var source in originalState.Inputs.OrderBy(i => i.RegistryId))
                {
                    Reserve("C22:create:" + source.RegistryId, token);
                    var candidate = new LegacyLibrarySelectedFieldInkSelection(this, source.CopyRanges());
                    Reserve("C51:register:" + source.RegistryId, token);
                    rangesOwner ??= candidate; //09C380 interns equal target-owned C22s.
                    var entry = new LegacyLibrarySelectedFieldRegistryEntry(this, source, rangesOwner);
                    if (entry.RegistryId < 0 || registry.Any(i => i.RegistryId == entry.RegistryId))
                        throw new InvalidDataException("Duplicate or negative actual original registry ID.");
                    registry.Add(entry); maximum = Math.Max(maximum, entry.RegistryId);
                    if (next <= entry.RegistryId) next = checked(entry.RegistryId + 1);
                }
                Reserve("C51:copy-original-next", token);
                if (originalState.NextRegistryId <= maximum) throw new InvalidDataException("Native Next must exceed target maximum.");
                next = originalState.NextRegistryId;
                //00C590/1243C0(flag0): new PAGE, then deep clone actual chosen
                //field. No guides/activeArea/input-spans exist in this producer.
                root = New(4, "", 0, token); var page = New(3, "page", 0, token);
                var ns = New(2, "", 0, token); ns.Uri = NameOwner.Intern(LegacyLibraryOriginalTextFieldBootstrap.PageNamespace);
                ns.Prefix = NameOwner.Intern(""); Append(page, ns, token);
                var version = New(0, "version", 0, token); version.Tag = 7; version.Value = I32(5); Attribute(page, version, token);
                var content = New(3, "content", 0, token); var layout = New(3, "layout", 0, token);
                Append(page, content, token); Append(page, layout, token); Append(root, page, token);
                var byId = originalState.Nodes.ToDictionary(n => n.Identity);
                selectedField = Clone(byId[OriginalInput.FieldNodeIdentity], byId, token);
                Append(content, selectedField, token);
                selectedInput = cloned[OriginalInput.InputNodeIdentity];
                Reserve("BCD:create-selected-field", token);
                //00C80B ignores BCC.V1F8=false; do not retain a fake CBB.
                trace.Add("BCC:context-setter-false-ignored");
                state = Snapshot(true, false); return state;
            }
            catch (PreparedRefusal) { state = Snapshot(false, true); return state; }
            catch { state = Snapshot(false, true); throw; }
        }
    }

    public LegacyOwnedRawInkIdentityExport ExportInputGeometry(LegacyLibrarySelectedFieldRegistryEntry actualInput,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualInput); token.ThrowIfCancellationRequested();
        lock (gate)
        {
            if (!ReferenceEquals(actualInput.Owner, this) || !registry.Any(i => ReferenceEquals(i, actualInput)))
                throw new ArgumentException("Exact target C20/C22 registry entry required.");
            if (!state.Succeeded) throw new InvalidOperationException("Complete selected copy required.");
            return CurrentView.ExportIdentityTransforms(actualInput.CopyRanges(), token);
        }
    }
    public byte[] CopyRawPointStorage(int originalSourceIndex)
    {
        lock (gate)
        {
            if (!state.Succeeded) throw new InvalidOperationException("Complete selected copy required.");
            if ((uint)originalSourceIndex >= (uint)raw.Count) throw new ArgumentOutOfRangeException(nameof(originalSourceIndex));
            return raw[originalSourceIndex].ToArray();
        }
    }
    private Node Clone(LegacyLibraryOriginalDomNode source, Dictionary<int, LegacyLibraryOriginalDomNode> byId, CancellationToken token)
    {
        if (source.Kind is not (0 or 3)) throw new NotSupportedException("Fresh ordinary field/input attributes and elements required.");
        var copy = New(source.Kind, source.Name, source.Identity, token); cloned.Add(source.Identity, copy);
        if (source.Kind == 0)
        {
            byte[] value = source.CopyValueBytes();
            if (source.NativeValueTag == 1) Reserve("clone:heap-value", token);
            copy.Value = value; copy.Tag = source.NativeValueTag; return copy;
        }
        //0EBE40 iterates attribute38 before child20.
        foreach (int attr in source.Attributes) Attribute(copy, Clone(byId[attr], byId, token), token);
        foreach (int child in source.Children) Append(copy, Clone(byId[child], byId, token), token);
        return copy;
    }
    private Node New(byte kind, string name, int origin, CancellationToken token)
    {
        var canonical = kind is 0 or 3 ? NameOwner.Intern(name) : null;
        Reserve("node:" + kind + ":" + name, token);
        var n = new Node(nodes.Count + 1, kind, canonical, origin); nodes.Add(n); return n;
    }
    private void Append(Node parent, Node child, CancellationToken token)
    { token.ThrowIfCancellationRequested(); child.Parent = parent.Id; parent.Children.Add(child.Id); trace.Add("append:" + parent.Id + ":" + child.Id); }
    private void Attribute(Node parent, Node child, CancellationToken token)
    { token.ThrowIfCancellationRequested(); child.Parent = parent.Id; parent.Attributes.Add(child.Id); trace.Add("attribute:" + parent.Id + ":" + child.Id); }
    private void Reserve(string name, CancellationToken token)
    {
        token.ThrowIfCancellationRequested(); int ordinal = attempts++; trace.Add("try:" + name);
        if (ordinal == cancelAt) throw new OperationCanceledException("Prepared selected-copy prefix cancellation.");
        if (ordinal == failAt) { trace.Add("refused:" + name); throw new PreparedRefusal(); }
    }
    private LegacyLibraryOriginalSelectedFieldState Snapshot(bool succeeded, bool faulted) => new(this, succeeded, faulted, attempts,
        next, maximum, root?.Id ?? 0, selectedField?.Id ?? 0, selectedInput?.Id ?? 0,
        nodes.Select(n => n.Snapshot(NameOwner)).ToArray(), registry.ToArray(), raw.Select(b => b.ToArray()).ToArray(), trace.ToArray());
    private static byte[] I32(int value) { byte[] b = new byte[4]; BinaryPrimitives.WriteInt32LittleEndian(b, value); return b; }
    private sealed class PreparedRefusal : Exception;
    private sealed class Node(int id, byte kind, LegacyLibraryOriginalDomName? name, int origin)
    {
        internal int Id = id, Parent; internal byte Tag = kind == 0 ? (byte)3 : (byte)0; internal byte[] Value = [];
        internal LegacyLibraryOriginalDomName? Uri, Prefix; internal List<int> Children = [], Attributes = [];
        internal LegacyLibrarySelectedFieldDomNode Snapshot(LegacyLibraryOriginalDomNameOwner names) => new(Id, kind, name, names,
            origin, Tag, Value, Uri, Prefix, Parent, Children.ToArray(), Attributes.ToArray());
    }
}

public sealed class LegacyLibrarySelectedFieldInkSelection
{
    private readonly LegacyNativeInkPositionRange[] ranges;
    public LegacyLibraryOriginalSelectedFieldContext Owner { get; }
    internal LegacyLibrarySelectedFieldInkSelection(LegacyLibraryOriginalSelectedFieldContext owner, LegacyNativeInkPositionRange[] ranges)
    { Owner = owner; this.ranges = ranges.ToArray(); }
    public LegacyNativeInkPositionRange[] CopyRanges() => ranges.ToArray();
}
public sealed class LegacyLibrarySelectedFieldRegistryEntry
{
    public LegacyLibraryOriginalSelectedFieldContext Owner { get; }
    public LegacyLibraryOriginalFieldSelection OriginalEntry { get; }
    public LegacyLibrarySelectedFieldInkSelection RetainedSelection { get; }
    public long RegistryId => OriginalEntry.RegistryId;
    public uint Category => OriginalEntry.Category;
    public string Name => OriginalEntry.Name;
    internal LegacyLibrarySelectedFieldRegistryEntry(LegacyLibraryOriginalSelectedFieldContext owner, LegacyLibraryOriginalFieldSelection source,
        LegacyLibrarySelectedFieldInkSelection selection) { Owner = owner; OriginalEntry = source; RetainedSelection = selection; }
    public LegacyNativeInkPositionRange[] CopyRanges() => RetainedSelection.CopyRanges();
}
public sealed class LegacyLibrarySelectedFieldDomNode
{
    private readonly byte[] value;
    public int Identity { get; }
    public byte Kind { get; }
    public string Name => CanonicalName?.Text ?? "";
    public LegacyLibraryOriginalDomName? CanonicalName { get; }
    public LegacyLibraryOriginalDomNameOwner NameOwner { get; }
    public int OriginalNodeIdentity { get; }
    public byte NativeValueTag { get; }
    public string? NamespaceUri => NamespaceUriName?.Text;
    public LegacyLibraryOriginalDomName? NamespaceUriName { get; }
    public LegacyLibraryOriginalDomName? NamespacePrefixName { get; }
    public int ParentIdentity { get; }
    public IReadOnlyList<int> Children { get; }
    public IReadOnlyList<int> Attributes { get; }
    public byte[] CopyValueBytes() => value.ToArray();
    internal LegacyLibrarySelectedFieldDomNode(int id, byte kind, LegacyLibraryOriginalDomName? name, LegacyLibraryOriginalDomNameOwner names,
        int original, byte tag, byte[] bytes, LegacyLibraryOriginalDomName? uri, LegacyLibraryOriginalDomName? prefix, int parent, int[] children, int[] attributes)
    { Identity = id; Kind = kind; CanonicalName = name; NameOwner = names; OriginalNodeIdentity = original; NativeValueTag = tag;
      value = bytes.ToArray(); NamespaceUriName = uri; NamespacePrefixName = prefix; ParentIdentity = parent;
      Children = Array.AsReadOnly(children); Attributes = Array.AsReadOnly(attributes); }
}
public sealed class LegacyLibraryOriginalSelectedFieldState
{
    private readonly byte[][] raw;
    public LegacyLibraryOriginalSelectedFieldContext Owner { get; }
    public bool Succeeded { get; }
    public bool IsFaulted { get; }
    public int Attempts { get; }
    public long NextRegistryId { get; }
    public long MaximumRegistryId { get; }
    public long SourceTag30 => Owner.OriginalInput.RegistryId;
    public int RootIdentity { get; }
    public int SelectedFieldIdentity { get; }
    public int SelectedInputIdentity { get; }
    public IReadOnlyList<LegacyLibrarySelectedFieldDomNode> Nodes { get; }
    public IReadOnlyList<LegacyLibrarySelectedFieldRegistryEntry> Registry { get; }
    public IReadOnlyList<string> Trace { get; }
    public byte[][] CopyRawStorage() => raw.Select(b => b.ToArray()).ToArray();
    public byte[] CopyRegistryCounterStorage()
    { byte[] b = new byte[17]; BinaryPrimitives.WriteInt64LittleEndian(b, NextRegistryId); BinaryPrimitives.WriteInt64LittleEndian(b.AsSpan(8), MaximumRegistryId); return b; }
    internal LegacyLibraryOriginalSelectedFieldState(LegacyLibraryOriginalSelectedFieldContext owner, bool succeeded, bool faulted,
        int attempts, long next, long max, int root, int field, int input, LegacyLibrarySelectedFieldDomNode[] nodes,
        LegacyLibrarySelectedFieldRegistryEntry[] entries, byte[][] raw, string[] trace)
    { Owner = owner; Succeeded = succeeded; IsFaulted = faulted; Attempts = attempts; NextRegistryId = next; MaximumRegistryId = max;
      RootIdentity = root; SelectedFieldIdentity = field; SelectedInputIdentity = input; Nodes = Array.AsReadOnly(nodes);
      Registry = Array.AsReadOnly(entries); this.raw = raw; Trace = Array.AsReadOnly(trace); }
}
