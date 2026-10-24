# Native278 exact-span index correction

Earlier descriptions called142A50 a containing-span lookup. Direct inspection
of the complete455-byte body142A50..142C17 and its signed branches disproves
that interpretation. An existing owner is returned only for identical signed
First and Last. The current successful exact-key dictionary behavior is not
disproved by this native index.

142A80 compares owner Last with the request.142A84 skips lower end groups.
At142AF7, a greater end goes to142A92 and sets the end-group sentinel to the
current node, so142AB1/142AB4 takes allocation. With equal Last,142B14/142B18
walks while First is smaller;142B28 then permits return only when First is
also no greater. Together these signed comparisons require equality. The
other return path142AB6/142ABA has the complementary entry invariant.

Pure linked-node transcription checked5040 insertion orders and176400
lookup/insert/repeat operations against an independent exact-key map. Cases
include nested ranges, shared ends, disjoint ranges and the signed sentinel.
After creating[0,10], requests[2,8] and[2,10] allocate distinct owners; only
[0,10] returns that owner. These are successful abstract pointer-state
controls, not vendor execution or allocator/failure equivalence.

Root separately verified the complete body against the original PE, all21
comparison instruction pins and the pure controls. Frozen evidence manifest
`ru-142a50-exact-index-frozen-manifest-20261024.json` has SHA256
`9fdf4897f73321b864e325d138e0b00903409a0ba1f5870e4e4760c01ea017d5`.
The original MyScriptText PE SHA256 is
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.

This corrects the index premise only. Native13E7F0 first augments all unique
275 owners in TOP/WORD encounter order, then13D360 ranks owners with a carried
quota, then13D000 runs. Calling a completed single-owner augmentation once per
word would change that chronology. Pending-tail/root flush handling and the
whole-chain source/publication lifecycle remain unfinished; no restriction
was removed by this documentation correction.
