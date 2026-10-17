# Prepublication adoption of the input-imported result

`LegacyLibraryTextPublicationScope.PrepareConfigured` creates the immutable raw
document, its retained selected INPUT/C22, and a fresh
`LegacyLibraryTextProcessingContext`. It runs the proved nonempty input import
before preparing any raw grouping or neural source. The imported State18 must
be 1. The initial result identifier is zero and its strings3E pool is empty.

The scope then constructs the shared CHAR/WORD state and Seed using that exact
document, selected input and result-owned strings3E. `Seed.ProcessingResult`
and `Context.Result.PublicationSeed` identify this join. The result keeps a weak
Seed reference so an externally retained context does not keep an abandoned
publication scope alive. The scope owns the Seed strongly.

`Run` executes the existing genuine configured2 decoder, using the same loaded
model and raw document. The known secondary-count property remains explicitly
2; this is not an automatic native property-selection claim. `PrepareWords`
requires successful source completion from that exact invocation, binds its
actual TOP once, then initializes the publication UUID before the shared
document WORD/CHAR callbacks. Decoder WORD275 publication is a separate earlier
stage. There is no replacement Seed, cloned result or independently allocated
string pool after recognition. UUID generation retains the existing documented
library entropy adaptation; constructor/import UUID bytes remain zero.

The returned WordChoices retains the existing strict actual275 views and
source-clip order. Its source ranges, UTF8, full CHAR40/WORD48 and ref24 rows are
unchanged. Native input tag0 is a selection-registry ID, not a physical stroke
ordinal; the selected C22 here contains the one full raw-document range.

## Lifetime and failure boundaries

The explicit scope owns the adopted publication lifetime. Closing Context
immediately rejects new context entry, while its retained publication lease
defers request DOM, result bindings and strings destruction. WordChoices borrows
those strings and does not free them. Scope disposal closes WordChoices and
releases the final lease. Seed/WORD validators reject subsequent live child
operations. Already copied bytes, ranges, identifier bytes and DOM snapshots
remain usable. Scope finalization is a fallback; callers must dispose the scope
deterministically. The old Seed API does not acquire IDisposable semantics.

Before-entry cancellation is reusable. Foreign completion and unsuccessful
source completion are rejected before the WORD binding is consumed. A failure
after entering the adopted recognition/WORD preparation poisons that scope and
keeps its input/UUID prefix until disposal. Reentry does not replace the owners.
Downstream document or DOM operations must not race scope disposal; this batch
does not promise general thread safety for the existing document pipeline.

This route is a checked library-owned configured2 join. It does not implement
049370 resource scheduling, history/prior import, SDK-page ownership or C20
acceptance. Global multi-line owners are distinct and are not coerced into this
shared-span candidate API.

## Verification

The new probe performs three genuine configured raw inferences (including
reordered physical input), checks the actual input/result/Seed/3E identities,
and publishes both retained alternatives. It runs the committed independent
raw-AL/WORD120/CHAR40/ref24/source-range oracle, then independently reconstructs
the imported full C22 endpoints from original raw point counts. No expected text
is passed to recognition. Coverage includes context disposal before publication,
WORD disposal while strings remain borrowed, final lease destruction, immutable
copies, invalid lifetime access, foreign completion, cancellation and entered
failure prefixes. Managed and NativeAOT captures are compared in full.

`audit_text_publication_adoption.py` rechecks three native constructor/import/
pending-selection audits and separately checks the portable source ordering and
lifetime seams. Its source checks do not describe native address identity.
