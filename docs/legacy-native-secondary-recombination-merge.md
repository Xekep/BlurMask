# Native secondary recombination merge

`LegacyNativeSecondaryRecombinationMerger.Merge` ports the nontracing
`MyScriptText 1801f87c0` merge used in the remaining higher-arc branch of
`18020e6a0`. It takes actual configured secondary 40-byte blocks and returns an
owned destination block plus the number of positions written. It does not
produce incoming candidate costs, choose eligible higher arcs, update heap
record fields, perform the later context refresh, or choose a final word.

The first selected alternative is the first incoming row when its signed cost
is less than **or equal to** the first destination cost, otherwise the first
destination row. That first selection happens even when both costs are
`04000000` or above. Subsequent selection visits each supplied stream in its
existing order, admits signed costs below `04000000`, breaks cost ties in favor
of incoming rows, and suppresses duplicate **mapped context identities at `08`**.
Raw context, metadata or lexical text equality do not enter this deduplication.
The implementation does not call a generic sort or reorder the supplied lists.

After selection, native writes the destination **in reverse order**, reading
left-source rows from the live destination. For each written alternative it
copies only cost `00`, increment `04`, byte count `1e`, the entire metadata dword
at `18`, and mapped context qword `08`, in that order. Raw context `10`, bytes
`1c/1d`, state `1f`, and all other destination residue remain unchanged. It also
preserves unwritten active positions and full inactive capacity. This matters
with duplicate source lists: reverse writes can change a later source row before
an earlier selected position reads it. The API deliberately preserves that
native alias effect rather than snapshotting selected rows. Caller buffers are
preserved; live aliasing happens inside the owned destination copy.

The checked API requires configured counts 2 through 20, complete covering
40-byte storage, distinct incoming temporary storage, and cancellation checks.
The caller that invokes `1f87c0` enters with enabled secondary storage; the local
native selection vector bounds this port to 20 alternatives. Tracing `2368` and
its `280`-byte side-table copies are explicitly refused. The API does not assume
that arbitrary input streams are sorted or unique; it reproduces the literal
operation on their given order, including alias behavior outside usual callers.

`tools/LegacyWordSecondaryRecombination.Probe/reference_controls.py` implements
an independent two-stream selection and byte-slice reverse writer. Its 1024
controls compare complete destination capacity, signed limits, empty availability,
ties, duplicate identities, count-20 storage and unsorted encounter lists. Fifteen
controls distinguish live native copying from snapshot copying. Trace, overlap
and cancellation guards are checked separately.

The actual Russian resource control uses neural/final-cache scored stored child
rows with their real shared symbol-context identities and costs. It performs all
36 pairs of six child rows, checks retained raw/state/residue fields and copies
the resulting pool through the real ordinal writer and `213410` row materializer.
The materialized record and primary blocks remain exact. These are diagnostic
incoming pairs: higher-arc eligibility, incoming score adjustment and subsequent
heap/context refresh are still separate producer/consumer work, not guessed
word-controller policy.

Generate controls with `reference_controls.py output.json`, then supply controls,
Russian acoustic `.res`, lexical `.res`, engine profile JSON and separation JSON
as the five probe arguments. Managed and Windows Native AOT checks run without
GUI or vendor DLL loading.
