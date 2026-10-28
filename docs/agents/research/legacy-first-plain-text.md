# First selected plain TEXT publication

`LegacyFreshFirstOwnedRecognition.RunText()` adds an owned first-pass TEXT result
without widening `Run()`'s strict single-word contract. It runs the same actual
primary producer and B0/B8 passes, then materializes the selected one-column,
fresh6C, noE0, no-traits/shadda branch of1F42D0. It accepts no caller tables,
selected labels, word boundaries or separator decisions. Its result retains the
same factory, cuts, graph, NN/cache scores and shared273 glyph pool.

For each visible selected cell, native B8+0E bit0 either continues the reverse-built
WORD276 or publishes it. Ordinary word costs are the differences between the
current word-end prefix and the predecessor prefix. WORD18 divides by the actual
source-node difference; WORD10 and1C use the corresponding total/acoustic deltas.
Root publication uses the remaining unadjusted prefixes. TOP relative mass is
never substituted for these fields. Native argument7 stays−1 and argument8 carries
the actual word-end boundary.

After an interior publication, the builder resets and the actual first1A0 callback
determines whether a separate protocol word is published. A successful FFFB result
is encoded by13AF30 as a kind1/count1 macro, rather than rewritten into a guessed
literal space. Such words retain reset scalars1/1/0/0. All words are prepended by
279 during the reverse walk, producing the actual reading order. Zero-span
trailing/leading/interior records remain pool-owned and detached; they do not
become fake published glyphs. The native E2/E4/99 outer guard still precedes both
glyph allocation and the word-boundary branch. Hidden cells remain in the source
walk and cannot force publication.

The new private-construction `LegacyFirstMaterializedPlainText` exposes real
published words, their shared pool, allocated glyph references, detached delimiter
references and the separate owner64 score map. It does not collapse multiple words
into an invented single result. The portable selected-path budget is2047 cells so
ordinary glyphs plus potential interior/end delimiters remain within the existing
4096-record pool and publisher limits.

## Checks and remaining work

Seven original coordinate inputs run actual first NN/cache/primary/B8. Four prior
single-word materializations preserve their complete WORD storage, source spans,
glyph40 records, reading references and minimum owner scalar. Three actual wider
or English-shaped paths, previously refused by the strict API, now produce two
ordinary words and one real separator word each. No expected text is passed to
recognition, and no Russian or English accuracy threshold is asserted.

The independent Python audit backtracks the captured B8 records, derives the ten
ordinary word intervals directly from visible selected boundary bits, recomputes
prefix differences and binary32 scalar bits, verifies source spans and glyph
references, and checks the three encoded protocol words and detached endpoints.
The strict API still refuses the same three multiword paths. The static auditor
pins the vendor PE and verifies native allocation guards, scalar deltas, reset,
interior request and publisher ordering without executing vendor code.

This stage materializes the selected first path only. Other terminal columns,
traits, alias/case shaping, complete main-language winner and empty-first24A
lifecycle remain separate. Source720 publication of the multiword list is the
next owned adapter; it must preserve actual glyph alternative/range owners and
skip real negative-arc delimiters using the native source consumer rules.

`LegacyFirstPlainText.Probe` takes AK, engine profile, separation profile and a
capture path. Its `audit_output.py` takes the capture. The optional static audit is
`tools/handwriting/audit_first_plain_text.py --output <report>`.
