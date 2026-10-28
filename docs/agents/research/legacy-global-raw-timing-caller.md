# Native raw timing constructor boundary

STATIC ONLY follow-up to the ordinary geometry proposal. This closes the
admitted C1C constructor dispatch and distinguishes an actual clone from a
default constructor. It does not implement a timing API or a Ready2DB owner,
execute SDK DLLs, change current XY inputs, or prove higher-level iink/UI
ingestion defaults. Earlier frozen proposals remain unchanged.

## Dispatch proof

The additional read-only Ghidra exports cover12 complete function ranges:
Ink097C10/022BC0/090CF0 and Engine001D90/001F30/001F40/001160/037DD0/
038000/038010/038120/037E90. The audit preserves608 linear instructions,
all direct/indirect calls, full PE byte hashes, full body bounds, and15 table
bindings. Tiny leaf methods absent from the old bulk export were recovered
in an unsaved read-only Ghidra transaction, not by executing the DLL.

Engine type service1DB830 has V10=037DD0 (registered descriptor handle),
V20=037E90 (type-ID constructor lookup), V40=038000 (actual descriptor),
V48=038010 (constructor descriptor lookup), V78=038120 (allocation).
037DD0 returns registered row+10;038000 dereferences handle-10.
038010 reads actualDescriptor+18 metadata, then metadata+10 constructor
descriptor. It returns NULL when metadata is absent; it does not search for a
different timing owner. The type service pointer is also stored at1DB910.
This is a static dispatch proof for the registered native type, not execution
or complete global engine bootstrap certification.

C1C descriptor E7130+18 points to metadata E6FD0. E6FD0+10 points to
constructor descriptor E6FB8:

| Constructor lane | Actual value | Meaning |
| --- | --- | --- |
| +00 | NULL | no default constructor |
| +08 | 097C10 | clone from an existing C1C |
| +10 | 0978D0 | extended construction from caller descriptor/size |

Do not mistake097C10 for a default. It reads input object+28/+2c/+30/+34,
copies channel identities and layout, and preserves existing timing. It does
not assign a newly chosen samplerate.

Public voCreateObject at1F30 is a five-byte jump to1D90. The latter resolves
the actual type and constructor descriptor, requires nonnull lane0, allocates
and calls that lane. C1C's NULL lane therefore produces1040002 refusal;
there is no inferred default rate. voCreateObjectEx1F40 uses lane+10,
forwards caller descriptor and size unchanged to0978D0, and has its own
pointer/size/type guards. Size0 delegates1D90, which still fails onC1C.

0978D0, proven by the composed previous package, accepts descriptor size24
bytes, copies the caller channels and reads a FLOAT rate at descriptor+10.
It derives format+28 via float32 1e6/rate+.5 and CVTTSS2SI. XY-only removes
the time-channel reader, but does not remove or select a default samplerate.

## Raw C1E class factory and caller baseTag

C1E descriptor E6C60 is type2. Its descriptor+20 class-method table is
E6980, whose V0=090CF0 creates ordinary kind0 C49 and forwards caller's
fifth baseTag to08DDB0.08DDB0 stores it atC49+30 unchanged. This is distinct
from C1E metadata E6AD8, its constructor table E6CA8, and that table's base
constructor090C90. They cannot be substituted for E6980.V0.

Public voGetInterface1160 accepts type0 only; therefore this proof does NOT
claim that public voGetInterface(typeC1E) exposes the type2 class-method table.
The static class binding is the same native C1E factory used by the existing
raw-current-view proof. Any high-level caller wrapper assigning time values
is a separate producer requiring separate evidence.

Fresh C20 default creation/065940 append and textField/addItems INPUT operate
on an incoming raw object. They retain/copy that object's real data and create
INPUT selection/tag ownership; they do not retroactively select a C1C rate or
replace C49+30. Thus no rate/baseTag receipt can be recovered merely from the
present immutable library XY owner, current raw object index, INPUT ID, C51
counter or selected source range. Two explicit caller metadata descriptors can
create identical XY with different native time fields.

## Permitted future direction, still requiring approval

A separate NEW typed input receipt can accept explicit physical samplerate
and signed64 start/base time from a caller, construct an admitted XY-only
format using the proven native constructor branch semantics, retain those
parameters immutably with each real raw object, and expose validated temporal
sampling to the geometry owner. Its native provenance is "explicit physical
caller parameters throughC1CEx/C1E kind0", not "SDK automatic defaults".

The current XY constructor remains unchanged and produces no such receipt.
No synthetic timestamps, implicit constant rate, RawObjectIndex conversion,
post hoc zero baseTag, or caller assertion boolean can bridge existing XY
owners to this new type. A broad format/time-channel interface is outside the
initial proposal. C4A/C4B temporal sampling, signed conversions/overflow and
source bounds still require independent numerical controls and Managed/AOT
verification before a runtime timing API is accepted.

The current first geometry proposal remains an INCOMPLETE structural/
rotation/moments/vertical prefix. Closing explicit caller semantics does not
magically supply missing physical metadata for its existing13 rawXY owners.
No full horizontal geometry, actual2DB success,238B0/DOM publication or
recognition quality improvement is claimed.

```
python tools/handwriting/audit_global_raw_timing_caller.py REPORT.json --native-repo REPO --ordinary-plan-root FROZEN_ORDINARY_PLAN --capture global-word-text-managed13.json
```
