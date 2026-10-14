# Owned nested glyph record production

`LegacyNativeGlyphRecordPool` restores per-key descriptor insertion and the
ordinary symbol update used by first rich273 materialization. It supplies
owned node snapshots to `LegacyNativeGlyphRecordOwner`, whose13ae90 lookup
is separate. Native DLLs are not executed.

## Owners and native methods

* TOP273+48 owns a class274 pool. `156950` passes that pool and its canonical
  graph key to service08 `13ac80`; this returns a64-byte AVL node.
* Node10 contains the normalized node24/arc16/bit56 key;18 holds packed count
  and balance/duplicate flags;1a holds capacity;20 points to40-byte rows.
* `13acc0` resolves/creates the key node through1399f0, then invokes13a670.
* `13a670` deduplicates **signed descriptor-ID field00**, not symbol08. Forced
  duplicate insertion sets count word bit15; subsequent ordinary insertions
  stop deduplicating that node. Capacity grows by ten and rejects growth above
  07fe, so the maximum fresh-node capacity is2040.
* New rows preserve explicit allocated-storage opaque bytes. Field00 becomes
  the descriptor ID; flags04 lose their lowfour bits; word06 becomes0 and
  label08ffff; C=1f,10=−1f,14=0,18=0,20=−1.
* Ordinary13af30 symbol publication clears flags04 lowthree bits and writes
  raw alphabet label08. The encoded macro labelsfff9..fffc, componentcount0
  and kind4 branches require separate input policy and are not claimed here.

`ReplaceRecord` publishes complete actual classifier/candidate/trait fields
from their independent producers while preserving descriptor identity. It
does not compute label likelihood, cost or geometry from TOP273 mass.

## Portable storage and bounded scope

This implementation uses a managed dictionary for key ownership. It does not
reproduce native AVL rotations, pointer addresses or lowtwo balance bits.
Its lookup snapshots use portablezero lowtwo bits; native count extraction
discards them. Count and persistent duplicateflag semantics remain exact.
Maximum owners4096 and total records65536 are portable storage budgets.
Budget/cancellation rejection occurs before mutation, unlike unmodeled native
allocation-failure side effects.

The actual274 geometry-capture flag must be supplied explicitly.
`CreateWithoutGeometryCapture(true)` rejects: geometry280 allocation/payload
production is not guessed. Neither presence of a Russian alphabet nor first
model output proves that flagfalse. Fresh config ownership remains a separate
binding requirement. Missing owners/records are unavailable and do not become
empty fake glyphs.

## Verification

```powershell
python tools/LegacyGlyphRecordPool.Probe/reference_controls.py D:/glyph-pool-controls.json
dotnet run --project tools/LegacyGlyphRecordPool.Probe -c Release -- D:/glyph-pool-controls.json
```

The independent Python operation oracle covers256 pools and32768 insertions/
symbol writes with signed descriptor IDs, equal symbols, forced duplicates,
multiple bit56 owners, random opaque bytes and complete snapshot comparisons.
Produced nodes are passed through the existing13ae90 resolver. Native capacity
2040 and the next-growth rejection, explicit unsupported capture, malformed
references, caller score-channel ownership and cancellation are checked.
These tests establish storage/producer behavior, not raw Russian recognition
or a completed rich273 materializer.
