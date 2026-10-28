# Default DOM floating text: static next-dependency packet

Subsequent work implemented the [bounded compositor and zero/special branches](../../../../docs/agents/research/math-dom-floating-layout-20261028.md).
The observations and proposed finite generator below remain the original static
packet. Nonzero finite dtoa is still not implemented or admitted.

The later [mode2 finite generator](../../../../docs/agents/research/math-dom-mode2-digits-20261028.md)
now closes that finite boundary for precision9/17 at the actual DOM caller.
The paragraphs below describe the earlier static-only packet, not current
production admission.

This packet contains documentation and export requests only. No floating
formatter was implemented, no production source changed, and no vendor DLL
was executed. The production floating service boundary remains necessary.

## Provenance and completed checks

Source: `research/myscript/sdk/build/x64/MyScriptEngine.dll`, SHA256
`d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c`.
Default global90 service37 table20F290+C0 points to18008AFD0.
Document0078D0 supplies50 output bytes, significant precision17 for binary64
or9 after binary32-to-binary64 promotion, and final parameter-1.

Read-only Ghidra12.1.4/JDK25 exports used retained project `MyScriptEngine`
and program `MyScriptEngine.dll`. The batch tool verified project stability,
hashes and every exported instruction byte against the PE. Local artifacts:

| Export | Functions | Result |
| --- | --- | --- |
| `.artifacts/dom-formatter-valid-20261028-export` | 08AE70,08AEF0,08AFD0 | passed,14.01s |
| `.artifacts/dom-dtoa-20261028-export` | 08BBD0 | passed,14.78s |
| `.artifacts/dom-dtoa-helpers-20261028-export` | 08E990,08E9F0,08F6F0,1C10A0,1CDE40,1CDE50 | passed,13.82s |

`request.json` and `helper-request.json` reproduce the latter batches using
the documented [batch export tool](../../../../docs/agents/tooling/batch-function-export.md).
These reports establish static export integrity, not runtime equivalence.

## Facts read directly from native instructions

C0 calls08BBD0 with XMM0=input double, EDX=2 (mode), R8D=precision,
R9=&decimalPosition, stack20=&sign, stack28=local digit buffer,
stack30=2500. Evidence:08B017..08B04E. The helper's decompiled first argument
is incorrectly typed as a pointer; its input is the IEEE double in XMM0.

The final caller parameter-1 takes08B0A8 directly to08B114. Thus the
fractional-place-limit rerun through08B0E5 is unnecessary for this caller.
C0 initially sets digit count `n=min(strlen(digits), precision)`.

08B080..08B0A3 selects notation using decimalPosition `dp`:

1. Start exponent `e=0`.
2. Only when `dp<0` or `n<dp`, set `e=dp-1`.
3. If `-3<=e<=5`, reset `e=0` and preserve `dp`; otherwise set `dp=1`.

This is not a conventional fixed exponent threshold independent of the
returned digit count. For example, supplied helper tuples `("1",17)` and
`("10000000000000002",17)` select scientific and fixed notation respectively.
These are compositor examples, not observed vendor executions.

When `e=0`, C0 emits the sign and either:

- `dp<=0`: `0.`, then `-dp` zeros, then `n` supplied characters;
- `0<dp<n`: first `dp` characters, `.`, then the remainder;
- `dp>=n`: supplied characters, then zeros to reach `dp` positions.

With `e!=0`, `dp=1`, the same mantissa emission is followed by lowercase`e`,
an optional minus sign and unpadded decimal exponent digits. No plus sign is
emitted for a positive exponent. C0 writes a final NUL and returns count
excluding NUL. Evidence:08B19F..08B2F3.

There is additional buffer-shortage logic and a secondary last-digit
increment at08B186..08B19F. The latter requires
`precision < original_strlen-1`, which a valid mode2 digit-count contract
would make unreachable at9/17; special literals below are shorter too.
Do not port these branches away until the helper length contract is closed.

The helper08BBD0 extracts the original sign bit before clearing it
(08BC7C..08BC99), including zero and NaN. For zero it supplies digits`0`
and decimalPosition1. It supplies decimalPosition9999 for exponent7FF;
nonzero payload selects`NaN`, otherwise`Infinity`.

Direct PE section-mapped literal reads confirmed:

| VA | Hex bytes | String |
| --- | --- | --- |
| 18020F570 | 496e66696e69747900 | Infinity plus NUL |
| 18020F57C | 4e614e00 | NaN plus NUL |
| 18020F580 | 3000 | 0 plus NUL |

The helper's zero/special outputs therefore have a closed static wrapper
trace. `0,dp1,n1,e0` emits`0`; a set sign bit adds`-`, preserving negative
zero. `Infinity,dp9999,n8` sets`e9998,dp1`, emits`I.nfinity`, then`e9998`:
14 bytes, or15 with a minus sign. `NaN,dp9999,n3` likewise emits`N.aNe9998`:
9 bytes, or10 with a minus sign. The50-byte buffer admits all of these; the
secondary increment is skipped at precision9 and17. These unusual strings
are **static predictions**, with exact branch/literal evidence; they have
not been observed by executing a vendor binary. They should be explicit
future compatibility cases, never normalized silently to ordinary NaN/Inf.

## Helper structure and supported conclusions

08BBD0 contains mode normalization0..9, binary-to-bigint conversion08E9F0,
decimal exponent estimation, a floating quick path and bigint fallback.
Mode2 clears the left/right stopping mode and uses at least one requested
digit; see08C004..08C025 and the corresponding decompilation case2.
The quick path is gated by requested digits<15 and1C10A0()==1. Therefore
precision17 bypasses that path; precision9 may enter it. A separate small
integer floating path also exists and must not be confused with that gate.

Nearest-even tie handling is explicit in the bigint final rounding:
08CBBC compares twice the remainder with the denominator,08CBC3 rounds up
when greater, and08CBCB..08CBCF rounds exact ties up only when the last
digit is odd.08CBD5..08CBDE removes trailing zeros on the other branch;
08CB11 handles carry through nines. The small integer path also checks an
exact half and the last digit's parity. These facts support a mode2
round-to-nearest-even significant-digit contract; they do not yet prove
every fast-path result matches a new mathematical implementation.

1C10A0 maps CRT rounding flags0,100,200,300 to1,3,2,0.
1CDE40 masks300;1CDE50 reads MXCSR rounding bits6000 and converts them to
those CRT flags. Thus1C10A0 is an active rounding-environment query, not a
constant. A future bounded port should explicitly establish its supported
rounding environment. No equivalence under arbitrary altered MXCSR, FTZ or
DAZ settings is established here.

## Attribution inference and smallest next implementation

The expression
`(normalized-1.5)*0.289529654602168 + 0.1760912590558 + binaryExponent*0.301029995663981`,
the mode switch0..9 and the bigint/quick-path structure are strong fingerprints
of David Gay's dtoa family. This is an attribution **inference** from the local
binary, not a verified source-version/license match. No specific published
revision was fetched or compared in this static-only task. An unrelated dtoa,
Ryu shortest conversion or `.NET ToString` is not established as equivalent.

The smallest candidate avoids porting all modes and allocator machinery:

1. Admit only caller scope: mode2,precision9/17,parameter-1,capacity50,
   default supported IEEE rounding environment. Keep signbit and special cases.
2. Decode the exact finite IEEE value as integer significand times2^exponent,
   using BigInteger numerator/denominator. Include subnormal binary64 values;
   binary32 input has already been promoted exactly by Document0078D0.
3. Determine decimal exponent with exact integer comparisons to powers10,
   not a floating log. Round the exact scaled rational to9/17 significant
   decimal digits using quotient/remainder and ties-to-even. Normalize a carry
   into the next decimal position and remove trailing zeros while retaining
   decimalPosition. This is a proposed mathematical implementation, not a port
   already demonstrated equivalent to the native quick paths.
4. Apply the exact C0 compositor above. Under the candidate mode2 contract,
   finite outputs need at most24 bytes excluding NUL:17 significant digits,
   optional sign and decimal point,`e`, exponent sign and three digits. Small
   fixed values and special outputs also fit50. Finish a formal bound before
   excluding native retry/short-buffer behavior from the implementation.
5. First freeze an independent exact-rational oracle and adversarial IEEE
   fixtures: signbit zero, NaN payload/signs, infinities, subnormal/normal
   transition, decimal boundaries, exact decimal halfway ties, carry through
   nines, trailing-zero removal, notation boundaries and float promotion.
   Separately close native mode2 helper equivalence by a source-identified
   reference or an audited independent transliteration of the relevant native
   branches. Comparing two copies of the proposed BigInteger algorithm would
   not establish that missing native equivalence.

No runtime controls or floating implementation are included in this packet.
Production should continue requiring its explicit floating formatter service
until those obligations are met.
