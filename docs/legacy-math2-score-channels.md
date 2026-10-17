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
03B300 initializer retains the identical decoder reference at its input A8 in EEF B8 under the default Engine service; see the [concrete retention proof](legacy-math2-force-policy-producer.md).
The offsets therefore name the same EED168 decoder:

```text
0x70 + 0x50 + 0xA8 = 0x168
```

The V30 call through EEE80 targets EEF03CAC0. That method reads EEF B8 and invokes its
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
PE fingerprints,8 code regions,96 exact instructions and10 table bindings.
It includes the EED-to-EEE packet arithmetic, the EEF same-pointer decoder retention, float32
temperature/exp/sum/log/store calls and the separate log/cost output channels.
The full beam, token-mask producers and final structural acceptance remain
separate work.

## Default completed-hypothesis score

037430 appends a completed candidate and writes its normalized score. Without
an active constraint model the exponent is1. The prefix count excludes EOS;
the cumulative candidate cost includes EOS. The native operations are:

```text
base = float32(float32(prefixCount + 1) + 5) / float32(6)
score = float32(cumulativeCost / base)
```

Both CPU-dispatch branches of native powf return the positive finite input
base unchanged for exponent1. The static audit checks their exact branches,
the default exponent, optional constraint-model override and the5/6 literals.
An arbitrary double-precision combined division does not preserve all bits.

`LegacyPreparedMathCompletionScore` implements only this explicitly prepared
no-constraint numeric kernel, with a portable count bound of one million.
`WritePreparedRecord` changes just the first float of a caller-prepared
168-byte snapshot; it does not construct the rest of the native record or
claim native allocation/failure lifetimes. The ordering probe checks4096
scores with an independent float32 oracle and then sorts the complete scored
records through the separately restored036810 ordering kernel. The prior181
ordering controls remain. Native tensor decoding, beam expansion, configured
constraint exponents and acceptance are still unported.
