# Legacy accepted-result source-line publication

This stage recovers the actual legacy result consumer, rather than identifying a glyph rectangle with a font metric. In pinned MyScriptText.dll (`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`), `0283b0` takes the result-vector record's `+10` legacy273 object, gets its frontend222 at `+8`, and calls `021ab0`. That helper obtains the frontend's actual source through virtual250 and interface218. This path does not require an unproven old247/modernEED resource adapter.

`021ab0` counts `max(sourceLineId+1)` across source218.virtual10 input descriptors whose low flag nibble is not3 and whose `+10` source-line ID is nonnegative. It appends that many zero-initialized56-byte records to source2DA's F8 array, but writes prepared records at the result wrapper's existing `+68` line offset. These are distinct counters. A null virtual40 guide leaves its allocated record unchanged. The wrapper offset advances by the inferred count even if some guides are null.

For each present guide, the helper calls source218.virtual100 (`1692b0`) with no external transform. Its mixed32-byte output carries double angle, float offset, float height, double originX and double originY. The guide's corrected height is selected only when strictly positive; that branch uses corrected baseline and zero slope. Otherwise it uses base baseline, slope and height. The source's actual rotation at secondary+A8 rotates the baseline intercept and adds to `atan2(slope,1)`. The output offset uses `originY-originX*angle`: the multiplication at169456 is by the retained angle, **not its tangent**.

`021ab0` stores the output height, promoted to double, at F8 record+20, origin coordinates at+8/+10 and angle at+18. It writes owner tag−1 at+28 and the original guide baseline, separately, at+30. This is the concrete owner-bound legacy source-guide→source-line-metric producer. Existing `LegacyPreparedResultLineMetrics` then ports050660's source2DA→result2DB owner/range binding and Document177590's narrowing to SDK lineInfo float+18. Published missing-guide records have zero metrics; neither stage invents an accepted fit.

`LegacyNativeSourceLinePublication` ports this null-external-transform, successful-allocation branch with owned portable records, input/line budgets, cancellation and finite-field validation. It requires actual prepared descriptors, guides and rotation. It does not manufacture IDs, suppress inactive line holes, globally sort IDs, supply an accepted winner, or implement the external affine-transform branch. The library throws on an unrepresentable scalar output instead of publishing native error-path uninitialized stack data.

This proves metric ownership for the **legacy** bridge. It does not mean that every other source-frame height is a Document x-height. The complete old273 winner materializer, its character source-line mapping, modern result adapter, and the application's connected CTC-word metadata are separate unfinished boundaries. Passing an explicitly prepared guide does not constitute raw handwriting recognition or a live application font-size fix.

The old row producer distinguishes original guide geometry and acoustic-frame geometry. `16e850` first copies initial guide+0/+4 into+C/+10. Its later `16e570` receives the **base** guide pointer and modifies+0/+4 during deskew; it does not copy those values back into+C/+10. Thus the null-transform publisher's fallback reads the pre-deskew estimate baseline/height, while the acoustic-frame path reads the separately deskewed fields. Guide slope+8 survives both operations. No late copy-back has been established, and no adapter replaces these fields with `Frame.Line`.

Reproduce without GUI or vendor execution:

```powershell
python tools/LegacySourceLinePublication.Probe/reference_controls.py D:/BlurMask-build-artifacts-20261011/source-line-publication-controls.json
dotnet run --project tools/LegacySourceLinePublication.Probe -c Release -- D:/BlurMask-build-artifacts-20261011/source-line-publication-controls.json
dotnet publish tools/LegacySourceLinePublication.Probe -c Release -r win-x64 -p:PublishAot=true -o D:/BlurMask-build-artifacts-20261011/source-line-publication-aot
& D:/BlurMask-build-artifacts-20261011/source-line-publication-aot/LegacySourceLinePublication.Probe.exe D:/BlurMask-build-artifacts-20261011/source-line-publication-controls.json
```

The original Python oracle supplies1164 mixed scalar cases and512 descriptor/append cases, including missing guides, inactive large IDs, existing records and overwritten wrapper offsets. Additional managed/native controls cover256 SDK/font-policy compositions, corrected-height threshold neighbors, an angle/tangent discriminator, ownership, budgets and cancellation. Trigonometric results allow a small cross-libm tolerance; source heights and ownership fields are checked exactly. No samples, labels or model confidence are fitted to observed recognition output.
