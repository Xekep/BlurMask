# Fresh first-pass A8 shortlist

`LegacyFirstAcousticShortlistTable` restores the alias-fold-disabled A8 producer
inside Text1ef070 after primary98 records. It is one of the three subsequent
tables, not final Russian recognition or the B0/B8 symbolic-context passes.

The outer loop visits finite primary40-byte records in stored order, stopping
at the first cost≥67108864. Only the first occurrence of each ArcOrdinal10
creates an A8 group. Its Total0 and Acoustic4 are copied from that primary
record. Its labels come from frontend160's native top20 ranked group; fallback
e2 and closed-shape e4 are skipped. SymbolC, Arc10 and truncated float
likelihood×255 clipped to1..255 are written. Fresh child249 binder156630 writes
the frontend configuration+11=0. First iteration1ef070 reloads aliasfold88
from that same owner+11, preserving the constructor's initial zero. Therefore
no alphabet case/equivalence rewrite is applied in this branch. Constructor
state alone would not prove the value after that first-iteration reload.

Only bytes0..8, C..D and10..11 are defined by this producer. The remaining
40-byte cell contents retain their previous bytes. In particular A8 byte9/A,
packed category/span E and symbolic identity18 are not primary-row metadata
and must not be reconstructed by copying the whole primary record. Unused
cells have only their Total0 overwritten with the impossible-cost sentinel.

F8 is a **global fixed beam width**, not a per-node active group count. Every
node's A8 storage is F8×A0×40 bytes; the number of distinct finite groups varies.
Cell order is `A0 * distinctArcGroup + withinAlternative`. Native1f2560's global
A8 cell ID is `(A2+A4)*F8 + FC + A0*group + alternative`; the later B0/B8
tables and terminal selector own their separate offsets and consumers.

The API snapshots the supplied primary records before invoking a bounded
top20 callback, copies initial output storage, preserves cancellation and
rejects nonfinite or duplicate normalized shortlist entries. A zero output
capacity and enabled alias-fold branch are outside this scoped API. It accepts
an explicit A0 cap; the actual fresh child249 diagnostic uses A0=1 and F8=5.

Verification:512 vendor-free independent Python fixtures exercise10301 fixed
cells, first-arc ownership, repeated primary arcs, impossible early-stop,
special-label exclusion, top20 bounds, likelihood clipping and all opaque
bytes. Managed and Native AOT agree. The actual committed RU resources are
also exercised from original printed MIR strokes through descriptor0/line0,
24a/265/297,35 first-NN/final195 groups, complete nonroot primary recurrence
with frontend118/140 corrections, and19 active A8 groups. The underlying
primary diagnostic still begins with mixed-script `MИP`; this stage does not
claim a final winner or cursive accuracy.

```powershell
python tools/LegacyFirstShortlist.Probe/reference_controls.py D:/controls.json
dotnet run --project tools/LegacyFirstShortlist.Probe -c Release -- D:/controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```

No vendor DLL is loaded or executed. B0/B8 still require exact symbolic-context
identity, predecessor traversal and per-kind geometry/eligibility proofs.
