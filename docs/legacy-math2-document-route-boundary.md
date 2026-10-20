# Modern Math2 output and the typed Math document routes

This is a static representation-boundary proof, not another recognizer, a recovered raw-markup parser, or application admission. It adds no runtime code. The SDK DLLs were only read. The new audit pins sixteen complete contiguous Ghidra bodies in MyScriptDocument.dll, 1,359 instruction locations, 51 strings, and seven data regions. It composes the existing seventeen-body typed-MyScriptMath audit. These are body extents exported by Ghidra, not the first unwind record of a larger function.

The conclusion is narrower than “the SDK has no bridge.” The complete recovered fresh modern producer does not turn EE4 markup into registered 138D objects. A separate Document event grammar does build mathematical DOM nodes, but its input is typed events with owned payloads and selections. No actual edge from the modern producer into that grammar has been established. Indirect callbacks outside the audited boundaries remain an open call-chain question.

## Three distinct positive boundaries

| Boundary | Actual input | Actual output | What it does not establish |
| --- | --- | --- | --- |
| Document 027E20 / 028070 | Actual first AL alternative C-string and source ranges | Modern `sequence/symbol` text plus fallback terminal/segment DOM | Parsing `<sup>` / `<frac>` into a typed native expression |
| Math 00ADE0 dispatch | Registered object IDs 138D through 1393 | Terminal, nonterminal, rule, cell, border and table DOM writers | Construction of those native typed objects from EE4 strings |
| Document ED9 → EDA | START/END events, native interned character/name values, retained selection payloads | Reduction-built mathematical DOM and pending CB8 edits | Direct consumption of EE4 token bytes |

The first route is already represented by the bounded fresh publisher and outgoing/original-source adaptations. The typed Math dispatcher is separately documented in `legacy-math2-semantic-adapter-proposal.md`. Its positive type dispatch is not a lookup table that can be applied to arbitrary neural labels.

## Modern producer and the misleading SK branch

Document 027C20 tests derivation from EED and retains that recognizer at owner +28. Otherwise it permits type 13E and retains it at +60; its error names the accepted objects as `SequenceRecognizer or SK`. This SK is the configured knowledge/restriction attachment described in `legacy-math2-restriction-map.md`. It is not evidence that type 13E selects the legacy recognition engine. In particular, the cold path's null SK must not be replaced by a made-up legacy parser.

Document 027E20 attaches a text node containing the actual first alternative C-string. Document 028070 appends those symbols to `sequence`. Its fallback contains `symbolSegmentList`, a `symbolCandidate` character `?`, a `mathTerminalNode` with empty name, full INPUT ref/range, selected index zero and pending `MATH_NODE` registration. The optional transforms and later patch operations do not constitute a semantic markup parser. A complete sequence may therefore contain a neural label such as `<sup>` as a symbol string while its fallback terminal still has an empty name.

This modern/legacy **result representation** distinction has direct consumer evidence: Document 109040 and 10A280 require `mathResult/sequence` and report `can not use MathInspector on legacy math result` when it is absent. That explicit refusal is a stronger discriminator than guessing from the unrelated SK attachment. It does not prove which engine originally produced every document accepted by other APIs.

Document 106EA0 is an additional source-selection maintenance boundary: it derives a range list from an existing registry item, updates that selection through C22, and reports whether it became empty. It does not recover a terminal name from AL text.

## ED9 owns a typed event builder, not a second raw-label decoder

The descriptor at Document 208450 is ED9 derived from base 4. Its constructor 10A5A0 accepts constructor sizes 0x10 or 8, retains/resolves the destination document object, creates CB8 for `mathResult`, appends a temporary `symbolSegmentList`, creates EDA, binds the builder as its owner, and creates a string/name owner with empty and `noname` values. The EDA descriptor at 208BD0 derives from DE. Constructor 10C140 initializes the parser stack/counters and deferred payload slots. No neural tensors or AL alternative buffers enter these constructors.

The forty names at 2089F0 describe this event grammar. Tokens 0..19 are `$`, START/END_NON_TERMINAL, START/END_TERMINAL, START/END_TERMINAL_CANDIDATE, CHAR, START/END_RULE, START/END_TABLE, START/END_BORDER, START/END_CELL, START/END_HORIZONTAL_PAIR_SEQUENCE and START/END_VERTICAL_PAIR_SEQUENCE. The remaining entries name grammar reductions such as characters, terminal_candidates, non_terminal, rule, table_item, mixed_tree, horizontal_sequence, vertical_sequence and result. This is a parser protocol, not the model's `<sup>` / `<frac>` vocabulary.

The complete 10BA10 method gives a concrete producer, rather than an assumed correspondence: it allocates 32-byte event records with token kinds 3, 5, 7, 6 and 4 in order. It interns `noname`, supplies a candidate header containing 1.0f bits, interns the supplied character string, and retains the supplied source-selection object in the CHAR payload. Each record is passed to the actual parser's V68. A rejected CHAR event releases that retained payload; rejected records are freed. This method explicitly constructs one terminal around supplied text. It neither classifies strokes nor infers a fraction or exponent from text.

10C3D0 consumes event kind integers, indexes the checked action/goto tables, shifts owned records and calls reductions 10C790. Its malformed-token path reports the unexpected event name and unwinds parser-stack payloads. Its stack writes have an index-below-1,000 guard, but the counter advances even when the write is skipped; this is not evidence of safe overflow recovery, and no independent runtime parser is admitted by this proof. The complete reduction body builds `mathNonTerminalNode`, `mathTerminalNode`, `symbolSegment`, `symbolCandidate`, rules, table/cell/border and sequence structures. For example, terminal reduction 4 uses the START_TERMINAL name payload, END_TERMINAL selected index and accumulated CHAR selection; it writes ref/range and registers pending `MATH_NODE`. Candidate reduction 7 uses the supplied score fields. Names and ranges are event data, not derived by searching an AL C-string. The `noname` fallback in nonterminal reduction 3 is also not a recovered neural terminal-name dictionary.

10BC70 submits event zero and accepts only parser result 1. 10BD30 uses the same completion event before applying generated edits to the actual document: CB8 V10 creates the patch, BCC V228 is checked, then V40, V48 and V50 run in order, and BCC V230 frees the patch. The subsequent INPUT/source-tag installation and observer completion can still fail after DOM operations have started. This boundary does not promise rollback of a partially applied prefix. Destructor 10A890 releases parser/builder-owned resources. Selected helper methods 10A970 and 10AAB0 are pinned alongside these owners; this is not a claim that every ED9 event method has been transferred.

## Consequence for the independent library

The experimental structural adapter is intentionally named library glue. It consumes the actual first cold EE4 output, preserves its UTF-8/ranges/score bits, strictly parses supported structural labels into an owned AST and reports uncalibrated confidence. Its explicit equals-gated solver helper is independent of these native event writers. The existing corpus already proves useful 2D output including a definite integral; no secondary candidate may be selected to match an expected answer.

Recreating ED9/EDA as an independent event API could be a later, separately approved compatibility task. It would require proving all event payload ownership, exact selected indices, source provenance and grammar productions, then testing success and every failure prefix. It would not by itself improve Russian text, calibrate mathematical confidence, recover the 138D object factory or justify changing the application's recognition default. No runtime seam or app switch is proposed by this batch.

## Reproduction and remaining scope

Run `python -I tools/handwriting/audit_math2_document_route_boundary.py REPORT.json --native-repo SDK_REPOSITORY`. The report includes the dependent typed-Math evidence. The DLL SHA and every body/data extent are fixed in the auditor; it imports the existing read-only Binary helper. It never loads or executes vendor code. The semantic interpretations above come from complete decompilation and checked branch/data bindings; byte hashes alone are not an automatic semantic proof.

Unclosed work includes identifying the actual caller that supplies all typed-event/name/selection payloads, any indirect modern-to-typed conversion elsewhere, native structured-result confidence/admission, full original revision/observer semantics, and independent recognition quality for real human expressions. None of those is counted complete here.
