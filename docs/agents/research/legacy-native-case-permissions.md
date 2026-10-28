# Stored case permissions and case-path version

`LegacyWordLanguageModel.CasePathVersion` preserves the previously skipped
serialized short assigned to native language field150. The actual Russian
language model stores `0x200`. `13B+88` owns the language object, its virtual38
is `0F3360`, and `20C9D0` passes that value to the wrapper292 constructor.
`232090` accepts 0 and `0x200` and refuses other case-path versions.

`LegacyNativeCasePermissions` restores only the stored-triplet phase of
`234840`. It uses the raw six-byte triplets, without accessor repair or Unicode
conversion. Writes execute in storage order: second and third propagate to
first; third propagates to second; TCM200 additionally propagates the updated
first back to second. Byte values and chains across triplets are retained.
This is a single native pass, not an iterative transitive closure. The caller's
input is copied and its exact alphabet extent is validated.

An independent Python oracle models ordered directed edge writes over 433
actual/prepared alphabets and 11,258 masks, both case-path versions. It includes
random byte values, individual permission bits, empty/full masks, overlapping
triplets and missing entries. Managed and Native AOT checks compare full masks
and input snapshots, and verify extent, cancellation and unknown-version
refusals. The static audit pins DLL/resource hashes and the actual version
lineage.

The native method subsequently calls the inner source virtual68. That
dictionary phase, full wrapper292 queries, mutable alphabet preparation and
the owning factory remain separate. This phase alone is not evidence of a
complete Russian recognizer.
