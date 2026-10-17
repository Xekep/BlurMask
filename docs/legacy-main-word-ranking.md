# Prepared WORD ranking

`LegacyPreparedMainWordRanking` restores the numeric ranking portion of Text
`13D360`. It accepts complete 120-byte WORD payloads, the selected WORD indices
in actual279 traversal order, the carried quota and a same-pool276.V60 comparer.
It returns payloads, old-to-new indices, released indices and all 20-byte ranking
rows. The class is internal; it does not declare a TOP source pass complete.
Separate payload source ordinals follow representative swaps. An owning bridge
can retain the actual winning WORD's flat-reference vector and release the
correct displaced payload without reconstructing references from byte contents.

The implementation preserves duplicate grouping by WORD field3C/comparison/
special category, strict greater-score representative replacement, complete
payload swaps, selected first-word priority and the native unstable midpoint
sort for equal scores. Byte+D remains the original selection marker even when
duplicate selection is merged into the representative's protection bit.

The quota increment counts ranking byte+F (WORD flags300), including only rows
for which that field was initialized. It does **not** count selected byte+D.
Quota state carries across span owners. Special-mode ordering leaves the first
word fixed, stably moves special words behind the remaining ordinary words and
sets only their output score16 to negative MaxValue.

The prepared boundary requires nonempty selected spans, at most4095 words,
finite incoming scores other than negative MaxValue, and complete payloads.
It refuses unsupported input rather than inventing native allocation residue.
The generic comparison delegate in the numeric probe uses explicit integer
identities; it is not an implementation of the native linked-glyph comparator.
Binding actual275 span mutation,276.V60, source augmentation and failure-prefix
ownership is still required before `TOP.V68` can be considered implemented.

Validation covers2048 deterministic numeric cases, all120 payload bytes and
all20 ranking bytes. An independent byte-addressed Python oracle checks
duplicate groups, representative swaps, quota, pruning, equal-score ordering,
special ordering, releases and selected-index remapping. A static audit pins
four full PE regions, including the category15 count and the three sort calls.
A separate512-word control checks130816 comparisons and caps managed allocation
at2MB. The comparer receives read-only spans over one owned input snapshot;
it does not copy two120-byte records for every pair. Cancellation is checked
inside the pair loop as well as at each representative.
