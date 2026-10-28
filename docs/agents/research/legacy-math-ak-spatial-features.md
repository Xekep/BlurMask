# Source-bound mathematical spatial features

`LegacyMathAkSourceGeometry.PrepareUnguidedSingleStrokeSpatial` prepares the original configured `1211` spatial input: seven global values, three baseline values, 192 spatial values and two whole-source boundary values, for 204 shorts in total. It accepts the same explicit one-whole-stroke, absent-guide subset as the temporal input stage. Degenerate, unsupported and oversized inputs are refused. No model is evaluated and no candidate authority is created.

The raster uses the original normalized coordinates, not the 30 temporal samples. It preserves the native line-step tie rule, unique pixels, crossing counts, eight directional distances, overlapping diagonal projection at index 127, signed-short storage before clamping and the actual resource calibration. Output arrays are detached and feature ownership requires the same geometry instance and its private feature token. The existing token is reused without adding another token field to every geometry.

The source input is at most 4,096 points. Fixed raster capacity is 16,384 pixels. The dense prepared source reaches 14,641: whole-source normalization maps both nonzero extents to 8,192, then integer scaling gives maximum coordinate 120, covering a 121 by 121 grid. These are distinct bounds. The explicit new-loop bound is 563,520 iterations, excluding composed temporal work; the conservative array-payload bound is 175,812 bytes, including temporal scratch but excluding CLR object metadata and the already-owned source. Neither is an RSS or latency guarantee.

Independent prepared checks cover 13 vectors, all 2,652 feature values, four ownership/cancellation/degeneracy guards, six short-before-clamp controls and 11 corrupted captures. A data-only native audit authenticates ten complete PE regions, the actual AK program/model metadata and 32,258 adjacent-index controls. The original failed endpoint assertion and a Python whitespace-error draft remain preserved outside the repository; the canonical source packet includes the explicit script correction. No vendor code or neural network was executed for this stage.

The selected `13A4` base-tail source's `13A5` owns **five** acoustic networks: temporal `1F222`, spatial `2F204`, and three further feature families `3F279`, `4F279`, `5F223`. The raw resource also contains a separate first-vector `3F276` owned through another `13A3` source; it must not be included in the selected five-network combination. The existing isolated 1F factory temporarily loads the six-model raw catalog, which does not establish six-model classifier ownership. Further integration needs one immutable shared weight owner, the other three feature families, native weighted combination and alphabet expansion. Spatial features alone do not complete mathematical recognition.

Canonical packet SHA-256: `9068c27e6a3201fc9f81b3f7a3e37fe1310703a18856278aca1d614d05d4c6e2`. Root independently checked its predecessor packet and corrected native audit against the original PE bytes. Prepared commands:

```sh
dotnet run --project tools/LegacyMathAkSpatial.Probe -c Release -- . capture.json
python -I -B tools/LegacyMathAkSpatial.Probe/audit_output.py . capture.json report.json
```
