# Legacy Russian stored word language tables

`LegacyWordLanguageModel` is a bounded read-only loader for the inspected
type2d1/version101, base-version100, order2, flags3 storage. It restores raw
stored word/class cost lookup, not the complete language decoder or recognition.
Word IDs must already correspond to the caller's resource. The loader does not
infer a dictionary state's word ID or reinterpret a Unicode string as one.

The checked RU LK object occupies bytes 2513123..4565174 (2,052,051 bytes). Its
439,826 word slots equal the main dictionary's terminal count. A 10-bit packed
map associates words with 1,024 classes. Each word has a byte level code and a
256-entry integer cost table. Class unigrams use 1,024 byte codes; bigrams use a
dense 1,024-by-1,024 code matrix with 256 integer levels per order. The declared
bigram count is 681,695, distinct from the dense matrix's storage length.
The object also stores profile calibration and 13-category punctuation data.

Native static evidence is the type2d1 reader `1800fcc10`, base reader
`1800f0310` and inherited metadata reader `1801d2680`. The loader consumes every
byte of this checked profile, including the known-size uninterpreted calibration
and punctuation tail. It validates dimensions, packed class IDs and level-code
ranges. Unsupported flags/orders, truncated data, trailing data and nonzero
metadata identity requirements fail closed. No activation, authentication or
engine code runs. The source resource is now stored under
[`models/ru-RU`](../../../models/ru-RU); this component reads its tables as data.

`GetWordClass`, `GetStoredWordCost` and `GetStoredClassCost` expose uncalibrated
stored integer values. Dense class lookup uses `previousClass * ClassCount +
currentClass`, or the unigram table without preceding context. That orientation
is supported by native `180100090`, which selects a previous-class row and then
the current-class code. `1800fe2a0` converts stored levels into profile-dependent
integer costs; that calibration, history/category handling and word-ID
correspondence are not implemented by these raw methods.

This distinction also matters for acoustic recognition. Native `1801be930` and
`1801bf0a0` preserve candidate integer costs through `1801bc900` and `1801bc680`
before `1801bcbd0` calculates a separate calibrated acoustic confidence.
`1801be1a0` uses confidence for pruning, and `1801be530` normalizes exponentials
of the pre-existing costs for up to 20 candidates. Therefore the original
confidence-to-beam adapter in the
[directional word diagnostic](legacy-russian-directional-words.md) cannot claim
native word-level cost equivalence.

Vendor-free tests construct an original model with five classes and 41 words:
packed values crossing 32-bit boundaries, independently computed word/unigram/
bigram lookup values, previous/current orientation, invalid IDs, malformed
headers, out-of-table codes, many truncations and cancellation are checked.

```powershell
dotnet run --project tools/LegacyWordLanguage.Probe -c Release
```

With a locally supplied inspected LK resource, optional independent controls
verify 512 raw lookups. Generated resource-derived controls must remain private:

```powershell
python tools/LegacyWordLanguage.Probe/reference_controls.py local-lk.res .artifacts/private-word-language-controls.json
dotnet run --project tools/LegacyWordLanguage.Probe -c Release -- local-lk.res .artifacts/private-word-language-controls.json
```

Managed .NET and Windows Native AOT pass the same vendor-free and 512 actual
reference checks. Retained raw lookup storage is 2,042,282 bytes, excluding the
caller resource buffer, validation temporaries and runtime. This is not a
measurement of application memory. Dictionary word-ID alignment and native
profile weighting must be proven before these costs can rank handwritten words;
connected-stroke segmentation and real Russian cursive accuracy remain open.
