# Native initial language cache bound

`LegacyWordInitialLanguageBound.GetNativeInitialLanguageBound` restores the
initial cache value from native `1800fe150` for the checked order2/flags3
language storage with a word codebook and no backoff table. It takes the
explicit model, scalar calibration profile and positive runtime scale.

The native initialization sets cache `+0x2590` to `0x4000000`. It reads the
first calibrated word codebook level from cache `+0x08`, and the first
calibrated unigram and bigram codebook levels from cache `+0x10`. For each
order it adds the word and class values, then keeps the minimum, capped by
the initial sentinel:

```text
initialBound = min(0x4000000,
                   firstWordCost + firstUnigramCost,
                   firstWordCost + firstBigramCost)
```

The raw resource's backoff pointer `+0x90` is absent in this checked profile;
the alternative backoff contribution remains impossible. Other encodings
are unsupported by the existing language resource loader.

Getter `180100070` returns cache `+0x2590` when the model order is below 3;
otherwise it returns `-0x4000000`. The ordinary word beam `180204290` uses
this cache value in an early path comparison with its current heap limit.
No category or boundary contribution is included in this initial cache
value. Native codebook ordering and calibration assumptions do not make
this a universal mathematical lower bound for arbitrary caller coefficients
or deliberately unordered codebooks. The portable fixed-sequence decoder
does not automatically use it for pruning.

The two new read-only language model accessors expose first **stored
codebook levels**, not dictionary word ID 0, class ID 0, or a scan of all
words. Portable checked addition rejects signed overflow rather than
wrapping native integers.

## Checks

The own resource deliberately maps word/class ID 0 to codebook index 1,
proving the distinction from initial codebook index 0. Other controls cover
calibrated composition, sentinel capping, overflow, negative caller
coefficients and cancellation. Python independently generates 640
binary32 arithmetic controls.

```powershell
python tools/LegacyWordBound.Probe/reference_controls.py .artifacts/word-bound-controls.json
dotnet run --project tools/LegacyWordBound.Probe -- --controls .artifacts/word-bound-controls.json
dotnet run --project tools/LegacyWordBound.Probe -- models/ru-RU/ru_RU-lk-text.lite.res
```

Actual Russian stored initial levels are word 193, unigram 20663 and bigram
3567. At explicit scale 1, the four native initial bounds are `-31264`,
`-10437`, `-10437` and `-9521`; category and boundary are excluded. Profile
2 is the verified [stored-source word profile](legacy-russian-word-source-profile.md).

Managed and Windows Native AOT passed the own checks, 640 independent
controls and four actual-profile controls. No vendor engine is loaded;
these checks concern the restored elementary cache arithmetic and do not
test complete native search or handwriting accuracy.
