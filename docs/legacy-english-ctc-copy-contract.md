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
