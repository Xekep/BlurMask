# English CTC conversion: standard copy-helper comparison

Static checkpoint, 23 October. No SDK code was executed, no recognizer default
changed, and retained-graph alias preservation remains unproved.

The matrix conversion helper `Engine198A00` calls `1D9730`. Its registered
Ghidra body covers nine regions/1353 bytes, but that is not the complete
reachable implementation. Two indirect tables reach another35 instructions/
247 bytes outside those regions. A conditional branch also reaches the
separate16-byte `1D9720` REP MOVSB helper. All those bytes are retained and
checked independently against the pinned Engine PE.

The installed Microsoft CRT source is
`VC/Tools/MSVC/14.51.36231/crt/src/x64/memcpy.asm`, SHA256
`c9079ae590e0c34a1669a3dadce2c40e787fa021a324bbd18d75cfae520b01d2`.
It implements the shared memcpy/memmove routine. Assembling that unmodified
source and comparing it statically found one concrete difference: the newer
source adds a five-instruction near-copy dispatch guard before testing
`__favor`. The unmodified source comparison fails, as it should.

An explicitly recorded compatibility source removes only that guard:
`lea r9,[rcx+64]`, `cmp rcx,rdx`, `cmova r9,rdx`, `cmp r9,rdx`, and the following
`ja MoveWithYMM`. Original and modified sources and both assembled objects are
preserved separately. The SDK and original CRT source are unchanged.

Against this documented variant, the complete reachable control-flow graphs
match:330 instructions after excluding only reachable NOP padding, all43
ordered entries of four RVA dispatch tables, and the separate REP MOVSB
helper's exact16 bytes/nine instructions. Branches map to exact instruction
ordinals. Relocations identify the three globals and four tables; scalar,
memory and register operands otherwise match. This identifies the copied
runtime implementation through a bounded source comparison; it is not a claim
of identity with the unmodified installed CRT.

The comparison narrows the remaining work to the actual loader's selected
allocations, pointer ranges and matrix aliases. It does not establish that
those destinations cannot overlap a retained graph header, does not approve
a `V120=false` shortcut, and does not supply a finalized CTC line guide.

Frozen E evidence manifest:
`root-msvc-copy-source-comparison-frozen-manifest-20261023.json`, SHA256
`a75d32b2c13a88a7a43005a025d5bae34327e47774eb36c1e050bd8fd40bb06b`.
It pins both source variants, both objects, the comparison scripts and report,
assembler/include dependencies and the preceding byte audits. Microsoft CRT
source and compiled objects are analysis artifacts; they are not added to the
application or distributed by this change.

## Packet helper coverage supplement

The loader contains a call to `106360` in its **pre-0x106 version branch**,
outside the earlier24-function instruction inventory. The selected full English
resource is version0x106 and takes the direct `10E410` builder path instead.
The following helper audit therefore covers the older branch; it does not
establish the selected resource's admission. A fresh read-only export covers its full
registered body: 1466 bytes and 393 instructions. The actual `106DD0` caller
passes the local packet address and size112; this is pinned from instructions.
The immutable evidence manifest is
`root-ctc-packet-admission-frozen-manifest-20261023.json`, SHA256
`606d165b3a336ca80198a2aaac330a425db846e7563d8517e8f89dd64b94be50`.

`tools/handwriting/audit_english_ctc_packet_admission.py` verifies that complete
body, 26 instruction pins and six branch/call destinations. Conditionally,
when packet count+5C and pointers+60/+68 are zero, its two optional scans and
two post-builder mapping-write loops are bypassed. The builder `10E410` still
executes. This is a conditional packet-path result, not proof that the chosen
raw model produces that packet, or that builder destinations cannot alias
retained graph headers. Those proofs remain open. No vendor code is executed
and no runtime predicate or application behavior changes.

## Direct state-metadata extent supplement

`tools/handwriting/audit_english_ctc_state_metadata.py` checks the complete
356-byte/98-instruction `191470` body and 1070-byte/292-instruction `192AE0`
body directly against the pinned Engine PE. Capstone decodes every byte;
partial-register alias writes, the real MOV-based RBX restoration and the
callee's nonvolatile save/restore are checked explicitly. No SDK code executes.

The 64 direct metadata stores fit a 32-byte prefix and two 192-byte substates
at offsets32 and224. Their composed extent is416 bytes (`0x1A0`), matching
the actual constructor's kind8 allocation unit. This result does not establish
the allocation count, allocator separation or provenance of the caller's
pointers. The report lists another24 non-stack store instructions whose
payload bounds and aliases remain unproved. Runtime/workspace alignment is
also not inferred from serialized-model alignment.

One positive audit agrees with the independently decoded instruction-table
store inventory; seven corrupted extent, offset, dispatch, allocation,
save/restore and body-length controls are rejected. The earlier failed
POP-restoration assumption is preserved separately, not rewritten as a pass.
Evidence manifest `root-ctc-state-metadata-frozen-manifest-20261023.json`, SHA256
`3d0c58c7b206aa5ca0a5db44667516e8bf70d8d87ea13543d9cd4e4cd520017d`,
pins13 analysis files and dependencies. This is a partial static result;
`V120=false`, complete graph-header alias preservation and CTC guide authority
remain unapproved.

## CPU-layout supplement

`tools/handwriting/audit_english_ctc_cpu_layout.py` checks the complete
81-byte/27-instruction alignment mapper and6883-byte/1132-instruction CPU
setup body, including all seven dispatch entries and the actual stored field.
Enums1..7 map to float-unit alignments `[8,8,8,16,8,16,8]`; this is a runtime
packing rule distinct from the serialized model's alignment. CPU detection is
not executed, and the auditor does not prove its successful-result domain.

The independent raw reader pins the actual nested5F/version0x106 envelope,
four input/recurrent layers and separate output metadata. All three recurrent
layers have width127 and flags0. Conditionally on the native state-helper
layout and either supported alignment8/16, the prefix stride is256 and each
subvector stride is128. Consequently both alignments require the same kind8
workspace:14336 bytes for two rows,21504 for three rows. No host CPU choice or
extra-row mode is guessed. These arithmetic checks do not prove allocation
separation or the complete set of loader aliases, and do not approve V120.

One positive audit and seven corrupted body/table/field/resource/call/stride/
row-count controls pass. Frozen manifest
`root-ctc-cpu-layout-frozen-manifest-20261023.json`, SHA256
`18f1cdc678d70ebb65d3e2e480d14dfc59f84787b7792c353a398b560e399c5e`,
retains17 analysis files/dependencies, including the unchanged SDK/model and
an immutable snapshot of the portable auditor. No vendor code executes and
the application's CTC framing remains unchanged.

## Conditional scalar-index supplement

`tools/handwriting/audit_english_ctc_payload_indices.py` composes the two
preceding byte/model audits with an explicit algebraic mapping of every payload
store site in `191470`/`192AE0`. For actual width127/flags0, both alignments and
both extra-row modes reserve thirteen consecutive vectors: one leading vector
and six per substate. The24 distinct native store sites correspond to46 scalar
write occurrences because the substate initializer is called twice. The first
five substate vectors clear their previously initialized bias cells; the sixth
retains them. Repeated writes are retained in the certificate.

Every computed scalar write fits its own vector and the14336/21504-byte
workspace. This is a **conditional arithmetic certificate**: it explicitly
assumes the stated native-to-row mapping and a live, disjoint caller workspace.
It does not prove the complete native pointer dataflow, allocator separation,
matrix-copy aliases, loader admission or V120. It supplies no runtime shortcut.

Four arithmetic cases and eight corrupted-site/width/stride/row/count/overlap
controls pass. An earlier proposed control removed the bias increment before
rounding; on this specific width it leaves the rounded stride unchanged and
correctly was not rejected. That failed test assumption is preserved separately;
the final control changes the actual stride. Frozen13-item manifest
`root-ctc-payload-indices-frozen-manifest-20261023.json`, SHA256
`42b9c89a3d6e25fecd95caa64f001bea19ea36dc34a9ba27262bbee7ead77198`,
retains immutable copies of all three auditors and their dependencies. No SDK
code executes and application behavior is unchanged.

## Successful CPU-selector domain supplement

The later `audit_english_ctc_cpu_detection.py` closes the successful-result
domain left open by the earlier CPU-layout audit. It checks the complete
463-byte/133-instruction `10EE70` body, all seven switch targets, register
aliases, four output stores and all five returns. Requested enums outside1..7
fail the unsigned bound; enum7 reaches the unsupported-platform error tail.
Automatic selection returns only1,2,3,5,6. Successful explicit selection returns
one of1..6, including enum4. The combined domain is therefore1..6, which the
previous alignment mapper maps entirely to8/16 float units.

The proof allows all16 low feature-mask combinations, including combinations
that no real CPU may provide. It neither executes CPUID/XGETBV nor chooses a
host CPU configuration. Its72 abstract successful cases and eight corrupted
body/pointer/bound/table/return/domain/reset controls pass. This narrows the
alignment proof; allocator separation, complete native pointer dataflow and
retained-header aliases remain open. No V120 shortcut is approved.

Frozen nine-item manifest `root-ctc-cpu-detection-frozen-manifest-20261023.json`,
SHA256 `50dc48c51213c180e809292c60d0be6c7079e9bb73cf4c4a76718e6125bd0d37`,
retains the immutable auditor, full-body export, reports and byte dependencies.

## Bounded initializer address-dataflow supplement

`audit_english_ctc_state_dataflow.py` now follows register aliases, stack
arguments, metadata reads, branches and both nested calls in the two pinned
initializers. Addresses remain symbolic offsets in separate state/workspace/
stack objects. Unknown pointers, unwritten or partial reads, unsupported
instructions and undefined branch flags are refused. No SDK is loaded.

Eight cases cover alignment8/16, two/three rows and NULL/populated workspace.
All24 payload-store sites resolve to46 writes for each populated case; exact
addresses, scalar values and native order match the independent algebraic
certificate. Metadata stays within416 bytes and payload within14336/21504.
Nine corrupted pointer/stack/width/bounds/call/order controls are rejected.
This establishes the helpers' bounded address dataflow under a live, disjoint
caller-object assumption. It does not establish that assumption in the complete
loader, matrix-copy aliases, retained-header preservation or V120.

The cross-check corrected the earlier algebraic certificate's `orderedStores`:
native code initializes all six vectors before clearing the first five bias
cells. The old certificate grouped each vector's initialization and clearing
together. Counts, values and individual bounds were unaffected; the original
frozen certificate is preserved. The first analyzer draft also rejected a
valid XOR zeroing idiom because it read the old pointer's low bits first; the
corrected transfer rule recognizes the zeroing operation without that read.

Frozen16-item manifest `root-ctc-state-dataflow-frozen-manifest-20261023.json`,
SHA256 `f2b729d0d3e585dd205601d870b132f154267b85b860559002086f36034e2b0f`,
retains all four auditors, reports, controls and preceding byte dependencies.
Application behavior and CTC framing are unchanged.

## Matrix-metadata initializer address flow

`audit_english_ctc_matrix_metadata_dataflow.py` follows the four complete
`1915E0/192F10/193070/198960` helpers:1169 bytes and268 instructions. Its24
cases cover declared widths16/128/256, code2, flags0, alignment8/16, both
initializer modes and NULL/populated scratch pointers. This is symbolic address
analysis; no SDK instruction executes.

Every metadata write stays inside the declared896-byte object. An independent
field-image calculation matches all840 defined bytes and leaves56 padding bytes
unclaimed. Four gate descriptors intentionally share one scratch pointer within
each direction; the second direction uses the next scratch slice. No scratch
payload writes occur in these helpers. The checker preserves that sharing
rather than imposing disjointness on every matrix view.

Nine corrupted address/image controls are rejected, including a missing reset,
wrong width/code/flags/mode, merged directional scratch slices, undersized
metadata, a pointer subread and an unwritten-padding read. The first analyzer
drafts rejected two valid dataflow forms: a byte read from a defined integer
stack slot, and LEA with its symbolic pointer in the index register at scale1.
Those transfer rules are now explicit. The first independent store-count formula
also missed the extra overwrite of each scratch-pointer field; the corrected
counts are296/344 and the failed drafts are preserved.

The live/disjoint object and helper-input conditions remain assumptions at
this boundary. Complete caller argument flow, allocation origin, matrix import,
conversion aliases and retained graph-header preservation still need proof.
No runtime V120 predicate or application behavior changes. The31-item frozen
manifest `root-ctc-matrix-metadata-dataflow-frozen-manifest-20261023.json`, SHA256
`8bcef6d24929bcb9ce2e9589680c48c1f31f39602841d2212094e3907bf58f11`,
retains the auditors, reports, controls, failed drafts and byte dependencies.

`audit_english_ctc_matrix_blob_dataflow.py` extends that composition through
the complete761-byte/222-instruction `193630` helper. Its48 declared-input
cases bind quantized matrices and optional recurrent vectors to symbolic blob
offsets, preserving the initializer's shared scratch views. Every read/write
stays in the initialized448-byte metadata object; neither the blob nor scratch
payload is dereferenced. The independent final-field image matches all420
defined bytes, leaving28 padding bytes unclaimed. Eight corrupted row/layout/
mode/alias/bounds/read controls are rejected.

A separate arithmetic cross-check uses the serialized alignment8. The projected
three incoming/recurrent layer footprints plus the output footprint total
2,007,776 bytes, exactly the raw payload length after its nine FF padding bytes.
This equality does not prove native role ordering, caller arguments, actual
allocation separation or preservation of retained graph headers. Those flags
remain false, and the application's CTC path is unchanged. The36-item frozen
manifest `root-ctc-matrix-blob-dataflow-frozen-manifest-20261023.json`, SHA256
`7918917440dfea2af683664aa7c8ade3f637849f1798084e21ab5b8d2a7d5ac0`,
retains both composed reports, auditors, controls and dependencies.

## Selected106 initial-header projection

`audit_english_ctc_selected_header.py` checks the complete8980-byte selected
reader and the raw resource's initial fields. Its conditional projection has
five headers, not four: version106 increments the raw layer count and appends
the separately serialized output. The width lane is13/127/127/127/182, the kind
lane1/8/8/8/1 and all five flags are zero. The joint seven-lane allocation has
six entries per lane, including a spare, for168 bytes. The output kind1 does
not take the later kind4 header-splitting branch.

This projection assumes successful stream reads and covers initial header
fields only. The appended output quantization code remains unknown; it is
reported as NULL rather than guessed from the four raw codes2. Complete packet
construction, retained-header preservation, allocator separation and matrix-copy
aliases remain open. No SDK executes and no runtime predicate is approved.

The positive audit and four byte-integrity refusal controls are retained in
the nine-item `root-ctc-selected-header-frozen-manifest-20261023.json`, SHA256
`5fb3d8cb503e961071ae0fe2a06e3e12ae2f8f4d7590f264817a015f8cd3e253`.
Those refusals test input integrity, not independent semantic mutation coverage.

The later `audit_english_ctc_header_quantization.py` checks the complete1352-byte
constructor and its shifted quantization loop. Given the projected five-header
packet and successful construction, graph.count becomes4. Source code indices
0..3 are written to header indices1..4 at byte offsets120/200/280/360. The
appended unknown code slot4 is not read by this loop. All four consumed codes
are2. The proof checks the header-stride register's complete write set before
the loop; it does not establish live/disjoint caller objects or preservation
through later constructor/loader calls. Whole-constructor dataflow and V120
remain unapproved. Nine-item frozen manifest
`root-ctc-header-quantization-frozen-manifest-20261023.json`, SHA256
`2a4c826f5c09d11a1367e2da8613301913b0a8b3e95f9e5c70214c5d8d1c3276`,
retains the auditors, native body exports and report without SDK execution.

## Conditional whole-graph blob binding

`audit_english_ctc_graph_blob_dataflow.py` composes the complete1833-byte/
485-instruction `197DE0` graph walk with the complete `193630` binder. Eight
declared cases cover alignment8/16 and present/absent blob and scratch storage.
The twelve incoming/recurrent calls, their order, row counts, quantization codes,
and blob addresses match an independent field-image calculation. The output
matrix follows the separately traced inline branch. The resulting sizes are
2,007,776 and2,041,920 bytes. All5100 defined metadata bytes match; header and
graph fields remain unchanged. Unknown header padding and blob/scratch payload
reads are refused. Nine corrupted count/kind/alias/jump/write/read controls fail.

This establishes role order only for the declared initialized, live, disjoint
objects. Their actual allocator and caller origins remain unproven; the complete
loader alias proof and runtime predicate remain unapproved. No SDK code executes
and no application behavior changes. The53-item frozen manifest
`root-ctc-graph-blob-frozen-manifest-20261023.json`, SHA256
`75bc2c60a2678817454defb44c929dafd7bb81596d2a5f35d87f7d017897099e`,
retains the composed auditors, positive/negative reports and failed prototypes.
