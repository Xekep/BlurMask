# Fresh main TOP glyph UTF8 emission

`LegacyFreshMainGlyphUtf8Emission` binds the existing complete scalar/run,
canonical-scratch and circled-writeback kernels to an actual completed fresh
main result. Its input is a `LegacyFreshMainSingleLinePlainRecognitionResult`,
not caller-supplied symbol labels or an acoustic alphabet standing in for a
control alphabet. `Emit` checks the reference and all forty glyph bytes against
the identical current TOP pool before reading its descriptor.

This is a fresh raw/no external text/no associated or reused source scope.
It does not implement TOP virtual30 import, its converted control alphabet,
nonempty external alternate text, the complete document wrapper or OCR
acceptance. It does not improve recognition accuracy by replacing symbols.

## Three distinct native owners

| Owner | Actual producer and consumer |
| --- | --- |
| Primary alphabet | The same source-policy model acoustic alphabet supplied by the fresh main wrapper; `13A0B0` calls its virtual30 for ordinary in-range labels. |
| TOP273+18 control2A | `13B870` creates an Engine type2A. Its `050A20` constructor creates an empty type28 tree (`04D6F0` root8=NULL), count30=0 and mutable38=0. Glyph run-kind3 calls lookup-only `018F70` -> 2A virtual28=`0511B0`, which returns FFFF for an absent entry without insertion. |
| TOP273+20..3F alternate bytes/offsets | `2271F0` copies two sixteen-byte vectors from the actual source secondary170/180. These are not aliases of the primary or control alphabet. |

The source attachment is now traced through module initialization, rather than
inferred from a similar layout. Text `002530` installs descriptor219
`2DF150`+20 into context18+38. That field is table `2DF0A0`, whose virtual80 is
`169740`, exactly `return source219+120`. The returned object is the actual
400-byte secondary. Frontend222 virtual250 is `1530F0`, returning the same
frontend+60 source created by `150D00`. `2271F0` obtains that secondary through
this exact chain before copying its alternate storage.

Fresh `163970` creates the secondary with `16A130`, which zeroes170..18F.
The supported raw ingress uses physical-point insertion and the native
`157C60` -> `156950`/`157340` selected-first-result path. That path transports
actual glyph alternatives directly into720-byte rows, not UTF8 strings through
the text insertion interfaces. Its raw reorder/split lineage starts before
recognized results and copies the same fresh source; no external/prior text is
accepted by the private owning factories. The alternate appender `159670` has
two native direct callers, `164EC0` and `165D60`: external existing-text input,
registered separately from raw ingress. Those operations are outside this
typed owner. Thus the source alternate vectors copied by2271F0 are empty in
this scope, even though the native copy itself is unconditional.

TOP virtual30=`13BE80` is a separate import/conversion operation. It copies
alternate bytes with `13B5A0` and walks glyphs through `13B350`/`139DC0`,
interning text using `1396F0`, changing primary kind0 to control kind2 and
run-kind1 to control kind3. Ordinary virtual50 does not perform this conversion.
The new owner has no import or source-text insertion API. General converted
owners must not be silently bound to this fresh-empty policy.

## Exact empty alternate behavior

`13C920` expands the glyph and passes **nonnull TOP+20** into `13A0B0`, even
when both vectors are empty. Therefore kind4 fails the native offset-count<2
check. The new emitter preserves this refusal. Passing `null` to the reusable
prepared helper would select primary-alphabet fallback, which is a different
native branch and would be incorrect for this actual TOP.

Nonzero kind3 newline/space/NBSP runs look up the actual empty control owner,
receive FFFF and emit the native replacement token. They do not look up those
strings in the acoustic alphabet. The unwritten kind3 subkind1 token remains
an explicit bounded refusal. An in-range primary label with no text entry also
fails, rather than receiving an invented replacement.

After token expansion, the emitter invokes the restored complete `0491A0`
canonical scratch/circled payload helper, using the checksum-checked portable
Unicode9 tables. A scratch without a successful circled replacement preserves
the original bytes. Marker handling therefore does not become unconditional
framework Unicode normalization. Malformed legacy five/six-byte UTF8 codecs
and allocator-failure replay remain outside the portable valid-UTF8 scope.

## Verification

`tools/LegacyMainGlyphUtf8Emission.Probe` runs three actual raw inputs,
including a reordered physical-stroke input. It compares all14 genuine emitted
pool records and1656 explicitly prepared descriptor controls against an
independent Python parser of the pinned raw acoustic alphabet. The controls
include all eight descriptor kinds, missing primary entries, synthetic labels,
run counts, empty runs, lookup-only control behavior and the native empty
alternate failure. It records263 bounded native/undefined refusals and checks
four ownership/cancellation guards. Prepared descriptors never mutate or
replace decoded glyph records.

Managed and Windows NativeAOT runs produce identical capture bytes. No expected
transcription is read. These real fixtures do not contain a successful paired
circled payload; the complete circled kernel's independent literal/arithmetic,
marker and caller-composition controls remain in `LegacyCircledUtf8.Probe`.
Neither byte parity nor those prepared controls demonstrate a handwritten
circled-symbol winner.

The static auditor checks both pinned PE hashes, ten code regions, the actual
module/source attachment,2A initialization/lookup, external insertion
registration and virtual30/50 separation. Vendor code is never loaded or
executed. The Ghidra skill is used only for static research and existing exports.
