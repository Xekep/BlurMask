# Ordinary main graph append stage

This batch implements the bounded value/list part of19D470,19C150 and19EB10.
`LegacyMainGraphAppendState` owns ordinary group metadata, physical ranges,
range-history vectors, auxiliary contexts, arc fields, root and ordered
connections. Its construction/append kernel is internal; the public surface
exposes immutable snapshots. It has no classifier-facing `Graph` projection.

`LegacyFreshMainSingleAtomDescriptorAppend.Prepare(resource, actualCutSource)`
is the actual-source adapter. It accepts an entire
[owned descriptor cut source](legacy-main-descriptor-cut-source.md) only when
every ordinary descriptor has exactly one atom covering one complete physical
stroke. It preserves that source, descriptor/guide IDs, global physical ranges,
shared XY/chronology lineage and pending kind3 metadata. It refuses a larger
source rather than dropping its unsupported descriptors.

The [global join plan](legacy-main-global-graph-join-plan.md) describes the
remaining cross-source1A3B30 construction,19E8B0 boundary pruning and downstream
context ownership. Even the actual adapter's two-descriptor examples are not
finished native global graphs: the cross-merge cap would admit1+1 nodes, and
that subsequent stage has not run.

## Append semantics

Local ordinary groups whose native byte6 is nonzero receive new global group
IDs in allocation order. Zero-reference groups map to−1 in the portable remap
snapshot and are not copied. Every copied arc must own a live input group;
otherwise the kernel refuses the invalid source instead of reading an
uninitialized native remap entry.

The source descriptor ordinals and physical/local point ranges remain unchanged.
Endpoint context IDs receive `((oldGlobalNodeCount & 0xFFFFFF) << 7)` with native
uint32 wrapping. Non-FFFF group/arc auxiliary indices receive the old global
context count. All auxiliary records, including currently unused records, are
appended in order. Group bbox, placement, arc flags, span, extra1A and raw scalar
1C bits are retained separately. The root's field00 is an opaque native scalar,
not a fabricated ordinary ink-group ID.

Ordinary range vectors are deep-copied. Initial group copying clears the history
vector. History is then copied from the original local group only when a copied
arc visits the native boundary lookahead window or the local final node and the
destination history vector is still empty; its byte8 becomes1. This includes
the native empty-history case. It is not an unconditional copy of every local
history vector.

For each new source-root arc in ascending node/ordinal order, a sole−1
predecessor is replaced by all old shared final-node arc ordinals, in ascending
order. Reciprocal successor references are appended to those old arcs at the
corresponding selector. The first source copies its actual root fields/vectors.
An empty source graph performs no append. The bounded ordinary scope requires
the sole-root predecessor form; token/multiple-root representations are outside
this kernel.

The kernel retains duplicate ordinary list entries and their order. Reciprocal
edge multiplicities are validated without sorting the stored vectors. Root
products are built once per old terminal arc, preserving native scan order
without repeated growing-array copies.

## Sticky reference byte200

Native1A37B0 increments a group reference count only while below200. Native
1A5B00 decrements it only while below200. Therefore reaching200 is permanent for
the allocation/removal phase represented here. The internal counter helper
restores allocation-then-pruning tracking; prepared cases cover199,200,201,
all-removed sticky groups and dead nonsticky groups. A sticky group is retained
even when no remaining arc references it. No live count is inferred solely from
the retained arcs.

## Actual one-atom ownership

For an ordinary descriptor with one whole-physical atom, local expansion has
one nondeferred node. There is no local merge or local pruning iteration.
The existing native rules give:

- actual descriptor U16+0, secondary descriptorFFFF, bbox from selected points;
- one source range and one range-history entry, ownership byte8=1;
- group kind0, live counter1 and fresh ordinary source flags50/51/52=0;
-19C280 root zeroed fields, flag4, auxiliaryFFFF and the real root successor;
-1A1850 span/placement0, flag4, group+4 copied into arc+18;
-1A1EF0's single-arc start0/end3 endpoint contexts.

These zero values follow the fresh source/constructor producers. They are not
caller-supplied success flags or default IDs for a general local graph. A real
pending kind3 event produces its own19EB10 record: flag0=1, zero other fresh
prefix scalars and oneFFFFFFF9 control per pending descriptor. The auxiliary
ordinal comes from allocation count. Source descriptor2 therefore retains its
real gap from separator descriptor1; it is not rebased to descriptor1.

`ResolveGroupSource` resolves the appended group's retained descriptor to the
same actual cut step, which supplies its real guide and physical ownership.
No first6120 frame is substituted for the main source.

19EB10 contexts are represented by a copied16-byte pointer-free prefix and an
ordered control array. If all four gate flags are zero, it returnsFFFF and
allocates nothing, even when later prefix bytes or controls are nonzero. For an
allocated context it zeroes the record, copies only flags0..5 and scalar8..B,
and copies the control vector. Raw pointers, allocator capacity/residue and
uninterpreted96-byte pointer layouts are not exported as reconstructed bytes.

## Bounds and checks

Both source states must use the same selected span capacity1..15. That is the
actual main8 case; general heterogeneous-capacity append is not inferred. The
portable limits are4096 nodes,131072 arcs/groups,65535 auxiliary records and
1,048,576 connection, range-history and control entries. Combined counts and
the root cross-product size are checked before copying or allocating the
product. In particular, two individually valid1000-root/terminal states are
refused before a two-million-entry join allocation. Cancellation is checked
through validation and append loops.

`LegacyMainGraphAppend.Probe` covers288 prepared appends,18 counter cases,
32 pending-prefix controls, nonzero group/context bases, dead groups, duplicate
lists, range-history windows, raw scalar bits and endpoint uint wrapping.
Three actual raw two-stroke inputs pass through real first TEXT,720,24B,24D,
29B,265 and descriptor cuts before this adapter. They retain descriptor IDs0/2,
line IDs0/1, physical IDs0/1, allocated pending context0 and root stitching.
Thirteen guards cover ownership, malformed topology/live counters, cancellation,
unsupported real multi-atom input and pre-allocation root-product bounds.

The independent Python oracle checks312 total append snapshots,2660 mapped
groups,4595 copied arcs,579 auxiliary records,492 uint32 endpoint wraps and nine
sticky counter states. It computes expected maps, histories and ordered root
edge products from the captured input states; it does not call the C# kernel.
These are numerical/ownership controls, not recognition accuracy measurements.

The static auditor pins60 instructions in13 exact PE ranges, including live
group mapping, context/endpoint offsets, root removal,19EB10's gate/prefix copy,
seed/root scalar initialization and sticky-counter branches. It reads the
pinned Text PE only; no vendor DLL is loaded or executed. Managed and Native AOT
captures are required to match byte for byte.

The probe accepts eight positional arguments:

```text
AK engine-profile separation-profile case-data publication-data line30 line50 capture.json
```

```powershell
python tools/LegacyMainGraphAppend.Probe/audit_output.py <capture.json>
python tools/handwriting/audit_main_graph_append.py --output <static.json>
```

No existing runtime API or single-line golden is changed. Multi-atom local
allocation metadata, cross-source candidate interning, boundary pruning,
global source-range DFS and per-group main NN context remain separate work.
