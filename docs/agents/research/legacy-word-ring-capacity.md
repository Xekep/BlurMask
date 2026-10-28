# Prepared main word storage capacity

Text `1FA300` grows storage arrays and allocates/aliases node rows. It is not
the final recognition winner selector. `LegacyNativeWordRingCapacity` restores
its signed scalar policy for already resolved container fields50,54,58,68:

`min(max(field50,field58,field68)+field54, max(field68,field50+field58))`.

Both additions wrap as native signed32 arithmetic. A positive requested logical
extent grows to a multiple of40. For a fresh allocation, the number of distinct
physical rows is the smaller of the rounded extent and retention distance.
Later logical rows alias the row at index minus that distance. The prepared
plan reports those identities; it does not copy bytes to imitate aliasing.

The four fields are explicit resolved inputs. Fresh container00C860 initializes
them to minus one; that constructor alone cannot supply a usable ring. The
prepared plan refuses nonpositive distance and caps requested nodes at1,000,000
as a portable implementation budget. This does not emulate native allocator
failure or repair unresolved container settings.

The independent Python oracle retains the native conditional branch sequence
and builds identities by following previous-slot references. It checks positive,
negative, overflow and rounding boundaries against the simplified C# policy.
Managed and Native AOT execute the same controls. The separate static auditor
checks pinned PE/decompiled text without executing vendor code.

```
python tools/LegacyWordRingCapacity.Probe/reference_controls.py controls.json
dotnet run --project tools/LegacyWordRingCapacity.Probe -c Release -- controls.json
python tools/handwriting/audit_word_ring_capacity.py static-report.json
```

Grouped native heap ownership, existing-row migration on growth, tracing and
alternative buffers, configuration resolution and final winner publication
remain separate. This arithmetic stage does not enable application recognition.
