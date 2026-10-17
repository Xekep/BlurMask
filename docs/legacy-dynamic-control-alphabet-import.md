# Dynamic control alphabet and copied result import

`LegacyNativeDynamicControlAlphabet` implements the fresh Engine `2A` IO5 byte
interner (`050F10`, lookup `0511B0`). `LegacyNativeGlyphControlImport` implements
the bounded `13B350`/`139DC0` pool conversion. The owning entry point is
`LegacyFreshMainGlobalControlImport.Prepare(actualSnapshot).Run()`.

This creates a separate copied result lifecycle. It preserves original WORD
storage and reading references and copies **every** pool node, including nodes
not referenced by a positive-span word. Traversal is signed 64-bit node-key
order, followed by record ordinal. It does not mutate the original published
pool, infer a new winner, or emit accepted text.

## Byte owner and allocation prefix

Symbols use exact byte identity; case, normalization and canonical equivalence
are not applied. IDs are insertion ordinals. The hash is unsigned 32-bit
`h = h * 33 ^ byte`, starting at `0x1505`. Native hash storage is represented by
owned semantic entries and allocation-prefix metadata, rather than native
pointers. The symbol vector grows to a multiple of sixteen.

The native leaf starts with value zero. A failed symbol-vector reservation
stores `FFFF` in that new leaf without adding a symbol. Duplicate insertion
then returns `FFFF` without retrying. Earlier allocated hash prefixes survive
failure. A failure before leaf creation leaves the native new-entry stack flag
unwritten; internal allocator controls therefore supply that residue explicitly.
They are prepared failure controls, not a claim about managed allocation faults.

Public insertion validates UTF8 without replacement. A distinct internal path
preserves `139DC0`'s pinned unit-literal byte interval: the literal starts
`C2 A7`, but the native call passes length **one**, hence only `C2`. The import
does not silently repair this into U+00A7. Raw byte lookup/read supports that
native interval. Portable bounds are 65,520 assigned labels, 65,536 leaves,
8 MiB aggregate bytes and 1 MiB per input; the count guard prevents native U16
rounding overflow. Cancellation can retain an already committed interner prefix.

## Import namespaces and supported source

Ordinary kind-zero records use their original primary alphabet text (or exact
native synthetic encoding), become kind two, and receive **control** IDs.
Kind-one runs become kind three and insert their required LF/space/NBSP token;
nonempty subkind one fails with the previous prefix retained. Other kinds and
all unrelated bytes remain unchanged. Failed import is inspectable but cannot
produce accepted text through this API.

The global adapter requires the actual completed, same-owner fresh plain main
snapshot, source kind `135`, no associated source/trace, and source format zero.
It rechecks result leases, original pool bytes, WORD bytes and references. Actual
WORD auxiliary vectors at `58`/`68` must have counts zero at `60`/`70`; other
counts are refused. Their nonempty conversion remains unimplemented.

The fresh alternate byte vector has active count zero. Native `13B5A0` reserves
one spare byte; its unknown contents are omitted, rather than invented as NUL.
Primary alphabet, dynamic control alphabet and alternate storage remain distinct.
**Post-import TOP V50 primary-alphabet pointer binding is not joined.** Converted
control IDs must never be decoded through the acoustic alphabet as a substitute.
General malformed codec conversion, other encodings and nonempty alternate or
WORD auxiliary vectors are outside this bounded implementation.

## Verification

`LegacyControlAlphabetImport.Probe` separates actual owning imports from
prepared numerical/failure controls. It captures 256 insertion traces, 18
allocator-failure traces, an explicit hash-bucket collision, the in-flight leaf
zero-to-FFFF transition, U16 capacity limits, 711 prepared pool imports, four
canceled imports, and eight actual global snapshots. Actual imports retain all
31 pool records and 631 existing NN evaluations; no extra inference is run.
`audit_output.py` independently parses the raw acoustic resource and checks
hash/allocation states, complete 40-byte records, IDs, failure prefixes and
unchanged original WORD storage. It does not use expected transcription labels.

Run the probe with nine arguments: AK resource, engine activation profile,
separation profile, case data, publication data, line30 resource, line50 resource,
output JSON, LK resource. Run the independent audit with `AK capture.json`.
Publish the same probe with `-r win-x64 -p:PublishAot=true` for Native AOT.
All output, build artifact and TEMP/TMP directories must point to the task's
`E:/BlurMask-build-artifacts-20261014` workspace.

`tools/handwriting/audit_dynamic_control_import.py report.json` checks pinned
Engine/Text PE hashes, seven vtable targets, twelve code ranges, nine failure-order
instructions and the unit-literal length/address. It only reads PE bytes and
existing decompilation. The unchanged `LegacyMainGlobalGlyphExecution.Probe`
capture is also rerun in Managed and Native modes as the old-runtime regression.
