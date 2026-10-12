# Native default configuration and alternative pools

This batch adds the literal fresh `20baa0` scalar defaults, a checked prepared
controller scalar reader, owned 40-byte alternative pools, nontracing `1f93a0`
and `1f9830` merges, and their `2144f0` ordering/rebuild/materializer bridge.
The previous one-alternative ordering API remains unchanged and diagnostic;
default configuration is not silently reduced to one alternative.

## Constructor and prepared configuration

`20baa0` writes dword `2400 = 00020002` and ushort `2404 = 2`. Therefore header
count `2400`, primary alternatives `2402` and secondary alternatives `2404`
each start at two. Other relevant literal stores are `230a=0`, float `230c=.25`,
`22f6=0`, dword `22e8=0`, trace byte `2368=0`, suppression byte `2609=0` and
dword override `2458=ffffffff`. `CreateFresh20baa0` exposes precisely this scalar
subset. It does not construct the native object, pointers or prepared input.

`ReadPreparedSnapshot` reads these exact fields and widths from an explicit
little-endian controller snapshot covering through `2609`. It does not replace
prepared values with fresh defaults or infer them from a recognition resource.
The row arc limit, source flags and later preparation/configuration remain
actual caller inputs. Pointer fields are never dereferenced or manufactured.

## Pool layout and ownership

Each owned pool row contains `alternativeCount * 28h` bytes. A heap slot's
signed dword `58` selects that row. Primary pool `2428` and secondary pool
`2430` are separate storage. Opaque U64 at alternative `08` is the identity key
used for deduplication; it is compared as bits and never dereferenced.

The implemented selection vector supports counts 1..16, including native fresh
two. This is a checked subset within the native stack selection storage, not a
claim about the SDK's configurable maximum. Trace `2368` and its parallel
640-byte blocks remain refused. Pool creation/scoring upstream of these merges
is explicit; the buffer does not fabricate alternatives from a word or confidence.
Selection and field copying use stack storage without per-candidate GC objects.

## Primary `1f93a0`

The first alternative is selected from the two row heads. Normally their signed
costs at `00` decide. If both heads have byte `1f` bit5 (`20h`), the two heap-slot
costs at `10` decide instead. A tie chooses the duplicate row's first entry.
Remaining cursor heads use raw alternative costs; equal costs choose the
duplicate cursor. `4000000h` and higher mark unavailable heads. Selected U64
identities are unique; duplicates advance the cursor without filling a slot.

Selection references are then copied backwards into the leader's original
pool row. Only `00..1b`, byte `1e`, and byte `1f` low six bits copy. Destination
bytes `1c..1d`, `1f` high two bits and `20..27` remain owned by the destination.
Both heap slots remain unchanged. Unfilled pool tail entries remain unchanged.

Reads occur from the live pool in that descending copy order. They are not
replaced by snapshots of all chosen source records. With repeated identities
inside an input row, a preceding destination write can overwrite a later
source entry. The code preserves literal native behavior and imposes no
unproved pool-uniqueness invariant. The heap-cost seed rule can also leave raw
costs nonmonotonic after prior merges; global numeric sorting of all pool entries
is therefore not substituted for cursor-head selection.

## Secondary `1f9830`

A strictly lower duplicate head cost first swaps the complete 128-byte heap
slots, including ordinal `58`, opaque tail and lineage. Equal head costs retain
the leader. The leader head seeds selection; remaining cursor ties prefer the
duplicate cursor. Selected U64 identities use the same bounded cursor merge.

Reverse copying writes `00..1b`, byte `1e`, byte `1f` low five bits, and dword
`20`. Destination `1f` bit5 is preserved here, unlike primary copying. High two
bits of `1f`, bytes `1c..1d` and `24..27` also remain untouched.

The merged best alternative updates the leader heap slot:

| Heap field | Alternative source |
| --- | --- |
| I32 `10`, I32 `14` | I32 `00`, I32 `04` |
| byte `26` | byte `1e` |
| U16 `1e` | U16 `18` |
| byte `20` | byte `1a` |
| byte `21` | alternative `1f` low5 plus inherited heap high2; heap bit5 clears |
| U16 `3e` | U16 `20` |

Trace pointer replacement is not performed in nontracing scope.

## Ordering and row consumer

`LegacyNativeWordAlternativeOrdering` selects the literal comparator from the
prepared configuration and explicit frontend280 result. It applies native
midpoint quicksort, then calls secondary merging first (or the ordinary full
record cost swap when secondary count is not greater than one), followed by
primary merging only in the initial phase. The continuation duplicate block
(`2144f0` after `211eb0`, calls at `1f9830`) has no `1f93a0` call: it leaves
primary pool bytes unchanged and accepts an absent primary pool while retaining
the actual prepared primary count2. An explicitly supplied primary pool remains
an owned unchanged snapshot. Later comparisons see updated leader fields, as native.
Distinct records compact forward and rebuild through `203de0`/`1f75b0`.

The returned complete pool snapshots remain indexed by the transported `58`
ordinals and feed the existing `213410` materializer. Primary blocks obey its
primary arc limit; secondary blocks copy under their own enabled count. No
pool is replaced by a text-string dictionary, generic score ranking or a
one-alternative production default. Cancellation can leave prior pool merges
applied; transaction rollback is not claimed.

The focused phase regression uses two duplicate slots whose primary alternatives
would visibly change under `1f93a0`. It proves unchanged primary bytes in
continuation mode, optional absence of that unused pool, and that the same
fixture changes under the initial merge. The independent 128 continuation
ordering streams also expect the complete original primary bytes.

## Validation

Managed and Windows Native AOT passed 768 independent complete primary/secondary
merge controls and 128 prepared scalar views. An independent tuple/standard-library
head-merge oracle checks all pool bytes, both complete heap slots, identity ties,
bit5 differences, live reverse-write behavior and sentinel heads. A further 256
whole default2/2 sort/merge/pool/rebuild streams check 1,152 materialized records
with their exact primary and secondary blocks, including changed leader keys
across repeated merges. The generator reuses only the frozen independent native
comparison/permutation/heap oracle definitions, without executing its top-level
generator or vendor code.

Actual RU source/geometry/context/neural/cache scoring supplies eight retained
continuation records. Sixteen explicitly replayed paths with diagnostic +1
duplicates and caller-created alternative records run with constructor counts
two/two read through the prepared snapshot. They produce eight native key groups,
then three explicitly quota-selected stored rows, each carrying 80 primary and
80 secondary bytes, and feed the next continuation's exact original root.
The preparatory fixture still uses the released one-alternative lifecycle to
obtain those inputs; the new merge stage does not claim complete native initial
alternative construction or language search scheduling.

Opaque identity initialization, diagnostic prior, prepared primary arc limit and
quota three are explicit. Global `212ec0` quotas, upstream alternative creation,
trace transport, native search/winner and confidence remain unfinished.

```powershell
python tools/LegacyWordAlternatives.Probe/reference_controls.py .artifacts/legacy-word-alternative-controls.json
dotnet run --project tools/LegacyWordAlternatives.Probe -c Release -- .artifacts/legacy-word-alternative-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```
