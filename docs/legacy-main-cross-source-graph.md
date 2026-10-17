# Owning ordinary cross-source construction and final pruning

`LegacyFreshMainGlobalOrdinaryState.Prepare(AK, actualLocalGraphs, table)`
continues the [allocation-time local graphs](legacy-main-local-graph-metadata.md)
on the same sequential shared cut source. It retains primary **and secondary**
descriptor/line ownership, global physical ranges, composed chronology and
original source through that owner. No independent line samplers are looped.

This result is the ordinary96-byte group/32-byte arc state after `19E8B0`.
Native compact38/arc-table materialization, global227 context ownership,
normalization/NN and multiline decoding are still separate. In particular,
one scalar line-height/baseline cannot be applied to every cross-source group.
This endpoint does not promise recognition accuracy or expose a finished
classifier graph.

The fresh scope is exact main247@0, sampler297@188157, graph21B@189379 cap8.
Reader `1AD9D0` maps serialized graph byte349 (actual1) to runtime158, enabling
different descriptors. Constructor `19C970` clears1DB inside DWORD1D9 and2D3
inside DWORD2D0. Clone `19CB00` copies only the resource prefix8..15F plus the
individual1D8/2D0 bytes; `1A6980` does not overwrite1DB or2D3. This owning raw
factory excludes external/reused property state, so cross construction runs
and optional `19E320` does not. These are constructor/clone/invocation bindings,
not caller-provided feature switches.

For each nonempty local source, append its live groups/contexts/nodes using
`19D470`. If previousLocalNodes+currentLocalNodesР Р†РІвЂљВ¬РІР‚в„ў1<cap, call `1A3B30` with
leftNode=old global end, requiredLeftSpan=previousLocalNodesР Р†РІвЂљВ¬РІР‚в„ў1 and rightNode=new
global end. The right alternatives come from that exact left selector. Same
descriptor pairs and right flag8 are excluded. Ordinary source metadata4/8/10
uses its native sentinels before the bbox/guide tests. Multi-physical geometry
uses guide+C for its baseline difference and guide+4 for height; it does not
replace these with source-frame/bbox guesses.

Cross histories use native `1A3450` sorting and `193690/193720` composition.
Interning is first-match in ordinary group allocation order. New groups retain
left primary/context and right secondary, real union bbox, zeroed native kind
and flags, and saturated counters. `1A41C7` searches for a reusable arc at
**leftNode+1**, even when allocation occurs at rightNode; this distinction is
preserved. New `1A37B0` arcs keep predecessor/successor list order, span, context
IDs and auxiliary inheritance. They do not allocate a fictitious global atom.

After **all** append/cross calls, `19E8B0` visits every nonempty descriptor start:

1. mode1 over[start,min(globalNodeCount,start+cap));
2. mode0 at that descriptor's final node;
3. advance start by the local node count.

Mode1 has important membership shortcuts. SecondaryFFFF immediately skips the
arc (`1A5C28Р Р†РІР‚В РІР‚в„ў1A616F`), without evaluating raw separation. For a real source pair,
presence of primary first and last physical IDs also keeps it. Otherwise a
complete secondary physical suffix is required before sampler194140; missing,
nonzero-start, nonconsecutive or short-final ranges reject. Mode0 evaluates
ordinary nonzero spans directly, including the local-final nodes omitted by
the earlier local pass. For two assigned guides, `193860` divides by the union
of their guide0/4 vertical intervals, **not** max(height). Raw cost is compared
to11C before later category clipping. Deletion preserves reciprocal list
multiplicity/order and decrements counters only below200. The native signed16
ordinal gate at1A5BD0/1A5BE6 is retained.

Portable safety remains explicit: at most131072 groups/arcs,65535 row ordinals,
4096 nodes,1048576 owned range/history/connection entries and1048576 cross work
steps per mutable kernel, including repeated point visits. Mutation is private; failure/cancellation publishes
no partial result. A prepared325-reference group explicitly exhausts the
point/work budget without a huge source allocation. Unassigned secondary-guide zero-height arithmetic is refused
instead of inventing a height. The current actual cut owner already requires
assigned ordinary guides. Pending separators, including any trailing pending
events, remain with the retained cut-source owner.

`LegacyMainCrossGraph.Probe` has96 explicit prepared append/cross cases, eight
prepared membership-boundary controls and seven actual raw owning inputs. The
actual M-shaped4+4, vertical1+1 and two-physical2+2 descriptor pairs produce
five genuine source pair groups; MIR10+10 is excluded by the cap. These seven actual fixtures have no
11C rejections; the115 deletion/event-order controls are prepared, not claims
about real handwriting. No expected word chooses a source, cut, graph or label.
The independent Python oracle checks complete fields, histories, raw float
geometry, guide-union separation, append/remap, reciprocal deletion and event
order. The static auditor pins61 instructions in eight hashed Text PE blocks
and actual AK cap/runtime158 data. No vendor DLL is loaded or executed.

Probe arguments are AK, engine, separation, case, publication, line30, line50,
capture JSON. Run `python tools/LegacyMainCrossGraph.Probe/audit_output.py capture`
and `python tools/handwriting/audit_main_cross_source_graph.py --output report`.

The old single-descriptor sampler currently stops at local pruning, so its last
node needs the separately authorized final-mode0 integration. Existing real
fixtures below11C retain their old bytes; universal parity is not asserted when
native final pruning legitimately removes a candidate.
