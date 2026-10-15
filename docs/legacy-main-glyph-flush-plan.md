# Owned main glyph flush plan

`LegacyOwnedMainGlyphFlushPlan.Prepare(casePolicy, actualBacktrackRows)` restores
21FFD0's ordinary fresh CASE flush selection and its scalar/control writes.
It consumes the actual selected compact pool, numeric append events, full
predecessor chronology and owned graph boundaries. It does not accept a word
label, supplied segment boundary, synthetic winner or all-enabled masks.

A predecessor FFFA causes the ordinary segment to flush **before** that FFFA
is visited. Visiting FFFA then causes a separate protocol-only flush. Treating
the whole backtrack row vector as one word loses this distinction. The ten
actual inputs below all produce two flushes, without reading their expected
transcriptions. Their ordinary segments have respectively 1, 2, 3, 5, 2, 3,
6, 2, 3 and 5 numeric rows; the additional FFFA segment has one row.

## Ownership and unfinished stages

Every flush retains the exact `BacktrackRows` and hence its materialization,
confidence, graph, terminal and fresh CASE policy. Public storage getters copy.
`CopyRows72()` preserves all numeric row padding. `CopyResidualAdditions()`
returns references encountered since the preceding flush, in native order;
it does **not** assert that earlier residuals were consumed. A genuine per-flush
217DB0 callback must carry its remaining old vector and append these additions.
This plan performs neither residual association nor any glyph/pool mutation.
It does not predict callback success: a publication executor must stop on the
first failed callback, leaving native prefix effects to that executor.

The current scope is fresh ordinary source headers `[0, 0xC0A]`, no tracing,
no mode22F6 or knowledge22F8, and subtypes already supported by the owned numeric
walker. Source class12 extra flushes, implicit source0 metadata, associated or
external state, an unflushed terminal tail and trailing unflushed residuals are
refused. The complete21FFD0 and accepted OCR output are not claimed.

20E230 clears25F8 before scanning source headers. It can append exclusion ranges
only after finding a source of class0xA00. Neither actual selected header has
that class. Together with20BAA0's zero initialization and the absence of other
named direct25F8 writers, this closes the ordinary no-range-override branch;
no positive/zero caller option is invented from2558.

## Packet writes and seed dependencies

A flush exposes a 64-byte **write template**, not a completed publication
packet, and a64-entry initial-WORD byte-origin map. Origin minus-one means the
template byte is known. A nonnegative origin is the exact byte offset to read
from the initial actual WORD276120-byte storage. Unknown template bytes are
storage placeholders; they are not claims that native metadata was zero.
A later seed bridge must bind the same actual WORD/pool owner before resolving
these origins. This plan does not manufacture a TOP273 owner.

221840 seeds packet0E from WORD32, packet14 from WORD3C and packet1C..2B from
WORD44..53. Packet3C initially contains literal minus-one, independently of
WORD3C. Source70 is called only if the **compact field+0x24 (decimal36)** is not
minus-one. The source receives the full tag, the raw compact U16+0x1C symbol and
boundary byte0, before FFFF/FFFD skipping. In the supported CASE path this uses
the same session's `ReadTerminal(fullTag, rawSymbol, 0)`, retaining its selected
source byte and case policy. No permission-span lease is invented after the
pass. A successful callback reset clears metadata and carries the previous
packet14 into packet3C, including any remaining seed dependency.

All ten actual fixtures have compact+0x24 equal to minus-one on every visited
record. Therefore they make **zero actual source70 calls**. The conditional
source70 branch is pinned statically and uses the existing checked metadata
service, but a genuine nonempty invocation fixture remains untested here.
Earlier exploratory code that tested decimal24 would have tested compact18,
not this field; its manual service calls are not native flush evidence.

Ordinary costs come from the real terminal total/RowCost14 and filtered
predecessor compact14/18 plus cumulative2520. Signed32 subtraction/shift wraps
before float conversion; DIVSS/MULSS stores remain separate. No finite guard,
exponential or confidence interpretation is added. The scalar oracle includes
zero denominators and preserves native Inf/NaN bits. Protocol flush writes
exactly `(1,0,0)` to packet00/04/08. Packet38/3C are boundary hints, **not**
TOP273.AC/B0; those result-owner fields have independent producers.

## Reproduction

All tools are headless; no vendor DLL is loaded or executed. The static auditor
reads the pinned PE and corroborates source70 gating, flush predicates,
integer/F32 instruction order, WORD seed offsets, callback-success reset,
source25F8 producers and the ordinary frontend168/170 bindings.

Build `tools/LegacyMainGlyphFlush.Probe/LegacyMainGlyphFlush.Probe.csproj` for
managed or NativeAOT. Its positional arguments are:

```
AK.res LK.res engine-profile.json separation-profile.json
legacy-case-data.json legacy-text-publication-data.json output.json
[line-2e3-30.res line-2e3-50.res private-recorded-manifest.json word-index]
```

The four optional arguments select an actual recorded coordinate track at
index0..8. The pinned dataset lacks pen-up events; one unsegmented path is an
explicit diagnostic input assumption, not ground-truth stroke segmentation.
Samples remain private, expected transcription is never read and no recognition
accuracy statistic is inferred from these runs.

```
python tools/LegacyMainGlyphFlush.Probe/audit_output.py AK.res output.json
python tools/handwriting/audit_main_glyph_flush_plan.py static-report.json
```

Managed and fresh Windows NativeAOT tests cover single-source M plus all nine
recorded tracks:640 full plans,1280 flushes,2688 complete72-byte rows and40
ownership/cancellation guards per runtime. The independent raw compact/graph
oracle compares every control byte and seed origin;4096 additional independent
scalar controls check signed wrap and exact float32 bits. Initial WORD/pool
ownership, actual source70 events, per-flush217DB0 and21FE30 publication remain
separate work.
