# Math2 first-copy to outer symbol ownership

This read-only checkpoint identifies the next owner after the source-aware
043AC0/043740 first-result copy. It does not implement a DOM writer. The smallest
genuine runtime join is one actual complete `Math2Mode0FirstResultCopy` into a
fresh outer symbol vector and a second independent string library.

## Actual native caller and field contract

029290 calls028A80 at02A3A0. RDX comes from its actual recognition context plus10,
and R8 is its separate external symbol-vector argument. 028A80 iterates the F0
recognition-row vector with stride240, reads each row's symbol pointer98 and
countA0, and copies those40-byte rows through026780. This is the first-copy
record78/80 at F0+20, rather than all168-byte decoder alternatives or their unit
arrays. The copied decoder score, primary/secondary histories and frontier are
not part of the outer40-byte rows.

| Outer symbol field | Producer and ownership |
| --- | --- |
| 00/08 | C21 range pointer/count, cloned by global range V88 at026A05/026A0C |
| 10/18 | Alternative vector/count, each16-byte alternative copied separately |
| Alternative00 | Text pointer re-interned through the destination library V10 |
| Alternative08 | Explicit UTF8 byte count, preserved independently of text |
| Alternative0C | Raw scalar bits preserved by MOVSS; no calibration |
| 20 | Low flag byte copied from the source row |
| 24 | Metadata DWORD copied from the source row |

026690 destroys old row alternatives/ranges, resets count, releases the old
outer library and creates a new3E library. 026780 obtains that exact destination
library from vector+10 at02693B. It never keeps the first-copy library as a
substitute. It reserves and initializes all requested destination rows, then
increments the vector's full count at02687C **before** filling individual rows.
Within each row it copies flags/metadata, interns each alternative, writes its
pointer/count/scalar and increments alternative count, then clones source
ranges. Allocation/copy failure may therefore leave initialized later rows and
a populated prefix. A completed-only portable owner may hide these from normal
reads while retaining explicit diagnostic counters and cleaning them exactly
once; it must not claim whole-record rollback or initialized ABI padding.

The full enclosing02AF50 caller already has a distinct outer library at
local48. After028070 consumes the vector,02AF50 destroys all outer rows through
0265A0, frees vector storage and releases that library. Original DOM/Ink edit
publication occurs inside028070 while these rows remain live. A detached outer
owner must retain its own library and the immutable original document, and can
survive disposal/GC of the first-copy result and decoder. It must not retain
decoder Source, Step, RecordIdentity, event leases, models or the old pool.

## Single source and prepared multi-source distinctions

For exactly one F0 row, native028A80 adds no wrapper symbols and strips no
existing table delimiters. It copies all source rows, including existing
`<table>` and `</table>` intervals. A single empty symbol vector is also a
026780 count-zero no-op after destination initialization.

For multiple F0 rows, the function emits `<table>`, `<\>` separators and
`</table>` using exact byte lengths7/3/8. These synthetic rows have empty source
ranges, flag0, metadataFFFFFFFF and scalar bits0. It removes an individual
source's first/last rows only if both first alternatives match the corresponding
table delimiters **and its symbol count is greater than2**. Two matching
endpoints alone are retained. Matches use the first alternative only, not any
alternative, raw AL symbol ID, case fold or label alias.

The default Engine global90 service37/table20F290 V20=089770 compares these
explicit bounded intervals. It first compares lengths, then uses bounded
byte-ordinal comparison1DA170. The audit pins the default registration and
both bounded-end calls. Host overrides are outside this proof.

Multi-F0 code dereferences first/last rows before copying; it does not establish
a safe general empty-row invariant itself. Null empty alternative pointers also
cannot be treated as ordinary nonempty literal intervals. The current library
has no genuine multi-C23/common-document grouping owner. Independently running
several raw factories makes different documents and cannot supply that owner.
Multi-source selectors here are explicitly prepared examples, not claimed
actual inference or permission to combine independent copies.

Range completeness means every **retained** ordinary symbol row receives its
exact original-source collection. It does not mean all original ink must be
covered by the output. Negative mapped runs already skip attachment upstream;
the multi-source branch intentionally removes matched boundary rows and their
ranges. Synthetic wrapper rows remain empty. No whole-stroke replacement or
union onto wrappers is justified by the native algorithm.

## Remaining DOM/source prerequisites

The existing [symbol DOM transport proof](legacy-math2-symbol-dom-transport.md)
pins028070/027E20. The next reader uses only a symbol's first alternative
pointer for its text node. It serializes the row's same C21 ranges, registers
`MATH_SYMBOL` through the actual CB8, negates that registration value for `tag`,
matches separate32-byte freeze rows, then appends a text node. A registration
can precede a later failure; a fabricated raw-object ID cannot replace it.

028070 creates a temporary parameterized BCC with byte0=0 and the original
name owner. Its CB8 descriptor category is literal0x66 (102). Existing
library textField/native-kind0/category100 and kind4 text-DOM target APIs are
not math-owner substitutes. Math-specific original field/C23 selection, CB8
name state, DAF transforms, freeze state and original DOM/Ink edit receivers
remain prerequisites for that publication. Root owns their next static proof.
Neither outer symbol text nor score is an accepted formula or a layout result.

## Proposed bounded owning runtime

Only after review, add `Math2Mode0OuterSymbolVector.CreateSingle(actualFirstCopy)`
in new files. The entry takes the actual completed typed first-copy owner;
it accepts no caller rows, strings, run endpoints, source IDs or empty-source
flags. It retains exactly the same immutable `Document` and a new independently
owned3E pool. Row identity is the native symbol-row index, not a detached
reconstructed decoder record. The new semantic row type carries ranges,
alternatives, flag and metadata; it should not invent a native raw-unit field.

A narrow first-copy metadata/read callback under its existing gate is needed
to preflight total rows, ranges and interval bytes before copying payload. No
ordinary `CopyRecord()` clone should be allocated merely to inspect extents.
The callback must validate complete/notfailed/notreleased, cancellation and the
same immutable document; it exposes no public mutable array. New target
allocation, initialized-row count and filling stages remain separate. Failures
poison the new target and release its pool once; the source is never reset or
coerced into another owner. No mutation of existing target state is claimed.

Tests should use every real first-copy input, compare all rows/ranges/scalar
bits to independent native copy rules, and verify repeated text aliases inside
the new pool. Force GC after disposing original decoder/source and again after
disposing first-copy. The outer strings and range/document identity must still
work. Budget/cancel guards must run before target payload allocation; injected
faults must expose initialized-count/alternative/range prefixes and reject
normal reads. Prepared multi-source tests can validate selector boundaries
without enabling a multi-source production factory. Old first-copy and full
EE4 captures must remain exact. No application, acceptance, existing-target
rollback or native allocator ABI claim belongs to this stage.

## Static verification

`audit_math2_outer_symbol_owner.py report.json` reads vendor PE bytes only. It
pins8 additional regions,69 instructions,3 default string-service bindings and
3 marker literals, and reruns the existing complete first-copy/DOM dependency
audits. Eight prepared row-selection controls cover single-source preservation,
strict greater-than-two stripping, first-alternative selection and empty
alternative vectors; one explicit multi-empty invariant refusal is kept
separate from native behavior. No vendor DLL is loaded or executed.
