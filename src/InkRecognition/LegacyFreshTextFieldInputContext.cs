using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyTextFieldInputContext.Probe")]

namespace InkRecognition;

/// <summary>A new library-owned textField INPUT context. Its C51 registry is
/// separate from raw-object IDs and from the existing document selection policy.
/// This is not an imported SDK page, layout, history, or prior result.</summary>
public sealed class LegacyFreshTextFieldInputContext
{
    private readonly LegacyNativeInkPositionRange[] fullSelection;
    private readonly byte[][] rawPoints;
    private readonly long next;
    private readonly long maximum;
    public LegacyFreshRawInkCurrentView CurrentView { get; }
    public LegacyOwnedRawInkDocument Document => CurrentView.Document;
    public LegacyFreshTextFieldInputContext? Parent { get; }
    public LegacyRetainedTextFieldInputSelection Input { get; }
    public long NextRegistryId => next;
    public long MaximumRegistryId => maximum;
    public int RawObjectCount => rawPoints.Length;
    public string ElementNamespace => "http://www.myscript.com/ns/myscript/page";
    public string ElementName => "textField";
    public int NativeElementKind => 0;

    private LegacyFreshTextFieldInputContext(LegacyFreshRawInkCurrentView current,
        LegacyFreshTextFieldInputContext? parent, CancellationToken token)
    {
        CurrentView = current; Parent = parent;
        // Fresh C20 creates its own C51:09E4C0, Next=0, Max=LONG_MIN.
        // Default C4D/C5E raw-append observers do not register selections.
        long registryNext = 0, registryMaximum = long.MinValue;
        rawPoints = new byte[current.Curves.Count][];
        for (int i = 0; i < rawPoints.Length; i++)
        {
            token.ThrowIfCancellationRequested();
            rawPoints[i] = parent is null ? current.Curves[i].CopyPointStorage() : parent.rawPoints[i].ToArray();
        }
        fullSelection = parent is null ? CreateFullSelection(current) : parent.fullSelection.ToArray();

        // Text0416C0 addItems: new input has no span children; C98 kind0
        // supplies category0+100(decimal). C51's actual returned ID is the tag.
        // The selected-copy branch instead preserves the explicit source ID.
        long requested = parent?.Input.RegistryId ?? -1;
        long actual = requested == -1 ? registryNext : requested;
        if (actual < 0) throw new InvalidOperationException("Invalid derived INPUT registry ID.");
        registryMaximum = Math.Max(registryMaximum, actual);
        if (requested == -1 || registryNext <= requested) registryNext = checked(actual + 1);
        Input = new(this, actual, parent?.Input.Category ?? checked((uint)(NativeElementKind + 100)), fullSelection);
        if (parent is not null)
        {
            //06D2C0 rebinds retained C22 to the child C20, registers with the
            // exact original ID, verifies it, then09ED40 copies parent's Next.
            if (Input.RegistryId != parent.Input.RegistryId || registryMaximum >= parent.next)
                throw new InvalidOperationException("Selected registry rebind violated native ID/Next ordering.");
            registryNext = parent.next;
        }
        next = registryNext; maximum = registryMaximum;
    }

    public static LegacyFreshTextFieldInputContext Prepare(LegacyFreshRawInkCurrentView actualCurrentView,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualCurrentView); token.ThrowIfCancellationRequested();
        return new(actualCurrentView, null, token);
    }

    /// <summary>C20.V170's bounded full-selection copy. Raw original IDs remain
    /// unchanged, while retained C22 and registry ownership become this child's.
    /// Cropped selections, auxiliary payloads and SDK page observers are outside
    /// this entrypoint.</summary>
    public LegacyFreshTextFieldInputContext CreateSelectedCopy(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested(); return new(CurrentView, this, token);
    }

    public byte[] CopyRawPointStorage(int originalSourceIndex)
    {
        if ((uint)originalSourceIndex >= (uint)rawPoints.Length) throw new ArgumentOutOfRangeException(nameof(originalSourceIndex));
        return rawPoints[originalSourceIndex].ToArray();
    }

    /// <summary>C51 assigned bytes18..28: Next, maximum live ID, dirty flag.
    /// Pointer lanes, capacity and allocator bytes are deliberately excluded.</summary>
    public byte[] CopyRegistryCounterStorage()
    {
        byte[] storage = new byte[17];
        BinaryPrimitives.WriteInt64LittleEndian(storage, next);
        BinaryPrimitives.WriteInt64LittleEndian(storage.AsSpan(8), maximum);
        return storage;
    }

    /// <summary>The source30 tag -> C20.V128 retained C22 join. Only a
    /// selection owned by this exact C20 context can supply its ranges.</summary>
    public LegacyOwnedTextFieldInputIdentityExport ExportInputIdentityTransforms(LegacyRetainedTextFieldInputSelection actualInput,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualInput); token.ThrowIfCancellationRequested();
        if (!ReferenceEquals(Input, actualInput) || !ReferenceEquals(actualInput.Owner, this))
            throw new ArgumentException("INPUT tag and retained C22 must belong to this exact context.");
        return new(this, actualInput, CurrentView.ExportIdentityTransforms(actualInput.CopyRanges(), token));
    }

    private static LegacyNativeInkPositionRange[] CreateFullSelection(LegacyFreshRawInkCurrentView current)
    {
        int last = current.Curves.Count - 1;
        if (last < 0) throw new ArgumentException("A nonempty fresh ordinary raw owner is required.");
        return [new(LegacyNativeInkPosition.Pack(0, 0, 3, current.Curves[0].PointCount),
            LegacyNativeInkPosition.Pack((uint)last, current.Curves[last].PointCount - 1, 5, current.Curves[last].PointCount))];
    }
}

/// <summary>Actual fresh addItems INPUT tag and retained C22. Private creation
/// preserves registry/C20 identity; no caller-selected ID or empty-span flag.</summary>
public sealed class LegacyRetainedTextFieldInputSelection
{
    private readonly LegacyNativeInkPositionRange[] ranges;
    private readonly byte[][] spanChildren;
    public LegacyFreshTextFieldInputContext Owner { get; }
    public long RegistryId { get; }
    public string Name => "INPUT";
    public uint Category { get; }
    public int InputSpanCount => spanChildren.Length;
    internal LegacyRetainedTextFieldInputSelection(LegacyFreshTextFieldInputContext owner,
        long registryId, uint category, LegacyNativeInkPositionRange[] actualRanges)
    { Owner = owner; RegistryId = registryId; Category = category; ranges = actualRanges.ToArray();
      // The only creation path is addItems on a new input, not setInputSpans.
      spanChildren = []; }
    public LegacyNativeInkPositionRange[] CopyRanges() => ranges.ToArray();
    public byte[] CopyRangeStorage()
    {
        byte[] bytes = new byte[checked(ranges.Length * 16)];
        for (int i = 0; i < ranges.Length; i++)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(i * 16), ranges[i].First.Value);
            BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(i * 16 + 8), ranges[i].Last.Value);
        }
        return bytes;
    }
    /// <summary>Document030FB0 visits actual INPUT span children. The new
    /// addItems path creates the input/tag and no span child; no DAF row is
    /// appended. setInputSpans/imported DOM nodes are not accepted here.</summary>
    public byte[] CopyDafInputRows32()
    {
        if (spanChildren.Length != 0) throw new InvalidOperationException("Unsupported imported INPUT span children.");
        return [];
    }
}

/// <summary>The actual INPUT registry/C22 owner plus its checked ordinary
/// geometry export. This is not a result2DA transform-vector write.</summary>
public sealed class LegacyOwnedTextFieldInputIdentityExport
{
    public LegacyFreshTextFieldInputContext Owner { get; }
    public LegacyRetainedTextFieldInputSelection Input { get; }
    public LegacyOwnedRawInkIdentityExport Geometry { get; }
    internal LegacyOwnedTextFieldInputIdentityExport(LegacyFreshTextFieldInputContext owner,
        LegacyRetainedTextFieldInputSelection input, LegacyOwnedRawInkIdentityExport geometry)
    { Owner = owner; Input = input; Geometry = geometry; }
}
