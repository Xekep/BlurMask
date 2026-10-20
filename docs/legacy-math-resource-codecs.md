# Older Math resources: codec ownership and actual grammar root

This static batch proves the concrete class-service codec lookup for the actual older Math files and independently decodes the complete **root** grammar schema. It adds a read-only Python reference and auditor, not a C# recognizer, a grammar evaluator or an app switch. Fifteen complete contiguous Ghidra bodies, eight explicitly labelled exact leaves/thunk, 2,426 instruction locations and fifteen data bindings are pinned. The preceding search/storage/public/modern-route evidence is composed. Vendor modules were never loaded or executed.

## These existing files match the required configured classes

| Actual repository file | Exact bytes | SHA-256 | Framed native class / version |
| --- | --- | --- | --- |
| `models/math/math-ak.res` | 3,685,881 | `8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1` | 1389 / 0109 |
| `models/math/math-grm-standard.res` | 6,637 | `adc9b7adce2d3af1a20a6262467afc0b1c6365bf27c120596d80c86ec54893f8` | 138A / 0100 |

Both have the actual 16-byte framed header (magic, length, class, flags and version), consume the whole file and have flags zero. Math's real class metadata derives 1389 from 1F47 and 138A from 1F48. The prior 0065A0 proof accepts exactly those base types into the **outer processor's** required +130/+188 owners. That is a positive resource/class match, not a filename-based guess or a mapping to Math2 EE4 labels. The newer `models/math2` resources are a distinct route.

## Exact class-service 0011 resolution

The checked Engine registration data supplies the type API method table at 1DB830 and the object API table at 1DB9C0. Type API V60 is the exact 037FA0 adjusting tail thunk: subtract 0x10 from the class-handle argument and tail-jump to complete function 0386C0. It is labelled as a thunk, not a complete Ghidra function.

0386C0 asks the actual class metadata's +30 override when present, otherwise its +18 callback, for the requested service. It returns the first nonzero result, treats sentinel 2 as no service, and otherwise follows the actual base-class registry while its guards permit. It does not assume that all classes' adjacent primary slots implement serialization. This batch uses the actual request ID 0011 only; Boolean/interface-support results for other IDs are not interpreted as codec tables.

| Actual class path | Native callback for service 0011 | Actual returned table | Reader / version / writer |
| --- | --- | --- | --- |
| Math 1389 | 033A20 | 09A378 | 02D9A0 / 0309A0 / 0309B0 |
| Base 1F47 used by the AK reader | 00BBC0 in MyScript2D | 088378 | 00B5C0 / 00B890 / 00B8A0 |
| Math 138A → base 1F48 | own 053C50 refuses 0011; base 02B230 accepts it | 088B78 | 0250C0 / 00C580 / 025450 |

The own callbacks, base metadata, returned pointers and function tables are pinned separately. The actual version leaves return 0109, 0104 and 0100 respectively. 1389's reader has native version branches and delegates to the real base 1F47 codec with the base version carried in the payload when required. Its complete reader and writer are each approximately 12 KiB, not a small header shim. No claim is made that all of that AK payload or its learned operators has been independently implemented by this batch.

The base 1F47 reader creates its actual nested alphabet/object first, reads and owns the two version-dependent object vectors, invokes their initialization hooks when present and obtains the final retained object. Its destructor frees those owned references and buffers. The derived 1389 constructor/destructor and full reader/writer are pinned. Nested resource/model semantics and all native allocation-failure behavior are still separate work.

## The grammar object used during search is a clone

The prior boundary conservatively called processor +190 a derived service. The complete Object API V18 implementation 0399F0 establishes a more precise owner: it clones the supplied object by its actual type and its registered clone constructor; serialization/reload is only a fallback when that clone constructor is absent. 0065A0 releases the previous object at +190 and stores a clone of the actual configured grammar at +188. It is an owned grammar object, not merely a borrowed function table.

The actual Math 138A metadata names clone constructor 047BA0; it delegates the base 1F48 copy and resets its own working caches. Consequently the subsequent search invokes that derived grammar object's primary methods. The base-copy implementation, all transitive child sharing and the data-producing search internals are not called complete here. A future independent implementation must distinguish immutable shared resource data from per-invocation mutable grammar state and clean each owned clone once.

## Complete root schema, explicitly opaque children

Complete MyScript2D reader 0250C0 and writer 025450 define this root payload after the framed header:

1. A 32-bit rule count.
2. For each ordered rule: a one-byte tag, a 32-bit byte length and name bytes, a 32-bit child count, then that many framed objects read by the actual object loader.
3. A trailing 32-bit scalar stored at owner +38.

The native record stride is 0x80; string and child buffers have independent owned counts/capacities and are initialized before population. The reader can retain partially filled state on failure. The root destructor 024F30 releases stored names, child object references and arrays. These boundaries do not imply full rollback of a partially loaded native object.

`math_legacy_grammar_root_reference.py` independently reads that exact root order and rewrites it. It treats each framed child as an owned **opaque byte block with its full header**, checking bounds and preserving its bytes. Analysis-tool budgets and stricter header admission are explicitly library/reference policy, not native malformed-input equivalence.

On the actual grammar file the reference reads 26 ordered root records and 53 framed children, obtains trailing scalar 16, consumes all 6,637 bytes, and recreates the original file byte for byte. The children span eighteen actual class IDs, including 13A8/13A9 and the other mathematical pattern types. These are not all one terminal type. Root names include the actual square-root, pair/list, table and fraction-related entries; those names are resource data, not inferred recognition labels or a claim of solved expressions.

The auditor also refuses six deliberately malformed analysis inputs: an incomplete root header, extra trailing byte, missing tail, foreign root class, excessive rule count and corrupted actual first-child framing. This does not replace decoding or verifying the 53 child payloads. The opaque children are visible in the report with exact offsets, class/version and hashes, so the remaining semantic work is not hidden behind a “complete grammar” label.

## What this enables and what remains open

The actual file/class/codec relationships now support the next data-producing analysis: configured older resources → owned grammar clone → search → actual typed candidate/result. Separate exports have already identified the search wrapper and real grammar output slot, but those algorithms and the original raw/stroke request producer are not transferred by this root-schema reference. The caller's selected C20/C22/DAF sources cannot be replaced with lookalike arrays or preselected expected labels.

Still open are the full AK payload/operator closure, all grammar child classes and matching rules, actual request/source/timing admission, candidate construction/selection and the final 13EC result-to-typed-storage ownership. No raw modern EE4-to-older-grammar bridge, native confidence calibration, font-size/baseline estimator, Russian recognition completion or production app switch is claimed. The existing cold first-record structural adapter remains independently named glue with uncalibrated admission.

Run `python -I tools/handwriting/audit_math_legacy_resource_codecs.py REPORT.json --native-repo SDK_REPOSITORY`. The report includes the actual resource headers, complete root reconstruction, strict analysis controls and preceding search dependencies. `math_legacy_grammar_root_reference.py RESOURCE REPORT` can produce the root-only report separately without vendor binaries. All native extents/tables are exact static evidence; semantic interpretation still depends on the reviewed data flow and does not establish unexamined indirect paths.
