# Completed native selected text

The completed GLOBAL adapter now labels checked selected text explicitly with `TextRecognition.NativeSelectedText`. The original candidate score remains available and is not treated as a probability of correct recognition. The property has a public getter and an internal initializer. Historical raw/FIRST results and failed or empty results do not acquire this label.

The adapter accepts ordinary finite candidate channels in the native numeric domain [0,1], preserving the existing token, source-owner and full original-ink coverage checks. The previous local 0.5 cutoff was not established as a native selected-text export rule. Sentinel and out-of-domain values remain rejected. Application layout retains positioned word metadata and checks its bounds, ranges and coverage independently. Native selected text has a separate admission mode with no confidence value for probability-based acceptance. Ordinary OCR and mathematical category thresholds remain in place.

Validation: the real admission predicate passed 21 synthetic domain/token cases against both the previous and corrected libraries. Opposite-mode controls fail on the expected low-score case. The application compiled; positioned-word layout, automatic layout and mathematical layout checks passed without starting its GUI.

On the original nine coordinate-only recordings and 27 additional fixed recordings, 31 of 36 requests returned selected text; only 17 words matched their labels exactly. Five requests timed out. These recordings lack physical pen-up boundaries; writer overlap with model training is unknown. Targets were read after inference, and Unicode comparisons were literal. This is regression evidence, not representative accuracy or completed Russian cursive support. Removing the output cutoff does not fix incorrect word selection. No release-readiness claim follows.

The current central source-completion route is retained. The first proposed patch used an older adapter preimage and was refused before compilation; the accepted patch transfers only the admission changes onto the current route.

Focused check:

```powershell
dotnet run --project tools/NativeSelectedTextAdmission.Probe -c Release -- tools/NativeSelectedTextAdmission.Probe/fake-admission.json after
```
