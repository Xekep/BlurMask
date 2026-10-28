# Actual global published WORD quote cleanup

`LegacyFreshMainGlobalPublishedQuoteNormalization.Apply` joins a completed
`LegacyFreshMainGlobalFirstAlternativeSnapshot` to the existing21FBF0 kernel.
It retains the same global terminal, current result lease, glyph pool and
published WORD list in native head/Next order. It constructs no old CASE owner.
Two explicit, distinct four-byte scratch slots carry their untouched residue
and scalar results across all words. Only the kernel's ushort08 quote writes
can change glyph records; scalar alternative storage is separate.

The mutation claim belongs to the actual execution session. Capturing another
snapshot cannot restart cleanup. Pure precondition failures leave it usable;
exceptions and cancellation after the claim consume it and retain written
prefixes. Nonempty control descriptors still require the actual distinct2A
owner and explicitly refuse that missing dependency. Imported copies from the
separate control-import lifecycle are not supplied to this adapter.

The existing static audits pin21FBF0's published-list access, common WORD last
reference and successful221840 caller order. The actual global TOP binding is
checked separately by `audit_main_global_result_ownership.py`.

The portable `LegacyMainGlobalQuoteNormalization.Probe` uses the same nine
resource arguments as the global first-alternative probe. Managed and Windows
Native AOT agree exactly, SHA256
`fff3243191a38abf005d9ad5cf7375c4341b8304428145d80bd596b1c49c2b2f`.
The independent raw-AK oracle reconstructs all linked references, scratch bytes
and1,240 full pool bytes for eight actual inputs, including unvisited records;
56 additional guards check scratch, cancellation and session-level reuse.
The512 full alternative-allocation checks and120 original guards also pass.
All631 cached group evaluations remain unchanged.

These actual inputs contain **no synthetic quote output**. Their glyph pools
remain unchanged, while the scalar scratch operation executes. Positive quote
rewrites remain independently covered by the separate prepared kernel tests,
not by an actual acoustic quote fixture. No improved Russian accuracy, enabled
21F320 rewrite, complete273 assembly or accepted text is claimed.

Run the output oracle as:

```powershell
python tools/LegacyMainGlobalQuoteNormalization.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res <capture.json>
```
