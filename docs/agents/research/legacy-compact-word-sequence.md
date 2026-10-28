# Prepared compact symbol sequence

`LegacyPreparedCompactWordSequence` restores two stages used by `2075E0`:
`2067C0` context difference and `206960` symbol normalization. Both accept an
explicit944-byte prepared frame and return a new byte array. The source input
is unchanged; active/inactive descriptor bytes and native padding are preserved.
This is an intermediate producer for `206D30`, not the full `207CB0` controller.

## Context ownership and descriptor layout

`AppendContextDifference` receives the actual context factory, current and
previous portable context identities, complete source row88, uint source node,
knowledge symbol0E, sentinel flag24B8 and special symbol24DE. It scans the
previous chain for the first identity also present on the current chain; equal
labels under different parents do not count as a common ancestor. It appends
only the current chain's difference, in forward order. Runtime input metadata
is not guessed: source row18 differs from the full uint node and signed row5 is
negative selects the single knowledge0E descriptor instead. On that branch
context pointers are not read, matching native ordering.

The frame has a signed active count at00 and up to50 twelve-byte descriptors
from04: ushort symbol at+0, byte kind at+2, coordinate index at+4. Byte+3 and
bytes+8..11 are untouched by the extractor. Index is initialized to -1. Kind3
marks the exact native control/special cases and increments3AC with signed
wrap. The explicit24B8 branch preserves its native sentinel exclusions.

`207420` may alter the current context before this operation (associated-source
quote surgery or a knowledge0E node for source mismatch). That producer is
still outside this API. Supplying the unmodified context is not described as a
complete native result path.

## Normalization and graph references

`Normalize` removes FFFD descriptors, merges adjacent FFFC..FFF7 using the
native six-by-six signed table, canonicalizes FFF7/FFF8 to FFF9, and applies
coordinate-prefix and remaining-glyph kinds/indices. Its six-token data is
pinned to PE1802E7B20, SHA256
`ad45c41cff7e88ad53dac9d8c09ba8cd86d89eeeb18d0fa835432024e71ac9a6`.
The unsigned bounds at206A2A/206A41 reject FFFD..FFFF from table indexing.

The caller supplies genuine sixteen-byte coordinate items at260, their flag
bytes at268+16*i, and counters3A0 (unflagged prefix count),3A8 (flagged count),
3A4 (remaining glyph count). Prefix count is bounded to19 because native
capacity20 includes the final item, which this stage does not consume as a
prefix. Packed coordinate qwords are graph node/arc references, **not XY**.
The coordinate producer and counter producer have their own ownership proofs;
this operation does not infer them from a stroke bounding box.

Flagged prefix items insert kind2/symbol0 descriptors, retaining untouched
fields exactly as the native move/write sequence does. Ordinary prefix items
set kind1; remaining items and selected sentinels may become kind4. Descriptor
removal leaves inactive slots untouched after shifting. Native writes the
coordinate index at the current descriptor even if active count is zero; the
bounded port preserves this write inside the frame. Requests requiring access
past50 symbol slots refuse rather than corrupting the coordinate header.
Source context chains are explicitly bounded to50 nodes in this prepared API.

## Write audit before result publication

Native2271F0 calls207CB0 before2257C0. The owning pass/terminal bridge currently
retains pre207CB0 rows. A reviewed direct-write inventory of207CB0 finds:

- 207FA3 clears beam6.bit1;208534 sets that marker while walking predecessors.
- 20B3B3 and20B629 write beam22's compact-relative record index.

The remaining stores target the two alternative pools, compact storage2460,
node offsets2478/2488, scratch queries, local56-byte candidates or23A0 pool.
Direct helper inventory:1F7840 allocates23A0 pool records;1FB940 grows compact
storage2460/2468;1FD190 only reads beam rows;206D30 writes compact/local indices;
2075E0 and2067C0/206960 produce local symbol frames;207420 modifies a context
pointer/local context factory;207780 writes query scratch;207920/207B00 sort
local56-byte entries;25EE50 clears the supplied local/detail storage. These
helpers do not write beam5/10/14/27/30 or source-end counts.

Consequently the bounded successful fresh branch preserves numeric selection
ordinal/cost/fallback fields, but **RowField22 and complete row bytes are not
postpublication results**. This inventory does not execute or complete207CB0,
prove allocation success, or authorize arbitrary unknown callback side effects.

## Validation and remaining work

1536 independent list-based normalization cases and512 context cases compare
all944 output bytes (1,933,312 bytes total), including all36 token pairs under
both sentinel flags, signed counter wrap, empty/removal cases, padding and
inactive slots. Twelve bounds/owner/cancellation guards and an equal-label but
different-context case cover the prepared scope. The static audit pins eleven
instruction spans and the exact144-byte token table without loading vendor
code.

Actual RU networks and the owning four-node DIRECT pass yield214 nonempty
finite row contexts for extraction, ancestor difference and normalization tests.
The selected prepublication winner138 has an empty context08: extracting that
alone yields no recognized text. Tests explicitly report this; the winner's
labels cannot be inferred from a guessed context or expected string. Actual
207CB0 predecessor/sequence scheduling,207420 source-owned surgery, compact
index publication and21FFD0 reconstruction remain required before text output
or acceptance confidence is claimed.

Run `reference_controls.py OUTPUT.json`, then the probe with
`controls [AK LK engine separation]`. The static script takes an output path
and optionally `--binary PATH`; it only reads the pinned PE.
