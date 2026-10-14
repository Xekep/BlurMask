# Ordinary auxiliary word-query pages and terminal injection

`LegacyNativeAuxiliaryWordQueryPages` connects the204290 reused request at1648
to the prepared aggregate source's auxiliary reader and its current-terminal
injection. It consumes an actual selected row88, prepared shared context
container/capacity/output allocation and live policy fields. Parent selection,
source refresh, associated2300 category bypass and downstream word scheduling
remain separate. It does not publish a winner or enable Russian AI-pero.

## Verified producer and dispatch

The request stores current tag00, parent-context10=0, category18=0, owner1C equal
to the ending node and the **sign-extended stored char20** at request20. Skip38
starts0. Request24 is4 only when254C and inputflags10 are both set; request26
retains the actual input flags. The shared context08 and output capacity3C come
from constructor preparation, rather than a guessed pointer or new allocation.

204290 initializes local310 inputflags to0. Non-special frontend280=false keeps
that zero; passing other flags in this scope is refused. Frontend280=true uses
its separately obtained graph-input flags; `(flags&3)!=0` selects the limited
mode. Frontend280=false uses2601. In limited mode, remaining byte19 is2602 minus
current26 when positive, otherwise0. This port receives the actual getter values
explicitly; it does not recover a missing graph-kind/input-descriptor owner.

Static vtable/disassembly confirms aggregate28D table1802E8BB0 slot48 points to
236B40: `mov r9d,1; jmp236910`. Dictionary2B7 table1802B4360 slot48 points to
093F50: `mov r9b,1; jmp093B50`. These are the auxiliary forms of the already
restored aggregate and checked packed-source readers. Context factory and child
dictionary identity are retained by those prepared owners.

For the supported no-associated-bypass branch,2600 or current06.bit0 suppresses
the source read. Otherwise each iteration invokes the actual auxiliary reader.
Accepted skip advances only by the source returned count. It never includes the
current-terminal injection. The complete returned32-byte records keep their
native page order and aggregate source-ID stamping.

## One-time current-terminal injection

The pending predicate is current05.bit1 set and current07.bit0 clear. After any
page with count below capacity, the next slot receives current00..07, zeros at
08..13h, current ushort24h at slot14h and zeros at16..17h. Extra bytes18..1Fh
remain the exact output-allocation residue. This reuses the checked existing
`CreateAlreadyTerminalQuery` producer. A full page delays injection until a
later slot is available; injection occurs once. A subsequent zero total count
ends this current-row query loop.

The output buffer persists across pages, just as the prepared allocation does.
Public page records and full buffer snapshots are owned copies made before the
consumer callback. The current row and initial allocation are copied before
source callbacks. Consumers can feed these records into the terminal/history
language path while retaining the original current row and ordinal ownership.

Negative returned counts are refused: native-1 with a pending terminal can reach
a negative output-buffer slot. The port does not simulate unsafe pointer writes.
Iteration limits are an explicit resource bound, not an inferred native winner
limit. Callback cancellation is checked immediately. Failure/rejection requires
a fresh session; calls cannot run concurrently or reenter the same session.

## Actual Russian-resource fact and independent checks

The actual initial092990 auxiliary60 map has two non-FFFF entries: joiner184 and
controlFFFB (lexical joiner0 and space68). Scanning all895571 packed graph nodes
finds17 non-root edges with those labels, and **zero terminal edges** among them.
The root's label0 sentinel is excluded from the edge count. For this exact flags0
dictionary and initial map,093B50's auxiliary-terminal gate therefore produces
no records at every node. This fact is not generalized to foreign resources or
decorators and does not itself authorize skipping their queries.

The real-resource probe binds092990 maps, shared source68 contexts, the selected
packed dictionary and aggregate child. Thirty-two actual dictionary terminal
rows produce zero auxiliary pages, then exactly one current-terminal injection
and the native zero-count stop. Their complete tag and allocation-residue bytes
are checked. These are dictionary-owned rows, not a handwriting-quality corpus.

The independent original Python oracle checks512 synthetic caller streams,
1606 exact requests and1266 full-buffer pages. It covers full-page delayed
injection, partial pages, injection excluded from skip, special/non-special
frontend mode, byte-wrap/sign-extension, source/row bypass and consumer abort.
Eighteen guards cover extents, flags ownership, cancellation, negative counts,
iteration exhaustion, failed reuse, reentrancy and input snapshots.

```powershell
python tools/LegacyAuxiliaryWordQuery.Probe/reference_controls.py D:/BlurMask-build-artifacts-20261011/auxiliary-word-controls.json
dotnet run --project tools/LegacyAuxiliaryWordQuery.Probe -c Release --artifacts-path D:/BlurMask-build-artifacts-20261011/auxiliary-word-managed-artifacts -- D:/BlurMask-build-artifacts-20261011/auxiliary-word-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res
```

Managed and Windows Native AOT use the same controls. Vendor DLLs are inspected
statically and never loaded or executed by these checks.
