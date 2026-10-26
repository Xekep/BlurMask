# Completed native selected text

The completed GLOBAL adapter now labels checked selected text explicitly with `TextRecognition.NativeSelectedText`. The original candidate score remains available and is not treated as a probability of correct recognition. The property has a public getter and an internal initializer. Historical raw/FIRST results and failed or empty results do not acquire this label.

The adapter accepts ordinary finite candidate channels in the native numeric domain [0,1], preserving the existing token, source-owner and full original-ink coverage checks. The previous local 0.5 cutoff was not established as a native selected-text export rule. Sentinel and out-of-domain values remain rejected. Application layout retains positioned word metadata and checks its bounds, ranges and coverage independently. Native selected text has a separate admission mode with no confidence value for probability-based acceptance. Ordinary OCR and mathematical category thresholds remain in place.

Automatic replacement separately retains the pre-existing local candidate-score threshold of 0.5. This is a product policy, not a native export rule or a probability claim. Lower-score native results remain available from the reusable library, while the application keeps the original handwriting. Headless checks cover the low-score refusal and inclusive threshold, strong candidates and all three automatic text paths.

Validation: the real admission predicate passed 21 synthetic domain/token cases against both the previous and corrected libraries. Opposite-mode controls fail on the expected low-score case. The application compiled; positioned-word layout, automatic layout and mathematical layout checks passed without starting its GUI.

On the original nine coordinate-only recordings and 27 additional fixed recordings, 31 of 36 requests returned selected text; only 17 words matched their labels exactly. Five requests timed out. These recordings lack physical pen-up boundaries; writer overlap with model training is unknown. Targets were read after inference, and Unicode comparisons were literal. This is regression evidence, not representative accuracy or completed Russian cursive support. Removing the output cutoff does not fix incorrect word selection. No release-readiness claim follows.

The subsequent matching-row bound removes an independently reproduced false refusal for the 389-point long-word input. Its original preflight charged all 3,105 global rows per source; the actual refresh kernel can append only matching rows from that source interval. All choices and existing limits are retained. Sixty-four kernel cases verify descriptor deduplication and two cases verify callback-failure prefixes. The same long word completes correctly in a separate diagnostic run with a 30-second deadline. Public two-second runs still time out; a repeated 36-request run returned 22 results with 14 literal matches and 14 timeouts. These timing-dependent runs are reported separately, without an accuracy or latency improvement claim.

The current central source-completion route is retained. The first proposed patch used an older adapter preimage and was refused before compilation; the accepted patch transfers only the admission changes onto the current route.

Focused check:

```powershell
dotnet run --project tools/NativeSelectedTextAdmission.Probe -c Release -- tools/NativeSelectedTextAdmission.Probe/fake-admission.json after
```
