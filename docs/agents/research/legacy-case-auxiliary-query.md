# Case-wrapper auxiliary32 branch

Wrapper292.V48=231AE0 is distinct from V40=233090 normal case expansion and
V58=233FC0's24-byte continuation. It forwards inner V48, then copies only
request bits58..62 to every32-byte record's byte7, preserving bits0,1,7.
It does not rewrite labels, context paths, prices or other fields.

The checked2B7.V48=093F50 applies its existing terminal-only query. This is a
nonempty32-byte source; it must never be replaced by the separately proved
empty2B7.V58 backend. All32 transported case states, including31, are valid
here because231AE0 performs no policy-table lookup.

`LegacyNativeCaseAuxiliaryQuerySource` binds that actual dictionary and same
symbolic context owner to a typed ordinary graph. It validates complete record
and optional side-cost extents, performs the exact masked write and preserves
negative native return values. Cancellation and failed-session checks match
the normal owner. It still accepts already prepared dictionary maps; current
controller permissions and aggregate forwarding of the full query tag remain
separate producers.

The actual-resource probe first derives092990's real auxiliary map, which
enables only joiner184/FFFB; all32 root queries correctly emit zero records.
Separately, a deliberately broad supplied auxiliary map stress-tests terminal
records through seven-record pages for every transported state. That stress
map is explicitly **not native auxiliary60**. The independent consumer checks
every source, context and price byte. The static auditor pins292.V48,2B7.V48 and the native
five-bit operation without loading a vendor DLL. These are source-stream
checks, not Russian handwriting accuracy or native-winner claims.

```powershell
LegacyCaseAuxiliaryQuery.Probe AK.res LK.res engine-profile.json captures.json
python tools/LegacyCaseAuxiliaryQuery.Probe/audit_output.py captures.json
python tools/handwriting/audit_legacy_case_auxiliary_query.py static.json
```
