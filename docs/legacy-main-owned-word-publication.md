# Owned plain word publication callback

`LegacyOwnedMainPlainWordPublication` executes the bounded ordinary `21FE30`
callback over a real `LegacyOwnedMainGlyphFlushSession`. It retains the actual
CASE source policy, selected compact result, glyph pool, geometry, source symbol
callbacks and residual overlay. It accepts the session's current association,
not caller-selected labels, boundaries or a supplied success flag.

## Native order and return value

The session resolves packet provenance and applies the actual `21FE30` WORD
field writes before residual association. The publication callback then runs
the existing `218560` chain executor over those associated rows. It queries the
working WORD's symbol span even when the chain reports failure. A positive span
requires both successful chain construction and a nonnull `279` publication.
A nonpositive span suppresses allocation and preserves the chain's success
flag; it does **not** turn a failed chain into success.

All normal callback exits reset the working WORD through its plain virtual0
operation. The glyph pool and already published words survive that reset.
The publisher retains native linked-head identity and prepends each published
word, with the packet's first/last boundary overrides. Positive-span publication
is distinct from a successful callback that emits no word.

Exceptions and cancellation follow the portable session's failure contract:
publication is single-use once claimed, an exception during mutation stops
that session, and committed packet/overlay/pool prefix effects are preserved.
Public progress accepts only the typed callback result for the exact current
association. A duplicate publication cannot provide an invented success result.

## Actual TOP values and disabled branches

The bounded fresh source has no associated source2300. Native `2271F0` therefore
writes the integer value2 to TOP.AC. This is a raw integer field, not float2.
Before the result callback pass, the first `221840` invocation has no publisher,
so it traverses the raw, unassociated native flush segments for `2188F0` width
statistics. Each segment is visited in reverse row order. Width subtraction is
float, the accumulator is double, and a positive signed count produces one
double division converted to the TOP.B0 single. Nonpositive count writes zero.
The owning callback prepares these values from the actual flush plan and source
geometry; opaque WORD allocator seed bytes are not statistics inputs.

`2271F0` clears the low callback duplicate byte for this first-result route.
`21FE30` transports that low byte to `218560`; upper opaque callback bytes are
not interpreted as a boolean. The separate chain executor still maintains its
real internal duplicate-carry state. Conversion and option arguments are zero
for this owning route.

Native trace2368 selects packet+40 detailed data instead of null. This callback
does not implement that branch. An additional local trace check is unnecessary:
the private `LegacyFreshMainPlainResultOwnership` constructor is reachable only
through `Create`, which rejects nonzero Trace2368, mode, knowledge22F8 or
source-format22F9 before creating the WORD pool/publisher. The session retains
that exact owner. Fresh geometry capture, ownerC4 and associated-source policies
are likewise explicit bounded owner properties.

## Verification

`LegacyMainWordPublication.Probe` uses actual CASE compact rows and the real
backtrack/flush/association/chain path. Ten actual inputs (one four-node fixture
and nine recorded coordinate tracks), each tested with32 explicit initial WORD
and record/stack residues, produce640 callback results and320 published words
per runtime. These are320 published records, not320 distinct transcriptions.
Zero-span protocol glyphs are retained in the pool but do not create an empty
word. Each input has133 controls:128 single-use/stale acknowledgements plus5
storage/cancellation/foreign-owner preconditions. The1,330 total controls preserve
source identity and chronology. Managed and Windows Native AOT captures match
byte for byte.

The independent output oracle checks all packet-field writes, normal resets,
complete120-byte WORD records, pool chains, flat reference order, native span
overrides, count/capacity and zero-span suppression:192,000 WORD bytes. Actual
glyph records are inputs from separately checked producers; expected text is
never used to steer recognition. These checks are not handwriting accuracy or
accepted-language-confidence measurements.
The recorded dataset omits pen-up metadata. Supplying one unsegmented path per
track is an explicit diagnostic assumption, not recovered stroke segmentation.

`audit_main_word_publication.py` independently reads the pinned PE and verifies
three native ranges and48 instruction pins: the complete `21FE30` callback,
first `221840` setup/mean, raw `2188F0` region, source2300-to-AC2 selection,
low-byte duplicate handling, trace null selection, positive-span/chain gates,
publisher and unconditional reset order. No vendor DLL is loaded or executed.

Commands after building the headless probe:

```
LegacyMainWordPublication.Probe AK LK engine separation case-data publication-data capture.json
    [line30 line50 recorded-manifest word-index]
python tools/LegacyMainWordPublication.Probe/audit_output.py capture.json
python tools/handwriting/audit_main_word_publication.py static-report.json
```

The current actual inputs have zero residual events and no source70 events.
Additional glyph subtypes,
genuine nonempty residual traces, associated sources, detailed/trace publication,
final alternative assembly, quote normalization and the full `2271F0` result
sequence remain separate. This owning callback publishes plain linked words;
it does not claim complete engine restoration or emitted-text accuracy.
