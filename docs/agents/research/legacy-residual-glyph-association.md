# Residual source association controller

`LegacyNativeResidualGlyphAssociation` restores the217DB0 controller over
explicit72-byte glyph rows and residual packed source references. It handles
the99 bypass, ordinary horizontal/ordered matching, upper-mark correction,
type1 integer-distance matching, strict first-winner ties, cumulative row40
offsets and native insertion order. Reverse merging writes only row30/44 and
preserves cumulative offsets and unused written vector slots. A merge failure
retains earlier writes and clears the logical residual count; an unsupported
source kind fails before that final clear, as in the native controller.

The API returns private copied storage. Portable storage covers the initial
used vector plus subsequently written slots; it does not model native allocator
capacity or allocation failures. Cancellation is checked at bounded iterations.
The source kind, line, bounds, guide, alphabet flag, upper-mark predicate and
merge dispatch remain explicit prepared operations. The genuine ordinary
source adapter and1AC3C0 dynamic-group/reference overlay are still required.
This kernel is not yet an owning21FFD0 integration or a recognition result.

The headless probe checks768 prepared cases,792 merge requests,49 retained
prefix failures and115 upper-mark corrections. The independent Python audit
checks every row/vector byte, residual logical count, exact merge slices and
source callback count. Three input/cancellation guards run separately. Static
auditing pins the whole native body,14 scalar contracts and six service slots
without loading vendor code.

Probe arguments: `capture.json`; independent audit: `capture.json`.
