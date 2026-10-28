# Actual2B1 BC280 landmarks

`LegacyDirectional2B1Landmarks.Extract` supplies the separately proved BC280
variant. Its point2 comparison uses point1 Y; the historical generic B6D20
helper remains unchanged. Inputs contain1..15 nonempty physical strokes and
at most4096 points. Coordinates are type-enforced signed shorts; the whole
point budget is checked before addition/allocation. Output flags and stroke
ends are owned snapshots. No stroke interpolation or character cuts are added.

641 independently generated controls cover signed-short extremes, plateaus,
singletons, multiple strokes and4096-point geometry. Additional analytic
reversal, generic-negative, empty/cancellation/budget checks pass. Root repeated
fresh Release/NativeAOT builds and all comparisons; both output logs match.
The [native helper review](directional-2b1-helper-pairs-20261021.md) separately
pins19 full bodies and the actual differing instructions. Numerical equivalence
of borrowed consumers is a manual supported-subset review, not a formal
full-program proof.

English MAIN N now uses this helper. Its shared downstream consumers remain
explicitly limited to1..255 prepared samples. Independent full feature/NN/cache
checks and Managed/NativeAOT parity pass on134 original owners and100 recorded
owners. The original134 capture remains `1eeff7449c49e7056b0f61ed71f6f89b83df03fef641def987cf63b225853af0`;
the100-owner capture is `38396269d1fa4f2bf91d54e7994a04e3b8fee8915e417fc621e7530237e0066c`.
In the latter184/11330 directional groups change, affecting13097 probability
entries; trajectory/raster inputs, source720 and graph data remain unchanged.

Fresh D0 pool/map/permission checks also pass on both sets. Managed/NativeAOT
D0 captures are `ab6d823c199586147d43ce01f49d22a1c2605a58952cec28455aea71971b739f`
and `f7b4fb9dd22d4f6170ea9be3585cc117841cda09df357b3dcd91c9d2eefb9a2f`.
These are numerical component checks, not a whole-word decoder or better word
accuracy. The default English CTC heuristic frame is separate. Russian V3
integration and its full post-inference evaluation are still pending.

Frozen source/evidence manifests on E are
`review-directional-2b1-landmarks-runtime-frozen-manifest-20261021.json`
(`c1dd5b06a6154802ab8ccf459c67f08d067c84358cd575c9fe536fdbde0c3e25`) and
`root-english-main119-bc280-frozen-manifest-20261021.json`
(`e1aa776464abeaf889838c4d554aa210c2fd67ac183a3e4260cb300d0abccde9`).
Vendor SDK code was never loaded or executed.
