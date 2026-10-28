# Original Math grammar configuration

`LegacyMathGrammarResource.LoadActual(grammar, acoustic)` authenticates private
copies of the stored6637-byte standard grammar and the actual Math AK resource.
It loads all26 rules,53 child elements across18 serialized classes, native
scalar fields and the stored trie. Distinct inner versions and native field
orders are preserved, including145D's18/20/1C/24/26/25 order.

Terminal names resolve through the same authenticated AK alphabet:297 stored
terminal entries map to259-label AL. AL has258 strings; virtual label256 has no
string. The immutable configuration exposes ordered terminal labels, native
field offsets and read-only rule/child/trie collections. It does not accept
caller-supplied label maps or acoustic hypotheses.

`WriteSerialized` reconstructs the original stored versions from parsed fields.
It does not return a retained input buffer. Independent Python parsing compares
every rule, child, field, trie and terminal label; reconstructed bytes match the
original file exactly. Prepared controls also cover resource fingerprints,
lengths, cancellation, collection immutability and detached input/output arrays.
The numerical model execution count is zero.

```sh
dotnet run --project tools/LegacyMathGrammar.Probe -c Release -- . capture.json
python -I -B tools/LegacyMathGrammar.Probe/audit_output.py . capture.json audit.json
```

The reference codecs are tracked in `tools/handwriting`; the CI check requires
no vendor DLL, Ghidra installation or ignored research directory. Static native
codec evidence is recorded by `audit_math_legacy_grammar_children.py` there.

This is configuration for the original matcher. The acoustic shortlist,
matching workspace/search and matched-tree issuer are still separate missing
runtime stages. Loading a grammar or resolving a terminal does not establish
that strokes were recognized. See [the producer chain](legacy-math-recognition-frontier.md).
