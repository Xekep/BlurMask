# Russian selected-source normalization default is native100

The suspected source6120 target-default gap is resolved by positive native evidence. No runtime value needs changing. The current `LegacyFirstAcousticModel.ReadSource` conversion of serialized target0 to100 matches the native21D reader. This does not explain the124 remaining mismatched words in the recorded261 corpus.

## Actual reader, clone and consumer

The pinned Russian resource has ordinary21D/v110 at13082. Bytes13098..13102 are `FF FF 00 00 01`: two normalization flags, zero target word and initial-network presence. This is the source's actual first model, not the later main-model target100 field at189752.

Text descriptor1802E3C80 identifies21D and links constructor/prototype table1802E3C40. That table names runtime constructors1801B0330/1801B0560/1801B0660 through1802E3B68 and class-service getter1801C6960. Getter1C6960 returns1802E3080 for service0x11 andNULL otherwise. Its actual first entry is reader1C2940. These are typed pointer bindings, not address guesses based on nearby function names.

1C2940 clears the prototype parameter span+8..+157, reads one byte each into+8 and+9, and two bytes into+0xA. For version110 (<118) it also initializes score exponent+0xC tofloat1. At1801C2A37 it compares the actual target word with0, branches over the default write if nonzero, and at1801C2A3E writes immediate0x64 into the same word. This is unconditional for the actual zero target after successful reads, not a screen-resolution-dependent heuristic or application configuration.

The normal clone constructor1B0560 calls runtime constructor1B0330 and copies the complete prototype parameter span+8..+157 into the owned runtime at the same offsets; buffer constructor1B0660 accepts an exact150-byte payload and likewise copies it after construction. The ordinary initialization150D00 obtains the selected source's ordinary object through virtual98 and clones it before virtual18 initialization, as the existing source-identifier milestone documents. The class descriptor and clone parameter layout are exact positive evidence; this scoped proof does not reimplement the entire engine object service.

Native1B4E40 consumes the runtime+0xA unsigned target alongside the same+8/+9 flags. The actualFF/FF/nonzero-target branch subtracts candidate minimumX and source baseline from actual prepared points, multiplies by target, divides by source line height and truncates into short coordinates. Its returned normalized baseline is0 and normalized line height is the target. Thus100 is a dimensionless normalized line-height unit, not100 raw pixels or an inferred display DPI. The ratio arithmetic is already covered by the separate prepared-normalization milestone; no alternate target is selected by expected text.

## Complete scoped evidence and limits

`audit_russian_normalization_default.py` verifies the pinned Text binary and RU acoustic resource, all bytes and3765 decoded instructions across seven complete scoped Ghidra functions/eight body regions, two honest exact closed leaves and four typed pointer tables. The full1C2940 body has **two disjoint regions**, including its seven-byte tail. Neither first unwind entry nor a function-name range is advertised as its full body. The two leaves (1B0320 size getter and1C6960 service selector) lack a Ghidra body and are instead pinned only through their actual closed return.

Complete exports for1B0330/1B0560/1B0660/1B4E40 and1C68C0/150D00 were newly obtained headlessly from the read-only existing Ghidra project. Full1C2940 comes from the preceding immutable degenerate-reader export. No vendor DLL was executed or loaded.

This milestone changes no runtime, weights, candidate thresholds, model selection, normalization target or default policy. The preceding runtime repair's document intentionally did not claim this default provenance; this separate positive proof resolves that open question without editing its frozen files. It does not prove every recognition error is a native library mismatch. Coordinate conventions, segmentation, source/model limitations and remaining producer gaps must be assessed independently. Continuous input tracks must not acquire invented pen-up events.

```powershell
python tools/handwriting/audit_russian_normalization_default.py OUTPUT.json --native-repo C:/Users/Xekep/Documents/GitHub/BlurMask
```
