# Actual English sparse language data and fresh numeric binding

`LegacyEnglishLkSparseLanguageData` loads the actual English2D2/v101 region from
its pinned lexical resource and retains the supplied `LegacyEnglishLkPackedData`
object identity. `LegacyEnglishLkFreshNumericBinding` derives source135/profile2
and scale1 from that same owning object. It constructs the actual literal
packet.byte0=0 numerical branch only. No caller chooses a profile, scale,
codebook, expected word, category mapping or packet1/2 policy.

The public reader verifies the complete input SHA256, copies it, verifies the
owned snapshot SHA256 again, then retains only the1580304-byte language region.
All75 fields are consumed exactly. Five packed vectors remain packed inside
that immutable wire;643871 expanded integers are never retained. Raw code
indices, sparse offsets and strictly sorted row labels are checked before the
object is published. Packed snapshots and75 field descriptors/payloads are
explicit defensive copies. No caller buffer, writable view or parse workspace
is retained. Construction can be retried after any cancellation or refusal;
there is no source journal/counter/one-shot publication in this data API.

Actual inherited64=260074 is the reserved extra word row and maps to class0;
ordinary dictionary terminal cardinality remains260074. Flags7 omit90 and268C,
but the constructor default for268C isFC000000. Numerical binding preserves
its negation04000000 instead of treating absence as zero. Word costs subtract
actual110[2]=0; class costs subtract F0 profile2 offsets and use130 profile2
multipliers. Backoff0x04000000 is transformed normally because literal packet0
is0. Boundary94C is12894; finalUnigram94D is115101.

Individual multiplication/addition/subtraction helpers are non-inlined to
preserve every native binary32 operation boundary and prevent FMA contraction.
Integer conversion has an explicit finite signed-int supported domain. Case
conversion rounds half away from zero and negates. Access is limited to the
native written1442D and1728triple cells, explicit[0,0]/[0,0,0] zeros and the
native duplicated triple history0 plane. Unwritten zero-border cells refuse.
There is no guessed all-zero169/2197 table and no claim about configured
ignore-case/capture policy.

The complete conservative logical copy/work preflight is12416948 bytes, bounded
to32MiB, and
includes input/owned input, two language-sized regions, a whole decoded/snapshot
workspace allowance, cost/case output plus defensive copies, descriptors and
hash scratch. Production does not allocate that decoded workspace. This is a
logical byte envelope, not managed/native processRSS. Every public defensive
snapshot checks its own bounded copy size. Cancellation is checked per32768
hash bytes, per field, per4096 packed values/rows and in numerical/case loops.
Prepared checkpoint hooks and hash-bypass structural controls are internal to
the named probe; the public reader always verifies both hashes. No callback
runs under an owner lock, and no failed constructor exposes a partial object.

The standalone [reference](../tools/LegacyEnglishLkSparse101.Probe/reference_actual.py)
reads actual resources without runtime outputs or expected handwritten labels.
The runtime oracle independently compares all75 field hashes, all643871 packed
values via digests, all1536 compiled cost entries and all144/1728 written case
results. Managed and Windows NativeAOT captures are byte-identical. The probe
also exercises143 refusal guards,18 canceled constructions,21 separately named
prepared controls, mutation isolation and stateless retry. Prepared controls
are not genuine resource acceptance or handwriting samples.

The [native boundary audit](../tools/handwriting/audit_english_lk_sparse101.py)
checks16 complete Ghidra bodies/315 call bindings/5 tables, plus3 bounded RET
leaves and the bounded2B7 constructor, against the original SDK bytes. It never
loads or executes vendor code. Existing packed P, English finalized-single,
V2/parent/directchild/plain and Russian lexicon/query regressions remain the
old owners; their guards and APIs are not widened. This checkpoint adds only
new runtime/probe/doc files and no old code seam.

Run `audit_runtime.py MODELS CAPTURE OUTPUT --native NATIVE_CAPTURE` from the
probe directory to rerun the independent raw reference. Its dependency is the
committed portable `reference_actual.py`, not an E-drive or research path.

Source V58 query/history/heap/case-request producers, configured root selection,
priority/epoch, normalization, shared contexts, masks, main80/B8/100 inference
and a linguistic winner are still separate dependencies. These objects cannot
be passed as proof that a native dictionary source/controller successfully
prepared. No English/Russian recognition-quality improvement or whole-engine
completion is claimed by the table checks.
