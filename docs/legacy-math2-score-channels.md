# Math2 score channels and the intermediate EEF owner

This is static engine analysis. It does not replace the app's greedy decoder
or improve the existing14-expression result of10/14. No SDK DLL or ORT model
is executed by this audit.

## The concrete callback route

EEE's owner80 is **EEF**, not EF1. Treating its V30 call as the EF1 V30 state
callback would connect different functions and different storage owners.

For EED's checked mode0 branch,0450C0 creates EF1 and retains it at EED168.
It initializes EEE with the0x108-byte packet starting at EED70. EEE0359F0 then
initializes EEF from that packet's0x50-byte offset, with a0xB8-byte packet. EEF's
03B300 initializer clones the decoder reference at its input A8 into EEF B8.
The offsets therefore name the same EED168 decoder:

```text
0x70 + 0x50 + 0xA8 = 0x168
```

EEE's actual V30 target is EEF03CAC0. That method reads EEF B8 and invokes its
V28, which is EF1's04B9E0 network-output callback in mode0. EF1 V30=04C930
instead prepares/reindexes state tensors. The EEF factory, primary table and
raw initializer are distinct; the audit pins all three rather than confusing
the factory's size callback with its initializer.

## Log probabilities and cumulative costs

EEF03CAC0 passes the decoder output to03C0D0. This calls Engine service54.V48,
whose registered target is186290. The input axes are time0, batch1 and
vocabulary2; time index-1 selects the final time frame. The inverse temperature
is computed with DIVSS from the EEF20 field.

For each batch row,186290 finds the maximum raw value, accumulates scaled
exponentials in float32 order and applies a float32 logarithm. Each output is
stored with the native multiplication/subtraction order.03C0D0 copies those
log-probability bits into EEF D8 and flips their sign bits in EEF E8. EEF then
adds the prior per-beam F0 cost to the cost channel before candidate selection.
The log-probability and cumulative-cost buffers are separate owners, even
though both originate from the same output tensor.

The exp/log calls reach the engine's own CPU-dispatched implementations at
1C45D0/1C41D0. Replacing these with arbitrary double-precision softmax calls
does not establish exact native float behavior. Their implementation and the
remaining EEF tensor, alias and mask lifecycle are not restored by this audit.
Conditional token probabilities are not calibrated handwriting confidence.

`tools/handwriting/audit_math2_score_channels.py report.json` checks both whole
PE fingerprints,6 code regions,59 exact instructions and10 table bindings.
It includes the EED-to-EEE packet arithmetic, the EEF decoder clone, float32
temperature/exp/sum/log/store calls and the separate log/cost output channels.
The full beam, token-mask producers and final structural acceptance remain
separate work.
