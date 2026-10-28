# Older Math grammar: all actual serialized children

This static milestone decodes every child payload in the existing 6,637-byte `models/math/math-grm-standard.res`. It adds an independent Python reference, a static auditor and this description. It does not add a C# matcher, recognize strokes, calibrate confidence, produce candidates or switch the app to another engine. Vendor libraries were read as bytes, never loaded or executed.

The preceding resource-codec milestone establishes Engine service-0011 lookup and the root grammar reader. Its reference deliberately retained 53 child frames as opaque blocks. The new reference replaces that remaining opaque **serialization** boundary for this actual file: 26 rules, 53 children, 18 actual classes, 297 terminal-set byte strings and 205 recursive prefix nodes are decoded and independently reconstructed. The full file is reproduced byte for byte. Fields without proved semantic names remain labelled by native storage offsets.

## Actual child dispatch

All eighteen child classes derive from registered type 13A7. Their actual class metadata has no own service callback; the base's 02BAF0 exact leaf returns codec table 088D90 only for service 0011. That table is reader 02B910, version 00C580 and writer 02BA00. Complete Engine 0386C0 in the preceding proof supplies the actual base traversal; this is not an assumption that adjacent slots always mean codecs.

The base reader reads a 32-bit name-byte length and that many bytes, then a separate 16-bit **inner payload version**, and invokes the concrete object's V40 reader with that version. The base writer emits name length/bytes, obtains the concrete V48 version and invokes V50 writer. The eighteen concrete primary tables and their descriptor/metadata paths are pinned separately.

Every child frame in the actual file has outer framed version 0100. This does **not** mean every concrete payload is version 0100: 13AA uses inner 0101, 13E2 uses 0103 and 145B uses 0101. Other actual inner versions are 0100. The independent reference admits those exact observed class/version combinations; broader old-version compatibility is not implemented or claimed.

| Class | Concrete reader / writer | Actual inner version | Serialized fields after the name/version envelope |
| --- | --- | --- | --- |
| 13A8 | 2D 02BE60 / 02C010 | 0100 | count, then ordered length/byte strings |
| 13A9 | 2D 02B430 / 02B470 | 0100 | u32 at +18 |
| 13AA | Math 057A80 / 022A70 | 0101 | u32 +18,+1C; u8 +20,+21,+22 |
| 13AB | Math 05D410 / 05D490 | 0100 | u8 +18; recursive prefix-node vector at +20 |
| 13AC, 13B0 | Math 055AB0 / 055BF0 | 0100 | u32 +18,+1C,+20; u8 +24,+25,+26 |
| 13AD,13AE,13AF,13E3 | Math 01D190 / 01D270 | 0100 | u32 +18,+1C; u8 +20,+21 |
| 13B1,13B5 | Math 022960 / 022A70 | 0100 | u32 +18,+1C; u8 +20,+21,+22 |
| 13B2 | Math 023B70 / 023CB0 | 0100 | u32 +18,+1C; u8 +20,+21,+22,+23 |
| 13B3 | Math 0646B0 / 064820 | 0100 | u32 +18,+1C,+20; u8 +24,+25,+26,+27 |
| 13E2 | Math 07BE30 / 022A70 | 0103 | u32 +18,+1C; u8 +20,+21,+22 |
| 145B | Math 0184F0 / 0185D0 | 0101 | u32 +18,+1C; u8 +20 |
| 145C | Math 013E50 / 013EE0 | 0100 | u32 +18,+1C |
| 145D | Math 011EB0 / 011FF0 | 0100 | u32 +18,+20,+1C; u8 +24,+26,+25, in that exact order |

The reordered 145D fields are deliberate. Treating its final bytes as an adjacent native-memory dump would be wrong. The version branches also matter: 13AA's old version omits +22 and initializes it to zero; 13E2's 0101 reads a discarded byte whereas 0103 writes +22; 145B's 0100 omits +20. Those older layouts were inspected but are outside this actual-file reference's admission.

## Prefix-node and set storage

13A8 uses a vector of 16-byte string headers, each holding owned bytes and length/capacity. The payload is a u32 count followed by u32 byte length and bytes for each entry. The largest actual first set has 154 entries, including ASCII and multibyte mathematical symbols. Bytes are preserved directly; no invented Unicode normalization or alphabet remapping is applied.

13AB's 05D410 reads the object byte at +18 and calls complete recursive helper 05CCC0. Each vector starts with a u32 count. Each 40-byte native node stores length/bytes, two serialized bytes at +12/+13, and another recursive vector at +18. The reader sets its unpersisted +10 word to FFFF. Writer 05CF20 emits the same recursive order and excludes that runtime-only word. The two node-byte meanings and the matcher that resolves them are not inferred from their values. The bytes contain familiar function/unit prefixes, but that alone does not prove their recognition, terminal acceptance or expression evaluation.

The reference preserves raw name/string bytes, explicit scalar widths and recursive ordering. It rebuilds all child frames from decoded fields rather than retaining their original opaque payloads. Root reconstruction still uses the previously reviewed root writer. Parsing all 53 children and comparing the entire 6,637-byte reconstruction verifies that no payload bytes remain unaccounted for. This is independent format reconstruction against actual data, not a vendor execution/parity test.

Native readers allocate and append as they read; failed prefixes can retain names, partially filled vectors or child nodes. This reference returns a detached Python value only after complete parsing and uses strict bounds, depth 64 and whole-node budgets. Its malformed-input refusal and cleanup therefore have deliberately narrower analysis-tool semantics, not native allocation-failure equivalence.

## Verification and remaining work

`audit_math_legacy_grammar_children.py` pins 29 complete Ghidra function bodies/regions, three explicitly labelled exact leaves, 565 instruction locations and 57 actual data bindings. All eighteen descriptor/base paths and payload reader/version/writer triples are checked against exact module hashes. The composed preceding audit verifies Engine codec lookup, root schema, grammar clone/search admission and earlier result-storage boundaries.

The auditor decodes the actual complete resource and checks its SHA-256, all counts and exact reconstruction. It rejects 214 strict analysis controls: truncation, trailing bytes, wrong inner version and excessive name length for each of the 53 actual children, plus excessive recursive node count and depth. These controls are analysis admission checks rather than claims about native errors.

Run `python -I tools/handwriting/audit_math_legacy_grammar_children.py REPORT.json --native-repo SDK_REPOSITORY`. The standalone reference accepts `RESOURCE REPORT` and needs only the preceding root reference alongside it.

Still open are the operators and learned contents of the 1389 AK resource, grammar-node matching and search/scoring, actual request/source/timing admission, the full candidate-producing tree expansion and the result consumer join. The concrete 02A600 → Math V90/V98 factories → 138D/138E → 028B10/028640 fill path has been exported for the next review, but is not transferred by this serialization milestone. No raw modern EE4 bridge, native font-size model, Russian recognition completion or full engine completion is claimed.
