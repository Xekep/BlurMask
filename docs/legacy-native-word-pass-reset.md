# Native search pass reset and trace storage

`LegacyNativeWordPassReset` restores `MyScriptText 180212d80` over the owned
56-byte heap descriptors prepared by `1fbb90`. Its two trace vectors implement
the reset/free decision and `180016a20` release order. Static inspection only;
no vendor DLL is loaded or executed. This is a pass-lifecycle component, not
the complete scheduler, result winner, or application Russian recognizer.

## Heap lifecycle

Each descriptor's active count at `00` is written to zero **before** calling
`1f73d0`. Phase zero takes its initial capacity from `20`; every nonzero phase
takes it from `2c`. Retained backing, allocation count, storage identity and
quota fields survive. There are no drop callbacks and no tail reinsertion,
because the active count has already been cleared.

If reallocation fails, the descriptor retains its new logical capacity, zero
active count, previous allocation and identity. Earlier descriptors remain
reset; later descriptors and both trace vectors remain untouched. A later
pass with enough retained storage performs no allocation. Reset never clears
the inactive 128-byte records or invents zero-valued native opaque tails.

The growth provider must supply every new byte and preserve previous storage.
`LegacyNativeWordHeapDescriptor.Resize` now checks cancellation immediately
after that external provider, including its failure return, before installing
the returned allocation. The prewritten logical fields remain observable.

## Trace-vector reset

For nonzero controller byte `2368`, the native routine processes controller
vectors `2370` and `2388` in that order. Each owns 16-byte entry headers, an
entry count and allocation capacity; `2380` / `2398` are separate used counters.

* With fewer than ten entries, only the two-byte entry field at `08` is reset.
  Every other entry byte, inactive header, entry count, capacity and vector
  allocation identity is retained. The used counter becomes zero.
* With at least ten entries, `016a20` releases each entry's allocation identity
  at `00` in ascending order, then the vector allocation. Null identities are
  not skipped. Only after these calls does `212d80` zero the vector identity,
  count/capacity and used counter.

`LegacyNativeWordTraceVector` owns byte snapshots and represents allocation
identities as opaque U64 data. The explicit release callback does not turn
those identities into managed addresses or dereference them. The trace-record
producer, its payload ownership and the allocator itself remain separate.

Checked budgets are 512 distinct heap descriptors and 4096 complete trace
headers per vector. Tracing requires exactly two separately owned vectors;
large vectors require a release callback before mutation starts. Missing
ownership, excessive capacities and reentrant descriptor/vector mutation are
refused rather than simulated. Cancellation checks bracket allocator/release
callbacks. Already performed releases/writes are not rolled back; callers must
stop the interrupted search rather than pretend its reset completed.

## Verification and composed source path

The independent Python snapshot oracle covers 512 complete resets, 773
allocation requests, 132 allocation failures and 1595 trace release calls.
Managed and Windows Native AOT compare full allocated byte buffers, all logical
fields and release ordering. Cases include both phase capacities, negative
nonzero phase values, the nine/ten boundary, inactive residue, zero capacity,
disabled tracing, and partial allocation failure. Additional controls check
foreign ownership, callback cancellation and repeated allocation reuse.

The existing heap-preparation probe also composes actual RU descriptor
`C0A`/weights `0,1`, fresh knowledge counts `2/2/1`, the general source bucket,
ten real neural/cache candidates and the explicit primary-budget-three quota.
Both subsequent pass resets retain the six-slot allocation and every opaque
byte, while clearing the old active results and restoring capacities four/six.
The frontend budget and final scheduling/winner remain explicit boundaries.

```powershell
python tools/LegacyWordPassReset.Probe/reference_controls.py .artifacts/pass-reset-controls.json
dotnet run --project tools/LegacyWordPassReset.Probe -- .artifacts/pass-reset-controls.json
```
