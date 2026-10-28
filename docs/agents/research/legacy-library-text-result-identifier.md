# Library-owned text result identifier

The restored native algorithm is Engine model62.V0=`092AC0`: two SplitMix64
draws with wrapping64-bit state, stored little-endian into raw16, followed by
UUID version4 at byte6 and RFC variant bits at byte8. V8=`092B80` formats those
raw bytes directly as lowercase8-4-4-4-12 text. Default .NET Guid field-byte
reversal is not used.

Native032550 replaces result38..47 before WORD publication, retrying if the
new raw16 equals the previous identifier. NEW2DA01C010 initializes that previous
field to zero. These bytes are a result identifier, not a bounding box. The
native formatter is subsequently used for the textResult identifier attribute.

`LegacyLibraryUuidGenerator` restores generation, state wrap, version/variant
and raw-order formatting. Its initial state is deliberately a portability
adaptation: eight bytes from cross-platform `RandomNumberGenerator`. Native
092CF0 instead mixes process/object addresses, clock/counter and thread state.
Those nondeterministic native addresses are neither guessed nor represented as
zero. The library's owner is explicitly named and is not claimed to reproduce
the native initializer or its process-global sequence.

The actual selected-input2DA seed retains `LibraryIdentifier38` and its exact
generator owner before any WORD/string publication. It generates a distinct
identifier from the constructor's proven initial zero field. Publication
preserves that same identifier object and raw16 snapshot. The existing assigned
seed20/WORD/CHAR/source records and recognized labels remain unchanged. The
identifier has not by itself created DOM nodes, applied selections, invoked
BE2 or completed0283B0.

Prepared-seed controls independently verify1024 native numeric sequences,
state wrap, raw16, lowercase text and128 forced identical-previous-ID retries.
Three genuine inference/source publications verify the result-owned library
identifier before and after WORD publication. Their entropy intentionally
differs across Managed and Native runs; Python independently recomputes each
live identifier from its captured test-only initial state. Only the prepared
capture projection is expected to be byte-identical across runtimes.

```powershell
python tools/LegacyTextResultIdentifier.Probe/reference_controls.py E:/BlurMask-build-artifacts-20261014/result-uuid-controls.json
python tools/LegacyTextResultIdentifier.Probe/run.py <managed-dll-or-native-exe> E:/BlurMask-build-artifacts-20261014/result-uuid-controls.json <capture.json>
python tools/handwriting/audit_text_result_identifier.py <static.json>
```
