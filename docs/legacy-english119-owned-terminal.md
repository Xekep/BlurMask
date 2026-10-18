# Owned English119 FIRST terminal selection

`LegacyOwnedEnglish119Terminal.Select(completedContext)` now consumes the
actual final B8 tables from the English119 selected249 source. Its typed
result retains that exact context, primary, source, graph, confidence/cache
and model lineage. The API accepts no labels, probabilities, masks, source
flags or caller-built terminal grid. It selects a genuine FIRST numeric
terminal and returns its exact source-order predecessor cells.

The implementation uses the existing terminal-selection and glyph-trace
kernels unchanged. Node/arc identities remain candidate graph references;
they are not turned into physical stroke ordinals or source720 ranges. Each
selected cell is checked against its own primary row and graph span. Copies
of the full40-byte cell storage are detached, and returned lists are read
only. Cancellation returns no partially constructed result. An empty list
would report no finite terminal; no fallback word is fabricated.

Invocation policy is proved in [the terminal plan](legacy-english119-terminal-plan.md):
NEW selected249/2B3 with no property events has99=0;16A130 source150 stays0
through this actual24A/24C/265 raw owner and is copied by158970 into A8.
F4=0xA000 and FC=0 are1EDB60 constructor writes. TOP273 constructor/reset
provide initial reference78=0. E6 comes from the same acoustic alphabet.
This scope excludes the external2458B0 temporary99 wrapper, reused sources,
property-configured owners and modern accepted-document routes.

The result's relative cost, mass and acoustic score are distinct native
channels. Actual A4=1 and99/A8clear make the sole selected alternative's
relative cost0 and mass1; neither value is recognition confidence. Existing
portable `Math.Exp` semantics are retained, with no universal CPU-exp
bit-equivalence claim. FIRST's best acoustic/context path is not a main
linguistic winner or published English text.

## Evidence

On124 actual raw English input fixtures from the already audited controller
capture, Managed and fresh Windows Native AOT produce identical1,638,187-byte
captures, SHA256
`8a5c9180ea9522b6cab25b83c83bd491a72ff69cb81f7f04356cce0973d876af`.
There are124 terminal choices,306 selected full40-byte cells,152 retained
word-boundary bits, one selected closed211 cell, and373 ownership/cancel/
immutability guards. No expected words or labels enter inference. These are
algorithm fixtures, not a representative English accuracy corpus.

The independent Python-I oracle reads the original raw247/110 resource,
217-label alphabet, source kind and BA5. It compares all154,000 B8 bytes
against the independently audited context capture, accounting for private
symbol-context identity allocations by their exact symbol tuples. It then
replays signed ranking, trailing-symbol/trait traversal, all scalar F32
operations, reference78 and every40-byte predecessor cell. The actual model
fingerprint is checked against the whole resource manifest and nested source
bytes. No SDK/runtime/research module is imported by this numerical audit.

The production terminal code does not evaluate networks. The probe verifies
identical cached group object references and unchanged score counts before
and after repeated selection. Existing generic terminal/trace controls pass
Managed and fresh Native:512 independent grids/1368 choices, including151
zero identities and481 exp infinities, plus256 causal fixtures/762 cells.
The existing RU blank-context Managed/Native capture is also checked against
its prior exact golden; no old runtime API or numeric kernel is modified.

The static auditor composes the actual invocation proof and pins the full
native1F42D0 function `[1801F42D0,1801F6C32)`, using its Win64 .pdata end,
SHA256`c4422cf488eba5defdbeed9d8d981d442e7e02f6cb53002ce2b9956bbf408b6d`.
Vendor binaries are only read, never loaded or executed.

```powershell
dotnet run --project tools/LegacyEnglish119Terminal.Probe -c Release -- models controller-costs.json terminal.json
python -I tools/LegacyEnglish119Terminal.Probe/audit_output.py models audited-context.json terminal.json
python tools/handwriting/audit_english119_terminal.py static.json
```

Rich273/274, English symbol update and actual WORD276/275 remain the next
boundary. A selected closed cell must follow its genuine native publication
branch; it cannot be replaced by a guessed letter. The existing RU-specific
materializer is not coerced into accepting this English owner.
