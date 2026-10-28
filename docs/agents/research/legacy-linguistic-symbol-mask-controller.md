# Prepared linguistic source → acoustic symbol mask

`LegacyLinguisticSymbolMaskController` implements the bounded, statically recovered
Text linguistic controller and dictionary enumeration path. It returns an explicit
`LegacyLinguisticSymbolMaskSeed` accepted by `LegacyOrdinarySymbolMaskProducer.Build`.
The source list, dictionary maps and refresh outcome remain explicit inputs. This
does not infer the app's active language configuration or replace similar-looking
Latin letters with Cyrillic letters.

## Static producer/consumer evidence

The examined `MyScriptText.dll` SHA256 is
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.
Only static PE bytes, disassembly and decompilation were used; the vendor DLL was
not loaded or executed.

| Native location | Proven role |
| --- | --- |
| `1802e7c60` / `1802e7cb0` | Concrete class240, parent209; runtime vtable A8 points to206720. |
| `180206720` | Read alphabet count from controller+28; call20d8f0; clear the output mask; enumerate controller+30 source.virtual30 with2066d0. |
| `1802066d0` | Inspect the token's low16 bits. FFE3..FFEA enable the entire mask; an in-range ordinary label enables one byte; other labels are ignored. Returns true. |
| `180236850` | Class28d source.virtual30 enumerates the stored source pointers in order and stops on the first false result. |
| `18023bd10` | Classes290/296/28f source.virtual30 delegate to their enclosed source. |
| `180093b10` / `180093af0` | Class2b7 dictionary source enumerates dictionary.virtual38; callback optionally translates the token's low16 label through source+58 before forwarding it. |
| `1801d6d50` | Classes139/13b dictionary.virtual38 enumerate every label of dictionary.virtual30's lexical alphabet, in increasing label order. This is not a scan of words or reachable trie labels. |
| `180092990` | Build the initial lexical→acoustic ushort map by exact UTF8 lookup, explicit control-token rules and optional normalization fallback; copy the initial map into source+58. |
| `1801b8850` | Existing ordinary producer consumes the callback's Boolean result and seed. Failed or absent controller uses its original all-enabled fallback; later restrictions, virtual stripping and primitive restoration still apply. |

Ghidra infers a void return for206720 and093b10. The machine-code epilogues preserve
AL from the preparation/enumeration call; the consumer tests AL. The original port
uses an explicit success/failure enum rather than interpreting a partial mask as
successful output.

`CreateExactDictionaryMap` intentionally selects the initial map's **normalization
fallback disabled** branch. It preserves lexical virtual labels as FFFF, exact
acoustic IDs and the native special control tokens. It does not reproduce the
conditional global Unicode normalization fallback, dictionary map mutations,
extended source decorators or word scoring. ZWNJ/ZWJ use exact acoustic lookup;
their separate auxiliary map in092990 is outside this API.

The 27 explicit conversions comprise NUL/space/newline/NBSP/replacement-character,
private scalars F0000/F0005/F0006/F0007/F0009/F000A/F000B/F000C,
F0010..F0018 and F0020..F0026. Only F0005 and F0020..F0026 translate into the
FFE3..FFEA enable-all range. High bits on direct callback tokens are discarded;
with a dictionary map, the incoming token's low16 bits index that map first.

Factory1eb340 is bound to resource class20e's initialization table at2e7418. It
chooses class23f for no linguistic sources, class240 for ordinary sources with
configuration byte+30 false, or class288 for the alternate mode. It attaches the
controller using frontend.virtual18, binds its acoustic frontend, and adds all
configured linguistic sources. Direct dataflow from that frontend to the ordinary
recognizer initializer's `param3[6]`/runtime190 pointer is still unresolved. The
full20d8f0 refresh/decorator builder is also not ported. Consequently no automatic
runtime binding or end-to-end recognition equivalence is claimed.

## Actual pinned Russian resources

The optional diagnostic reads acoustic alphabet192 at AK offset99 and lexical
alphabet139 at LK offset607074 in the checked bundled Russian resource versions.
All 139 lexical labels have exact initial conversions:138 acoustic IDs and one
space→FFFB control token. There are no missing lookups, virtual lexical labels or
enable-all tokens, so normalization fallback cannot affect this particular initial
map. The source seed enables138 labels; the existing ordinary producer enables145
primary labels after its proven hyphen/primitive restoration.

Both Latin `a/p/m` and Cyrillic `а/р/м` remain separate, enabled classes. This is
intentional in the observed lexical alphabet, not evidence of a lost script mask.
Language selection and final word/case/category scoring must be established from
their own native producers. The diagnostic does not execute handwriting inference,
measure accuracy or choose a label based on a known answer.

## Independent controls and NativeAOT

The original Python reference uses sets and modular token arithmetic, independently
of the C# implementation.435 controls cover source order, empty sources, false
preparation, failure after partial enumeration, explicit dictionary mapping,
low16 truncation and all enable-all boundaries. Own alphabets exercise27 control
conversions, distinct acoustic/lexical IDs, virtual labels, absent exact lookups,
and a source→ordinary producer→final acoustic class cache path where an excluded
Latin class has the best supplied neural score but remains impossible.

Managed Release and Windows NativeAOT pass the same controls and optional Russian
resource diagnostic. No proprietary executable is needed by the probe. Public API
bounds are original defensive controls:1..65535 acoustic labels, at most256 prepared
sources and1048576 total emitted tokens, checked dictionary indices and cancellation.

```powershell
python tools/LegacyLinguisticMask.Probe/reference_controls.py D:/BlurMask-build-artifacts-20261011/linguistic-mask-controls.json
dotnet run --project tools/LegacyLinguisticMask.Probe -c Release --artifacts-path D:/BlurMask-build-artifacts-20261011/linguistic-mask-managed -- D:/BlurMask-build-artifacts-20261011/linguistic-mask-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res
dotnet publish tools/LegacyLinguisticMask.Probe -c Release -r win-x64 -p:PublishAot=true --artifacts-path D:/BlurMask-build-artifacts-20261011/linguistic-mask-aot -o D:/BlurMask-build-artifacts-20261011/linguistic-mask-native
& D:/BlurMask-build-artifacts-20261011/linguistic-mask-native/LegacyLinguisticMask.Probe.exe D:/BlurMask-build-artifacts-20261011/linguistic-mask-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res
```
