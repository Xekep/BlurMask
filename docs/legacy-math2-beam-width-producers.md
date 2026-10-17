# Math2 decoder beam-width producers

The pinned native path chooses a signed positive requested width, otherwise a signed positive saved EED width, otherwise `1`; the selected width must be at most `16`. This audit establishes the producers of those inputs. It does not implement native beam search or establish that the application's greedy decoder is equivalent to native width `1`.

## Ownership and configuration order

All addresses below are virtual addresses in the fingerprinted `MyScriptDocument.dll`, with image base `0x180000000`.

| Stage | Native evidence | Effect on beam width |
| --- | --- | --- |
| Fresh EED constructor | `044080`, zero fill at `044125..044136` | Clears owner bytes `0x68..0x1D7`, including saved DWORD `0x90`. Cold saved width is zero. |
| EED copy constructor | `044180`, copy at `04453E/044546` | Copies saved `0x90` and the existing decoder `0x220`. A clone can inherit configuration. |
| Raw EED initializer | `044630` | Requires a `0x188`-byte packet and copies packet `0x18..0x187` into owner `0x68..0x1D7`. Packet DWORD `0x40` supplies saved width. |
| Model configuration | `045460` -> `044FB0` -> `044AC0` | Visits model owners `0x1C0`, `0x1C8`, `0x1D0`, `0x170` in order. The `config` JSON callback handles `beam_size`; callback frame `0x68 + 0x28` writes owner `0x90` at `044DFD`. |
| Request configuration | `045460`, store at `045745` | Reads the request's `+0x10` configuration object and its `beam_size` property after model callbacks. |
| Width selection | `0450C0` | Reads request DWORD first, then saved owner `0x90`, then fallback `1`. Rejects selected widths above `16`. Successful decoder creation stores the selected width at `04537E` before later construction can fail. |
| Warm initialization | `045938..045980` | Existing decoder `0x220` skips `0450C0`; re-running this initializer is not evidence of a new selection. |

The EED factory descriptor is `0x1801FD090` (type `0xEED`, parent `0x133`); its construction table is `0x1801FD010` and initialization service is `0x1801FD0D8 + 8`. The EEE child field `+0x90` used by `03A260` is a statistics pointer and is a different owner from the EED saved-width DWORD.

## Concrete EEB request caller

EEB constructor `0277B0` initializes request DWORD `+0x38` to `0xFFFFFFFF` (signed `-1`) and configuration pointer `+0x48` to zero. Its worker-count field `+0x18 = 1` is a separate setting. Service `027970` clones its EED source and invokes EED initializer `045460` with `EEB + 0x38` as the request packet (`027AD9..027AE8`).

Consequently, fresh EEB construction alone does not prove width `1`: the source clone may retain a positive saved width or an existing decoder. The audit establishes this concrete request caller, not every external setter or caller of the source object. A null request packet is not the width fallback contract; `0450C0` reads the packet DWORD unconditionally.

## Actual resource metadata

The audit reads `models/math2/math2-unwrapped.res` as data, with SHA-256 `2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a`. It independently bounds and fingerprints all three ORTM model blobs, then reads `InferenceSession.Model.metadata_props` directly from their FlatBuffers. No ONNX Runtime session is created.

| Model slot | `config` JSON |
| --- | --- |
| Encoder | `{"model_id":"20260504","counting_pred_tolerance":5}` |
| Decoder initialization | `{"model_id":"20260504"}` |
| Decoder | `{"model_id":"20260504","num_heads":4,"need_force_nonobs_positions":true,"max_len_factor":4,"delta_cost_max":-1,"seg_type":"sigmoid"}` |

None contains a `beam_size` configuration key. Raw occurrences of the text `beam_size` inside decoder model blobs are recorded separately; tensor dimension names do not supply a configuration value. Serialized resource mode `0` selects a decoder family and does not establish the width. `num_heads = 4` is also a distinct setting.

For a source known to come from the cold EED constructor, fresh EEB request `-1`, no request configuration, and these three model metadata maps, width selection resolves to `1`. This conclusion excludes inherited, raw-initialized, externally configured, and already prepared EED sources.

## Reproduction and remaining decoder work

Run from the repository root:

```powershell
python tools/handwriting/audit_math2_beam_width_producers.py E:/BlurMask-build-artifacts-20261014/joined-math2-beam-producers-static.json
```

The script checks the complete PE fingerprint, nine producer regions, 45 instruction pins, 13 table bindings, literal keys, and all three complete model blobs. It also runs the existing decoder-route audit, which checks three additional regions, 19 instruction pins, and ten table bindings. The report embeds that dependency's findings. This is a static-only check; no application runtime or inference behavior changes.

Native `038610` retains up to `2 * beam` candidates and uses EF3 token gates, score pruning, and a complete-hypothesis path. The next ownership investigation is the actual EF3 construction and its slot `+0x10` constraints callbacks around `0392xx/0394xx`. Restoring those prerequisites and the native search behavior remains separate from proving the width inputs. This work makes no recognition-quality or complete-engine claim.
