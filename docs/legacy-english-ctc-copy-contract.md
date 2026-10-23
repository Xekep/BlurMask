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
