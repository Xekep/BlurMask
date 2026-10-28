# Global main result ownership: static provenance

The main result uses the TOP273 object constructed by driver157C60, rather
than a separately initialized first-stage result. The driver writes its273
descriptor to the local object at `rsp+B0` and constructs it with the actual
driver220 capture byte. Later it passes that same address as argument3 to
the active controller's V80. The240 descriptor's method table binds V80 to
2271F0. Its reset argument is1 and the two following arguments are0.

2271F0 calls the273 reset operation on this supplied object. It does not
reconstruct a new273 with a different capture argument. The273 reset13BB50
preserves both field41 (capture policy) and field48 (the274 glyph pool owner).
The274 cleanup13ABF0 preserves its capture byte10 on both empty and nonempty
paths. This establishes the connection from the fresh driver's capture0 to
the actual main result. Reused drivers and property overrides remain outside
this fresh raw-input policy.

The source-format policy also follows the actual main controller.20C8C0
queries the selected247 source's V68 (1D62A0, resourceB9) and stores its value
at controller22F9.21FE30 passes that value to the279 publisher. It must not be
inferred from an unrelated first-stage controller's fieldF0. Fresh configuration
C4 is separately cleared by00C860.

`tools/handwriting/audit_main_global_result_ownership.py report.json` checks
six complete pinned native regions,18 exact instruction pins, descriptor/method
bindings and the complete direct273 reset write set. It reads the retained
binary and disassembly without loading or executing the vendor DLL. The
existing word-packet auditor checks the273/274 constructors and WORD reset.

This is a provenance proof for bounded plain result ownership. It does not
establish complete global glyph execution, alternative completion, document
acceptance or handwriting accuracy. Those consumers require their own actual
callback, byte-level and failure-prefix verification.
