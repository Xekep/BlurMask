# Persistent incoming secondary workspace

`LegacyNativeSecondaryRecombinationWorkspace` ports the incoming 40-byte
alternative writer in the nontracing higher-arc recombination branch of
`MyScriptText 18020e6a0`. Native explicitly calls
`25ee50(local_378, 0, 800)` once before iterating candidates. Accordingly this
owned workspace starts with exactly 800 zero bytes, retains that storage across
incoming candidates, and exposes only detached full-capacity snapshots.

For an available source secondary alternative (`00 < 04000000`), native writes
signed wrapped `adjustedCostOffset + source.cost` into `00`, wrapped
`incrementalOffset + source.increment` into `04`, count byte `1e = 0`, candidate
arc into short `18`, low-seven-bit alternative index into `1b`, and source mapped
context into qword `08`. All other bytes retain their previous value. For an
unavailable alternative it writes **only `00 = 04000000`**, retaining the prior
candidate's increment, context and metadata. It does not fill unavailable rows
with zeros or reset the workspace between candidates. Raw context `10` and
other unwritten fields stay at their literal initialization until another
explicit native writer changes them.

The API requires an exact owning configured source-secondary block and counts
2 through 20. The configured count fits the native 800-byte local array. Its
two offset inputs are explicit outputs of the preceding controller/correction
policy: it does not replace the `118/140` compatibility callbacks, source
descriptor ownership, earlier candidate filtering, or adjusted-cost production.
Cancellation is checked before each update/alternative; earlier completed writes
to this persistent object are not rolled back. Source blocks and returned
snapshot arrays do not share mutable workspace storage.

`tools/LegacyWordSecondaryWorkspace.Probe/reference_controls.py` independently
models 256 persistent streams with four candidates each. All 1024 updates
compare every one of the 800 bytes. They cover count 20, signed overflow in both
offset additions, full-width candidate arcs, residual metadata, and 5253
cost-only unavailable writes. Detached snapshot ownership, wrong source bounds
and pre-cancellation are checked separately.

The real Russian resource control uses six NN/final-cache scored child frames
and their actual stored root parents. It holds explicit diagnostic cost/increment
offsets constant per frame, reuses one workspace across six candidate parents,
and feeds all 36 updates through the native `1f87c0` merger, the post-merge heap/
context projection, actual owning parent contexts, ordinal writer and row
materializer. Raw contexts are checked against each chosen parent plus the
current frame's suffix, rather than incoming stale context fields. The scalar
incoming writer is now connected to its consumers; native eligibility and
runtime transition-cost orchestration remain separate unfinished work.

Generate controls with `reference_controls.py output.json`, then supply controls,
Russian acoustic `.res`, lexical `.res`, engine profile JSON and separation JSON
to the probe. Managed and Windows Native AOT checks use no GUI or vendor DLL.
