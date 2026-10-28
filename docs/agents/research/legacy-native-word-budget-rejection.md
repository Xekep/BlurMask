# Word heap rejection is not a decoder failure

The ordinary startup1FD240, parent201270 and alternative1FE740 consumers now
preserve the native distinction between declining a candidate and failing the
decoder routine. A full heap's strict cost test, or a zero-capacity heap, can
produce `Allocation.Accepted=false` without invalidating the caller. Remaining
query labels/arcs still run, and a later page can reuse the same producer.

Static1F8D60 is decisive: when1F8C90 returnsnull, it jumps to1F905D and returns
lowbyte1. Nontracing1F9080 likewise returns1 when1F75B0 provides no slot. Their
trace-allocation branches can return0, but the supported ordinary adapters do
not enter those branches. They continue to propagate actual exceptions,
malformed ownership, cancellation and callback failures; those invalidate the
session. The scalar heap allocator itself is unchanged and still rejects a
candidate equal to or more expensive than its full root.

Previously the three adapters converted `!allocation.Accepted` into routine
failure, returned before later candidates and invalidated reuse. This would
abort an owned2144F0 node pass as soon as its finite heap declined a weak
candidate, preventing the intended bounded search.

The regression checks include strict/equal/looser seeded full heaps for all
three consumers, complete128-byte preservation on rejection and repeated
producer reuse. Startup has an independent scalar control: query37 plus final
class-cache321 plus geometry0 yields358. Seeds357/358 reject every such
candidate;359 accepts one358 candidate and then rejects its equals while the
routine continues successfully. Zero capacity also succeeds without a slot.

For parent/alternative tests, the actual main118/140 geometry produces cost
cells over the checked graph. A supplied zero class/geometry cache leaves that
preallocation value unchanged at128, so the oracle takes its minimum **before
any heap allocation**, independently of the heap's choice. Seedsminimum-1,
minimum andminimum+1 test strict rejection, equality and acceptance. These
controlled cache values isolate routine/heap semantics and are not recognition
quality samples.

Existing original controls and actual-resource chains still run:512 full128
startup controls,1024 full80 parent packets,1024 full80 alternative packets,
512 restart tags,512 grouping queries and1024 reductions. Actual NN-derived
permission/query/cache records, typed continuation pages and materialization
are retained. Full native main scheduling/winner remains a separate stage.

Each existing probe takes five arguments: its controls JSON, acoustic resource,
lexical resource, engine profile and separation profile. Managed and Windows
Native AOT execute the same old controls and new heap regressions. Vendor DLLs
are read statically, never executed.
