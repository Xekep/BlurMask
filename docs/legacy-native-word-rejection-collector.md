# Native rejection ancestry collector and row consumer

`LegacyNativeWordRejectionCollector.Collect` ports the allocation-success,
nontracing, initially empty candidate-vector path of `MyScriptText 180210870`.
It consumes actual stored rows, a complete full-capacity 128-byte heap buffer,
the actual current node and `230a` primary arc limit. `CollectAndStitch` composes
the producer and both consumers on the same live owning storage. Its output feeds the
existing `211210` selection and `2113f0` live primary path stitcher. The collector
also performs the native raw-context invalidation on the owning primary rows.

This closes the earlier prepared-field gap: ordinary/anchored ancestry and
candidate cost/correction are now produced from actual record/alternative
metadata, rather than supplied scalar candidate fields. Source descriptor
headers, runtime `22f8` marker mode, per-append stack residue, and owning
higher-arc rows remain explicit inputs. It does not infer missing native source
scheduling, final winner, acceptance confidence, or production Russian policy.

## Proven producer/consumer behavior

Addresses use image base `180000000`. The implementation uses static private
exports; it never loads or invokes the vendor DLL.

* The outer `210870` loop visits primary arcs in ascending `ushort` order and
  stops at the first stored record `10` cost above `03ffffff`. Only source header
  bit `20`, selected by stored record byte `04`, enters the collector. The heap
  scan retains every `3e` encounter whose `1e` equals the current arc and whose
  `3e` is not `ffff`. Encounter order and duplicates are preserved.
* Every available primary alternative traces actual `18/1a/1b` parent metadata.
  Spans at `1a` are signed bytes; no chronological or sorted-ID replacement is
  applied. Tracing stops at node zero, a protected source state `1f & 40`, a
  protected parent, an absent/above-limit parent, or a raw context marker.
  Ordinary raw markers are `fffb` and `fff9`; with `22f8` enabled the additional
  predicate is literal `ushort(symbol + 15h) <= 8`. The per-original-alternative
  stop arrays retain parent node, last source node and last source arc.
* If the last source was protected, repair is enabled. Otherwise, a nonnegative
  predecessor with non-`ffff` arc gets its repair flag from stored record byte
  `05 & 04`. The no-heap-replacement branch emits only `210720` fallbacks for
  those flagged alternatives, including the native negative-node cases.
* For each heap replacement, the cost phase intentionally reads the stopped
  alternative using the **original outer alternative index**. It does not use
  the alternative index encountered at the end of tracing. A null raw context
  or raw context with null parent invalidates that stopped cost to `04000000`.
  Those writes are live and remain visible to subsequent collector iterations.
* Protected stops take the replacement prior arc from the stopped alternative
  `18`; other stops take it from the actual replacement record `1e`. Available
  target alternatives are visited in ascending index. The first unavailable
  target emits one fallback and ends that target loop; candidate summed cost
  above `03ffffff` ends the loop without an append.
* Candidate cost is the wrapped signed sum of replacement-record cost `10` and
  stopped-record cost `10`. For a stopped physical predecessor below `230a`,
  correction is prior-alternative cost minus its prior-record cost plus stopped
  record cost. The above-limit branch follows actual stored `1e`, `38/3c`,
  signed span `20`, and replacement `3e` to compute its anchored difference.
  Higher rows are record-only owning snapshots; no primary alternatives are
  fabricated for records that the `213410` materializer stores without them.
* Generated 36-byte records use the already verified ordinary/fallback writers.
  Source and target alternative bytes remain distinct, all three stack-residue
  bytes survive, and selection starts at zero. Source/residue/higher-row callback
  cancellation is checked immediately after each callback before consuming its
  returned data. Candidate sorting and row repair are separate subsequent APIs.

## Scope and ownership

The current adapter starts with an empty candidate vector and assumes successful
native temporary/vector allocation. It does not simulate `1.5` capacity growth,
allocation failures and their partial vector state, or native tracing side
tables. The existing stitcher explicitly refuses tracing. The primary row owner
keeps actual complete 88-byte records and configured primary/secondary blocks;
the higher-row callback owns complete 88-byte snapshots with optional empty
primary storage. The source-header callback returns actual header bits for a
record's source selector; its policy is not guessed from the language profile.

The checked implementation refuses missing rows/alternative storage, foreign
context identities, invalid spans/indices, cycles, ancestry beyond 4096 points,
more than 65536 heap/candidate records, and absent required higher-row ownership.
It has no rollback: raw-parent invalidation or subsequent primary stitching
already performed before cancellation remains live. Input heap bytes, original
supplied row snapshots, stored-record bytes and secondary blocks are preserved.

## Independent and real-resource controls

`tools/LegacyWordRejectionCollector.Probe/reference_controls.py` implements an
independent scalar ancestry/cost collector and then composes the separately
verified recursive selection and functional-symbol-path mutation oracles. It
does not execute the managed implementation or vendor DLL. Its 512 streams
include nodes zero/one/three, native 20-alternative local-vector capacity,
unavailable alternatives, duplicate heap replacements, ordinary and extended
markers, parent-null contexts, signed spans, protected links, ordinary and
anchored correction, and fallback/no-replacement paths.

The controls produce 739 complete candidates and compare 10240 complete stored
rows before/after the producer and its selection/stitch consumers. Exact source
header order, higher-record callback order and candidate-residue append order
are checked, and the standalone composed endpoint is verified against the same
independent expected primary bytes. They exercise 671 traced steps, 190 marker
stops, 266 protected stops, 166 raw-parent invalidations, 134 ordinary corrections,
122 anchored corrections and 483 fallback appends. Three focused cancellation controls cover
source header, residue and higher-row callbacks with the native live-write scope.

The actual Russian resource control builds neural/final-cache scored roots and
children, uses the native continuation writer for the input heap's `1e/3e`
relationship, produces two collector candidates from real owning records, and
feeds selection/stitching. A subsequent parent-alternative producer and
materializer consume the collector-selected row and verify its cost and context
lineage. The source descriptor, replacement scheduling, marker mode and unknown
stack residue are explicitly diagnostic; no expected word, winner or acceptance
threshold is injected.

Generate controls with `reference_controls.py output.json`, then run the probe
with five arguments: controls JSON, Russian acoustic `.res`, lexical `.res`,
engine profile JSON and separation profile JSON. Both managed and Windows Native
AOT execution are supported without GUI or vendor runtime loading.
