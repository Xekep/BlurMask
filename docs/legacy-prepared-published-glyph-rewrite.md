# Prepared21F320 linked-WORD rewrite

`LegacyPreparedPublishedGlyphRewrite` is an **internal prepared driver** for
the enabled contextual quote body. It changes neither the fresh F4-zero owner
nor GlobalCompletion. An actual enabled configuration/controller owner remains
unported; prepared WORDs and source-kind callbacks are not a recognition result.
The native owning caller221840 invokes this body after21FBF0 and only when
knowledge99 is zero. This prepared class models21F320 itself, not that caller,
property-copy lifecycle or an enabled global-controller factory.

`LegacyPreparedContextualQuoteSource` binds a live glyph pool, explicit source
kind callback, separate live TOP2A, and actual AL/category slices from the same
resource bytes. English source247 v110 and sampler297 v108 are accepted as
checked prepared slices, with envelope/end/version/domain/fingerprint checks.
The selected sampler offset is explicit. This is **not** a v110 source ingress
reader or proof of the entire root resource ownership tree. The existing
v10d/v106 owning readers stay unchanged. Native196500's >=106 category prefix
is shared through010D80; v108 additions follow those category tables.

The five actual acoustic resources supply the raw lookup IDs. English AL217
contains opening/closing single and double quotation marks; Russian AL192 does
not. None contains U+201E low-double. Default source-bound fields are always
literal AL lookups. Separate `preparedCarryControls` explicitly supply field
24BE through an internal prepared-controller seam; those are not observed
configured sources or actual low-double AL lookups.

The ordered input is the actual linked279 WORD list, each node retaining its
275 reading vector20/count28 and the identical live glyph pool. The driver
uses the existing21F210 cursor, including exhausted/empty WORD skips and
advance-before-read. Its tiny additive descriptor-validator overload leaves
the original cursor constructor/semantics intact. Pool/list/reference/event
budgets are portable safety limits (4096 WORDs,65536 references,1048576 events).
Stored WORD count/capacity must agree with the retained reading vector; neither
fallback linked glyph counts nor unrelated WORD owners are substituted.

Three scalar/reference pairs are prefetched **before** the first write.
Cached scalars are preserved when the same reference appears again; later
reads may observe earlier pool mutations. A next scalar FFFF terminates the
window even when it came from a real reference. It must not be replaced by
an end-of-vector test. The terminal cursor event's WORD ordinal is a portable
position, not a serialized native pointer.

Native source eligibility is signed16 arc >=0 followed by frontendB8:
152040→graph68 group lookup→graph88/1A78D0 kind byte7 equal0. Even nonquote
scalars with nonnegative arcs query kind before testing the quote label.
Negative arcs skip that callback. The explicit callback receives signed24
node and arc16; ordinal bits and key bit56 do not become graph IDs.

Frontend48 is fetched once after the leading rule, before the loop/trailing
rule. It resolves151BD0→graph170/1ACF30→runtime188. Interior previous-label
classification uses data50's signed-byte composite index then data70's first
ID, compared with1. A missing negative composite would cause unsafe native
indexing; this bounded driver explicitly refuses that lookup instead.

Only the selected glyph's **ushort08** is written. Kind, publication flags,
scores, descriptor identity, references and WORD storage remain unchanged.
`SetOrdinarySymbol` would wrongly clear kind bits and is not used. The native
carry branch first writes CloseDouble, then writes OpenDouble (21F8E3→21F8FF;
trailing21FBA4→21FBBD). Both stores are retained, including a stopped prefix
between them. Interior carry clears on this flip; its LowDouble update occurs
after the next scalar read. The existing scalar rules supply final decisions,
while the driver preserves the additional store and read ordering.

Scalar139C90's alternate byte2 is ignored by21F320, including kind4 raw IDs;
this differs from21FBF0. Supported kind3 controls use the actual supplied live
TOP2A lookup, never acoustic labels or the later imported2A. Kind3 zero-count
and subkind1 do not write a scalar label; they are explicitly refused **inside
the cursor after advancement and before scalar scratch is used**. Native
direct-entry/loop/helper stack residues therefore remain an honest boundary,
with no invented zero scalar. Supported descriptors overwrite every consumed
U16 scratch value, so no opaque stack bytes are interpreted.

Every Run attempt is single-use, including pre-cancellation and failure.
Mutations and the read/query/write trace retain the successful prefix; no
rollback or accepted output token is fabricated. An optional internal observer
is a prepared diagnostic fault/cancel seam, not a native callback or success
flag. A failure after a store leaves that store observable.

The dedicated probe uses1555 prepared linked-WORD cases over the five actual
AL/category resources. Resource controls and supplied-field carry controls are
separate arrays. It checks repeated references and nonzero record ordinals,
empty WORDs, real scalar FFFF, signed source keys, nonzero kinds, kind4 raw IDs,
live-control namespace, no-write refusals, query/observer/cancel prefixes and
single-use/foreign-owner guards. The independent Python oracle decodes raw AL
and category tables, reconstructs all reading/query/write events and compares
the complete40-byte pool records. These fixtures do not measure handwriting
accuracy or prove a configured English/Russian main pipeline.

```
LegacyPreparedGlyphRewrite.Probe models capture.json
python tools/LegacyPreparedGlyphRewrite.Probe/audit_output.py models capture.json
python tools/handwriting/audit_prepared_published_glyph_rewrite.py report.json
```

The static audit reads pinned PE bytes only, including source/category virtual
bindings, both ordered stores, scalar/cursor dependencies and the shared
category-reader prefix. Vendor DLLs are never loaded or executed.
