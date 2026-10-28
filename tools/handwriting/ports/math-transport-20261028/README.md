# Math CB8 edit-selection matching, 28 October 2026

The later [typed C# kernel](../../../../docs/agents/research/math-dom-cb8-edit-kernel-20261028.md)
uses the completed DOM conversion service directly and has managed/AOT controls.
The earlier standalone Python kernel and its static evidence are retained below.
Neither implementation establishes the missing nonfresh owning join.

`cb8_edit_match.py` ports the decision and required-service call order of
Document `032800`, the callback supplied by CB8.V10 (`033040`) when matching
old/new tag edits. This operation was previously hashed and instruction-pinned
but had no algorithm implementation. It is a standalone semantic kernel, not
a production owner, DOM writer, inference change or accepted formula route.

The matcher follows a negative new tag through the **ordered** pending list,
looks up the old selection's range and name in the CB8's retained Ink, checks
the pending name, then compares actual ranges. A null pending C22 instead
requires an actual old range count of zero. It must not match by tag number
alone, manufacture an empty range, search past a first duplicate pending ID,
or replace original source identity with a stroke ordinal.

Both typed attribute values are converted before parsing the old value. Failed
old parsing skips new parsing; failed range lookup skips name lookup. Failed
name comparison skips range comparison. The kernel retains these prefixes.
The existing old ID is not subjected to an invented nonnegative-ID test.
Native signed64 NEG wrapping is preserved for INT64_MIN, although normally
generated temporary IDs start at2 and do not reach that value.

## Static evidence

Before requesting exports, the existing inventory was checked at
`E:/BlurMask-build-artifacts-20261014/line-document-caller-all/MyScriptDocument.dll/functions.tsv`:
`0078D0` has592 body addresses; `032800` has614. The batch request is retained
in `request.json`. Fresh read-only export used MyScriptDocumentStage4 and
Ghidra12.1.4/JDK25.0.4.1, completing in22.56 seconds with unchanged project
hashes. Both functions'309 instructions matched the original PE independently.
No vendor DLL was loaded or executed.

Local excluded export:
`research/myscript/output/math-transport-cb8-20261028/`.
Binary SHA256:
`17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f`.
The exported `032800.instructions.tsv` SHA256 is
`a5ffbc3a0f56b9da26d46a4ab3bdfd15eab5ba0377b44ba26a23a30bb9fdbef0`.

Key decisions are pinned by the actual instructions at032850 (kind7),
03288A (attribute-name comparison),0328B4/0328DE (both conversions),
03291A/032956 (old/new parsing),032966 (new negative),032978 (NEG),
032980 (first matching entry),0329D1/0329F1 (same Ink lookups),
032A1E (selection-name comparison),032A48 (range equality) and032A50 (null
pending-range/count-zero branch). Prepared tests derive expected branch and
call traces directly from those instructions, independently of the kernel.

## Remaining exact owning boundary

Production integration still needs the actual CB8 pending list and retained
Ink, and edit-node envelope/name extraction (node kind0 field10 or kind3
field30). `0078D0` performs typed DOM-variant conversion; global90.V88 parses
the converted value in base10. These remain required services, rather than
an invented Python `int()` interpretation of every DOM type. Ink.V128/V130
and the range service.V28 must come from the same retained native-equivalent
owner. Prepared test adapters are expressly not that owner. The callback
itself changes no pending entry and allocates no final selection ID.

Thus this closes one matching algorithm's semantic implementation, but does
not close CB8 construction, tag allocation/resolution, DOM edit application,
multi-C23 grouping or original receiver/history acceptance. No recognition
threshold, beam cap, source budget, application path or existing runtime file
was changed.

## Reproduce

```powershell
& C:/Users/Xekep/AppData/Local/Programs/Python/Python313/python.exe -I -B -m unittest discover -s tools/handwriting/ports/math-transport-20261028 -p test_cb8_edit_match.py -v
& C:/Users/Xekep/AppData/Local/Programs/Python/Python313/python.exe -B tools/handwriting/export_port_batch.py --verify-only --request tools/handwriting/ports/math-transport-20261028/request.json --binary research/myscript/sdk/build/x64/MyScriptDocument.dll --program MyScriptDocument.dll --output research/myscript/output/math-transport-cb8-20261028
```

Verification passed4 test methods with18 independent branch/call-order cases,
plus old-negative-ID, signed64-wrap and immutable-pending controls. This is
prepared algorithm verification, not evidence of handwriting recognition
accuracy or a complete Math2 port.
