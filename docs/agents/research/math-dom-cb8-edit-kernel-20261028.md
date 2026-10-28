# CB8 edit matching: typed C# kernel

The Document032800 callback is now implemented as an internal reusable C#
[semantic kernel](../../../src/InkRecognition.Math/Math2DomCb8EditMatcher.cs).
It consumes the actual typed DOM conversion and mode2 formatter implemented in
the preceding packages. **A real retained nonfresh CB8/Ink/result owner is still
missing. This kernel is not wired into publication or recognition acceptance.**
The tests' owner, range and name handles are explicitly synthetic.

## Native behavior covered

Read the attribute name from envelope kind0/offset10 or kind3/offset30; every
other envelope supplies no name. Only opcode7 with a name equal to the owner's
tag name proceeds. Convert both old and new DOM variants before attempting the
old parse. A failed old parse skips new parsing; failed/nonnegative new parsing
skips the pending list. Negating the new ID wraps as signed64, including
INT64_MIN. The first pending entry with that ID is decisive; no fallback to a
later duplicate is allowed.

Look up the old selection and then old name through the same retained Ink's
services, preserving short-circuit order. There is no invented old-ID sign test.
Compare the pending name via the owner's service. A null pending C22 tests the
actual old range count for zero; a nonnull pending C22 uses the range service.
The callback mutates neither pending entries nor registry IDs.

The independent [earlier native review](../../../tools/handwriting/ports/math-transport-20261028/README.md)
and [request](../../../tools/handwriting/ports/math-transport-20261028/request.json)
are retained. A new read-only batch exported0078D0 and032800 in14.25seconds to
`.artifacts/dom-cb8-matcher-20261028-export`. All309 instructions matched the PE;
the project/tool inputs remained unchanged. Source Document SHA256:
`17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f`.
The vendor engine was never executed.

## C-string parsing differs from numeric attribute reading

032800 calls Engine.V88 with an absent end pointer. It accepts a numeric prefix:
`41tail` becomes41; `-2tail` becomes-2. It also ignores the DOM's declared length
when parsing that returned pointer. Conversely,031E20's string-number branch
requires full bounded consumption and must not be substituted here. Numeric
DOM variants are formatted first: a floating41.5 parses as old ID41; a formatted
`1e16` parses as1. These compatibility cases are tested explicitly.

The managed kernel reads only the storage interval supplied by its future owner
adapter, not guessed adjacent memory. It requires a covered stopping byte after
an all-digit prefix or sign. An absent pointer, empty covered interval, sign with
unknown following byte or all-digit interval without a covered stop refuses
explicitly. It does not turn an unknown native out-of-bounds read into a successful
end-of-span parse. Normal numeric conversion supplies its own50-byte NUL-terminated
scratch. Old and new scratch areas are disjoint and remain live through parsing.

## Verification and owner boundary

The [prepared Python controls](../../../tools/Math2DomValue.Probe/cb8_controls.py)
freeze35 branch decisions and their expected native service/conversion/parser
prefixes before C# compilation. The actual C# library is called with typed values,
including finite floating formatting, wrapping integer text, prefix consumption,
both supported name envelopes, missing services, first duplicates and null C22s.
Every observed service/conversion/parser prefix is separately interrupted; entry
cancellation must perform no work; pending operands must remain unchanged.
These account for291 callback checks in addition to544291 earlier numeric checks.

The final managed probe passed544582 checks. The complete Math library69 authored
files and both probe C# files are bound to their portable PDB/source hashes.
Local evidence: `E:/BlurMask-build-artifacts-20261014/math-dom-cb8-20261028/`.
The Windows Native AOT probe passed the same544582 controls. Its newly reviewed
Math DLL SHA256 is `24e5ee9bb00cab651f2b16f8b2a24e27ab7f67738f1144bd5ef1d9387098c0f6`.
Six actual model/official-ORT fresh publication cases and their independent oracle
also passed (70 failure prefixes,180 guards). Those actual publications remain
opcode2-only and do not exercise a nonfresh opcode7 owner.
The workflow runs these same frozen controls under managed and Native AOT builds
on Windows, Linux and macOS, alongside the independent C numerical reference.

`IMath2DomCb8Services` is an explicit prerequisite, not a reconstructed owner.
Its future adapter must retain the same actual Ink, canonical naming service,
pending-list order, C22 range identities and lifetime locks. The kernel does not
manufacture them from names, arrays, matching IDs or snapshots. Existing fresh
publication still creates only opcode2 actions and never calls this matcher.
Prior-result construction, edit production, transactional application, owning stale-tag
cleanup and original receiver/history acceptance remain unfinished.

## Subsequent ordered stale-tag cleanup

[Math2DomCb8TagCleanup](../../../src/InkRecognition.Math/Math2DomCb8TagCleanup.cs)
now implements the pure0339D0/CB8.V50 decision and service-call loop. It walks
borrowed old/new vectors in order, keeps any old ID present in the new vector,
and removes every other old ID through an explicit retained-Ink service. It
does not deduplicate old IDs, filter signed values, pre-delete a whole batch,
or roll back earlier deletions after a later false return/exception. An empty
old vector succeeds without a removal call. Input vectors remain unchanged.

The same frozen control generator adds10 targeted native cleanup cases and38
decision/cancellation/interrupted-prefix checks. These exercise duplicate old
and new IDs, absence/presence, failures at the first/second removal, signed64
extremes and preservation of already completed deletion prefixes. Synthetic
removal services are expressly not actual registry owners. This additional
algorithm still does not wire a nonfresh owner or authorize real deletion.

The expanded final managed probe passed544620 checks with70 authored Math files
and two probe C# files authenticated through portable PDBs. Its local evidence is
`E:/BlurMask-build-artifacts-20261014/math-dom-cb8-cleanup-20261028/`.
The final Windows Native AOT run passed the same544620 checks. Six real
model/official-ORT fresh publication cases and their independent failure-prefix
oracle passed against this same Math DLL, SHA256
`ce07e4407d0b1df0f811ce278730a522f2f2dce08f669a88764576964ebd87f3`.
The [seven-function owning map](../../../tools/handwriting/ports/cb8-owner-20261028/retained-owner-map.md)
documents the newly verified constructor/state layout, pending-list prepend
order, failed-C22 prefix and remaining concrete owning join.

The focused workflow cancels superseded runs of the same branch and limits
repository checkout to four minutes. This avoids an obsolete or stalled checkout
holding up iterative port verification; all current managed/AOT checks still run.
