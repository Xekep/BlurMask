# Native configuration anchor

The missing native assignment connecting the runtime frontend to its configuration is now identified. The mode-1 frame factory creates a fresh type-2B3 configuration, materializes type-222 runtime state and invokes configuration slot V10. Its target is the complete 16-byte function at RVA `00CAA0`, which writes the runtime into `config+48` and the same configuration into `runtime+8`. The previously considered `00CAB0` associates different fields and cannot substitute for this edge.

The frozen static packet pins the factory/caller bodies, interface tables and constructors. ROOT verified all 68 packet-file pins, the original DLL and full decompilation hashes, and the exact PE locations and complete bytes of the setter, base constructor and mode constructor. No vendor binary was executed.

This closes the pointer-binding prerequisite for the earlier ordinary group-kind analysis within the admitted fresh configuration route. The zero-configuration-byte conclusion excludes property import and custom session hooks. Property import function `00EBA0` invokes setters and must not be described as passive validation. Source-kind and special-symbol guards remain necessary; the result does not generalize to arbitrary native sessions.

No managed algorithm was changed by this finding, and handwriting accuracy was not remeasured. The Russian 3-of-9 result is unchanged. The next investigation concerns upstream geometry, context selection and neural-input normalization, including the distinction between primary-owner height and cross-source guide height.

Evidence under `E:/BlurMask-build-artifacts-20261014/`:

- `ru-native-M1-config-anchor-source-20261026/manifest.json`, SHA256 `38d4b1b7a25e0cb1a3c286a11e00980817037471a2b57c83ae12cc058d1eda81`.
- `root-ru-M1-static-admission-20261026/report.json`.
- `root-ru-M1-external-PE-admission-20261026/report.json`.
